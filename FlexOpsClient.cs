// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-04
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexOps.Sdk;

/// <summary>
/// Thin HTTP client for the FlexOps multi-carrier shipping platform API.
/// Provides typed request/response wrappers around the Gateway REST endpoints.
/// </summary>
/// <remarks>
/// <para>
/// The client does not retry. Every POST and PUT carries an <c>Idempotency-Key</c> header: the caller's key
/// when one is passed, otherwise a new GUID for that call. A retrying handler in the <see cref="HttpClient"/>
/// pipeline (Polly, <c>Microsoft.Extensions.Http.Resilience</c>) resends the same request message, so every
/// attempt of a call carries the same key and the Gateway replays a write it already finished instead of
/// running it (and charging the wallet) again.
/// </para>
/// <para>
/// Safe retry policy for such a handler: retry a GET on a network error, a timeout, 429, 500, 502, 503 or 504.
/// Retry a POST or PUT only on a network error, a timeout, 429, 502, 503, 504, or 409 with error code
/// <c>IDEMPOTENCY_CONFLICT</c>; never after a 500, because the Gateway does not keep a 500 for replay. Retry a
/// DELETE only on 429: DELETE is not covered by the Gateway's idempotency check.
/// </para>
/// <para>
/// A non-success response throws <see cref="FlexOpsException"/> (<see cref="FlexOpsAuthException"/> for 401 and
/// 403, <see cref="FlexOpsRateLimitException"/> for 429) with the Gateway's <c>errorCode</c> (or <c>code</c>) and
/// message. A success with no body (204, or an empty body) returns <c>null</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// using FlexOps.Sdk;
///
/// using var client = new FlexOpsClient("https://gateway.flexops.io", apiKey: "live_...");
/// client.WorkspaceId = "your-workspace-id";
///
/// var rates = await client.PostAsync&lt;RateShoppingResponse&gt;(
///     "api/shipping/rates",
///     new RateShoppingRequest
///     {
///         Origin = new() { AddressLine1 = "123 Main St", City = "New York", StateProvince = "NY", PostalCode = "10001" },
///         Destination = new() { AddressLine1 = "456 Oak Ave", City = "Los Angeles", StateProvince = "CA", PostalCode = "90210" },
///         Package = new() { Weight = 16, WeightUnit = "oz" }
///     });
/// </code>
/// </example>
public sealed class FlexOpsClient : IDisposable
{
    /// <summary>The header the Gateway idempotency middleware reads on POST, PUT and PATCH.</summary>
    public const string IdempotencyKeyHeader = "Idempotency-Key";

    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Gets or sets the active workspace ID used to scope requests.
    /// </summary>
    public string? WorkspaceId { get; set; }

    /// <summary>The Bearer token the client sends, if any (set by the constructor, <see cref="SetAccessToken"/> or a login).</summary>
    internal string? AccessToken { get; private set; }

    /// <summary>
    /// Creates a new FlexOps client.
    /// </summary>
    /// <param name="baseUrl">The FlexOps Gateway API base URL.</param>
    /// <param name="apiKey">Optional workspace API key for X-Api-Key authentication.</param>
    /// <param name="accessToken">Optional JWT access token for Bearer authentication.</param>
    /// <param name="workspaceId">Optional default workspace ID.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient. If null, a new one is created.</param>
    public FlexOpsClient(
        string baseUrl,
        string? apiKey = null,
        string? accessToken = null,
        string? workspaceId = null,
        HttpClient? httpClient = null)
    {
        if (httpClient is not null)
        {
            _http = httpClient;
            _ownsHttpClient = false;
        }
        else
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
            _ownsHttpClient = true;
        }

        if (!string.IsNullOrEmpty(apiKey))
        {
            _http.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        }
        else if (!string.IsNullOrEmpty(accessToken))
        {
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            AccessToken = accessToken;
        }

        _http.DefaultRequestHeaders.Add("Accept", "application/json");
        WorkspaceId = workspaceId;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    /// <summary>
    /// Sets the JWT access token for Bearer authentication.
    /// </summary>
    public void SetAccessToken(string token)
    {
        _http.DefaultRequestHeaders.Remove("Authorization");
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        AccessToken = token;
    }

    /// <summary>
    /// Sets the API key for authentication.
    /// </summary>
    public void SetApiKey(string key)
    {
        _http.DefaultRequestHeaders.Remove("X-Api-Key");
        _http.DefaultRequestHeaders.Add("X-Api-Key", key);
    }

    /// <summary>
    /// Builds a workspace-scoped path: <c>api/workspaces/{WorkspaceId}/{suffix}</c>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="WorkspaceId"/> is not set.</exception>
    public string WsPath(string suffix) => $"api/workspaces/{RequireWorkspaceId()}/{suffix}";

    /// <summary>Returns <see cref="WorkspaceId"/>, or throws when it is not set.</summary>
    internal string RequireWorkspaceId()
    {
        if (string.IsNullOrEmpty(WorkspaceId))
        {
            throw new InvalidOperationException("WorkspaceId is required. Set it before making workspace-scoped requests.");
        }

        return WorkspaceId;
    }

    /// <summary>Appends the non-null <paramref name="query"/> values to <paramref name="path"/>, URL-encoded.</summary>
    internal static string WithQuery(string path, IEnumerable<KeyValuePair<string, string?>>? query)
    {
        var builder = new StringBuilder(path);
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        foreach (var (name, value) in query ?? [])
        {
            if (value is null)
            {
                continue;
            }

            builder.Append(separator).Append(Uri.EscapeDataString(name)).Append('=').Append(Uri.EscapeDataString(value));
            separator = '&';
        }

        return builder.ToString();
    }

    // -- HTTP convenience methods ------------------------------------------

    /// <summary>Sends a GET request and deserializes the JSON response; <c>null</c> when the body is empty.</summary>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
        => SendJsonAsync<T>(HttpMethod.Get, path, null, false, null, null, ct);

    /// <summary>Sends a GET request with extra request headers and deserializes the JSON response.</summary>
    /// <param name="path">The relative request path.</param>
    /// <param name="headers">Headers added to this request only (for example <c>X-Workspace-Id</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<T?> GetAsync<T>(string path, IEnumerable<KeyValuePair<string, string>> headers, CancellationToken ct = default)
        => SendJsonAsync<T>(HttpMethod.Get, path, null, false, null, headers, ct);

    /// <summary>Sends a GET request and returns the raw response bytes (e.g. a label PDF); <c>null</c> when the body is empty.</summary>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public async Task<byte[]?> GetBytesAsync(string path, CancellationToken ct = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, false, null, null, ct).ConfigureAwait(false);
        var bytes = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        return bytes.Length == 0 ? null : bytes;
    }

    /// <summary>Sends a POST request with a JSON body and a new <c>Idempotency-Key</c>, and deserializes the response.</summary>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<TResponse?> PostAsync<TResponse>(string path, object? body = null, CancellationToken ct = default)
        => PostAsync<TResponse>(path, body, ct, null);

    /// <summary>Sends a POST with a caller-owned idempotency key (a new GUID when <c>null</c>) and deserializes the response.</summary>
    /// <param name="path">The relative request path.</param>
    /// <param name="body">The JSON body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="idempotencyKey">
    /// Sent as <c>Idempotency-Key</c>. Pass the same key to retry the same operation after an unknown outcome.
    /// </param>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<TResponse?> PostAsync<TResponse>(string path, object? body, CancellationToken ct, string? idempotencyKey)
        => SendJsonAsync<TResponse>(HttpMethod.Post, path, body, true, idempotencyKey, null, ct);

    /// <summary>Sends a PUT request with a JSON body and a new <c>Idempotency-Key</c>, and deserializes the response.</summary>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<TResponse?> PutAsync<TResponse>(string path, object? body = null, CancellationToken ct = default)
        => PutAsync<TResponse>(path, body, ct, null);

    /// <summary>Sends a PUT with a caller-owned idempotency key (a new GUID when <c>null</c>) and deserializes the response.</summary>
    /// <param name="path">The relative request path.</param>
    /// <param name="body">The JSON body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <param name="idempotencyKey">Sent as <c>Idempotency-Key</c>.</param>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<TResponse?> PutAsync<TResponse>(string path, object? body, CancellationToken ct, string? idempotencyKey)
        => SendJsonAsync<TResponse>(HttpMethod.Put, path, body, true, idempotencyKey, null, ct);

    /// <summary>Sends a DELETE request and deserializes the response; <c>null</c> for 204 No Content.</summary>
    /// <remarks>A DELETE carries no <c>Idempotency-Key</c>; the Gateway does not deduplicate it.</remarks>
    /// <exception cref="FlexOpsException">The Gateway answered with a non-success status.</exception>
    public Task<T?> DeleteAsync<T>(string path, CancellationToken ct = default)
        => SendJsonAsync<T>(HttpMethod.Delete, path, null, false, null, null, ct);

    private async Task<T?> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        object? body,
        bool hasBody,
        string? idempotencyKey,
        IEnumerable<KeyValuePair<string, string>>? headers,
        CancellationToken ct)
    {
        using var response = await SendAsync(method, path, body, hasBody, idempotencyKey, headers, ct).ConfigureAwait(false);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        var bytes = await response.Content.ReadAsByteArrayAsync(ct).ConfigureAwait(false);
        return bytes.Length == 0 ? default : JsonSerializer.Deserialize<T>(bytes, _jsonOptions);
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        object? body,
        bool hasBody,
        string? idempotencyKey,
        IEnumerable<KeyValuePair<string, string>>? headers,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, path);
        if (hasBody)
        {
            request.Content = JsonContent.Create(body, options: _jsonOptions);
        }

        // One key per call: a handler that resends this request message resends the same key.
        if (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch)
        {
            request.Headers.Add(IdempotencyKeyHeader, string.IsNullOrWhiteSpace(idempotencyKey) ? Guid.NewGuid().ToString() : idempotencyKey);
        }

        foreach (var (name, value) in headers ?? [])
        {
            request.Headers.Add(name, value);
        }

        var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        using (response)
        {
            throw await ToExceptionAsync(response, ct).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Maps a non-success response to a FlexOps exception carrying the Gateway envelope's <c>message</c>,
    /// <c>errorCode</c> (or <c>code</c>) and <c>errors</c>. A body that is not JSON keeps the status only.
    /// </summary>
    private static async Task<FlexOpsException> ToExceptionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var status = (int)response.StatusCode;
        var message = $"Gateway returned HTTP {status}";
        string? code = null;
        string[]? errors = null;
        JsonElement? data = null;
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(text))
            {
                using var document = JsonDocument.Parse(text);
                var root = document.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    message = StringProperty(root, "message") ?? message;
                    code = StringProperty(root, "errorCode") ?? StringProperty(root, "code");
                    errors = ErrorList(root);
                    if (root.TryGetProperty("data", out var envelopeData) && envelopeData.ValueKind != JsonValueKind.Null)
                    {
                        data = envelopeData.Clone();
                    }
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON (a proxy or load balancer page): keep the status.
        }

        FlexOpsException error = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new FlexOpsAuthException(message),
            HttpStatusCode.TooManyRequests => new FlexOpsRateLimitException(message)
            {
                RetryAfter = (int)(response.Headers.RetryAfter?.Delta?.TotalSeconds ?? 0),
            },
            _ => new FlexOpsException(message),
        };
        error.StatusCode = status;
        error.ErrorCode = code;
        error.Errors = errors;
        error.ResponseData = data;
        return error;
    }

    private static string? StringProperty(JsonElement root, string name)
        => root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    /// <summary>Reads <c>errors</c> as a string array, or flattens a ModelState-style object of string arrays.</summary>
    private static string[]? ErrorList(JsonElement root)
    {
        if (!root.TryGetProperty("errors", out var errors))
        {
            return null;
        }

        IEnumerable<JsonElement> items = errors.ValueKind switch
        {
            JsonValueKind.Array => errors.EnumerateArray(),
            JsonValueKind.Object => errors.EnumerateObject()
                .SelectMany(p => p.Value.ValueKind == JsonValueKind.Array ? p.Value.EnumerateArray() : (IEnumerable<JsonElement>)[p.Value]),
            _ => [],
        };
        var list = items.Where(e => e.ValueKind == JsonValueKind.String).Select(e => e.GetString()!).ToArray();
        return list.Length == 0 ? null : list;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _http.Dispose();
        }
    }
}

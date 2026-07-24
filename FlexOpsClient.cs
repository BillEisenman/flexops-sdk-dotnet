// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-04
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlexOps.Sdk;

/// <summary>
/// Thin HTTP client for the FlexOps multi-carrier shipping platform API.
/// Provides typed request/response wrappers around the Gateway REST endpoints.
/// </summary>
/// <example>
/// <code>
/// using FlexOps.Sdk;
///
/// using var client = new FlexOpsClient("https://gateway.flexops.io", apiKey: "fxk_live_...");
/// client.WorkspaceId = "ws_abc123";
///
/// var rates = await client.PostAsync&lt;ApiResponse&lt;ShippingRate[]&gt;&gt;(
///     $"api/workspaces/{client.WorkspaceId}/shipping/rates",
///     new { fromZip = "10001", toZip = "90210", weight = 16, weightUnit = "oz" });
/// </code>
/// </example>
public sealed class FlexOpsClient : IDisposable
{
    private readonly HttpClient _http;
    private readonly bool _ownsHttpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Gets or sets the active workspace ID used to scope requests.
    /// </summary>
    public string? WorkspaceId { get; set; }

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
    public string WsPath(string suffix)
    {
        if (string.IsNullOrEmpty(WorkspaceId))
        {
            throw new InvalidOperationException("WorkspaceId is required. Set it before making workspace-scoped requests.");
        }

        return $"api/workspaces/{WorkspaceId}/{suffix}";
    }

    // -- HTTP convenience methods ------------------------------------------

    /// <summary>Sends a GET request and deserializes the JSON response.</summary>
    public async Task<T?> GetAsync<T>(string path, CancellationToken ct = default)
    {
        return await _http.GetFromJsonAsync<T>(path, _jsonOptions, ct);
    }

    /// <summary>Sends a GET request and returns the raw response bytes (e.g. a label PDF).</summary>
    public async Task<byte[]?> GetBytesAsync(string path, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    /// <summary>Sends a POST request with a JSON body and deserializes the response.</summary>
    public async Task<TResponse?> PostAsync<TResponse>(string path, object? body = null, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(path, body, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
    }

    /// <summary>Sends a PUT request with a JSON body and deserializes the response.</summary>
    public async Task<TResponse?> PutAsync<TResponse>(string path, object? body = null, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync(path, body, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions, ct);
    }

    /// <summary>Sends a DELETE request and deserializes the response.</summary>
    public async Task<T?> DeleteAsync<T>(string path, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync(path, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);
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

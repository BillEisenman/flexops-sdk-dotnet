// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Webhook subscription management and signature verification.
/// </summary>
public sealed class WebhooksResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="WebhooksResource"/>.</summary>
    public WebhooksResource(FlexOpsClient client) => _client = client;

    /// <summary>List all webhook subscriptions for the workspace.</summary>
    public async Task<ApiResponse<WebhookSubscription[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<WebhookSubscription[]>>(_client.WsPath("webhooks"), ct);
    }

    /// <summary>Create a webhook subscription.</summary>
    /// <remarks>
    /// <see cref="CreateWebhookRequest.Events"/> is sent as the Gateway's comma-separated <c>eventTypes</c>;
    /// leave it <c>null</c> to receive every event. <see cref="WebhookSubscription.Secret"/> is set only on
    /// this response, so store it now.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <see cref="CreateWebhookRequest.Events"/> is empty or holds a value that is not a defined
    /// <see cref="WebhookEventType"/>. Nothing is sent.
    /// </exception>
    public async Task<ApiResponse<WebhookSubscription>?> CreateAsync(CreateWebhookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = new SubscriptionBody(request.Url, request.Description, ToEventTypes(request.Events, nameof(request)), null);
        return await CreateCoreAsync(body, ct);
    }

    /// <summary>Create a webhook subscription from an untyped payload.</summary>
    /// <remarks>
    /// The payload is sent as given, so it must already be the Gateway <c>CreateWebhookSubscriptionRequest</c>:
    /// <c>url</c>, optional <c>description</c>, and <c>eventTypes</c> as one comma-separated string with no spaces
    /// (for example <c>"LabelCreated,LabelCanceled"</c>). The Gateway ignores an <c>events</c> field and
    /// subscribes to every event. Prefer the <see cref="CreateWebhookRequest"/> overload, which builds and
    /// checks <c>eventTypes</c> for you.
    /// </remarks>
    public async Task<ApiResponse<WebhookSubscription>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await CreateCoreAsync(request, ct);
    }

    /// <summary>Update a webhook subscription. Only the properties set on <paramref name="request"/> are changed.</summary>
    /// <remarks><see cref="UpdateWebhookRequest.Events"/>, when set, replaces the subscribed events.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="request"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// <see cref="UpdateWebhookRequest.Events"/> is empty or holds a value that is not a defined
    /// <see cref="WebhookEventType"/>. Nothing is sent.
    /// </exception>
    public async Task<ApiResponse<WebhookSubscription>?> UpdateAsync(string webhookId, UpdateWebhookRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = new SubscriptionBody(request.Url, request.Description, ToEventTypes(request.Events, nameof(request)), request.Status);
        return await _client.PutAsync<ApiResponse<WebhookSubscription>>(_client.WsPath($"webhooks/{webhookId}"), body, ct);
    }

    /// <summary>Update a webhook subscription from an untyped payload.</summary>
    /// <remarks>
    /// The payload is sent as given, so subscribed events must be <c>eventTypes</c>, one comma-separated string
    /// with no spaces; the Gateway ignores an <c>events</c> field. Prefer the <see cref="UpdateWebhookRequest"/> overload.
    /// </remarks>
    public async Task<ApiResponse<WebhookSubscription>?> UpdateAsync(string webhookId, object data, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<WebhookSubscription>>(_client.WsPath($"webhooks/{webhookId}"), data, ct);
    }

    /// <summary>The Gateway create/update body. Null members are left out by the client serializer.</summary>
    private sealed record SubscriptionBody(string? Url, string? Description, string? EventTypes, string? Status);

    private static readonly JsonSerializerOptions ResponseJsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Joins events into the Gateway's <c>eventTypes</c>. The Gateway matches
    /// <c>("," + EventTypes + ",").Contains("," + name + ",")</c>, so names must be exact and the list has no
    /// spaces. A name it never sends would leave the subscription silent, so it is refused here.
    /// </summary>
    private static string? ToEventTypes(IReadOnlyCollection<WebhookEventType>? events, string paramName)
    {
        if (events is null)
        {
            return null;
        }

        if (events.Count == 0)
        {
            throw new ArgumentException(
                "Events must name at least one event; leave it null to receive every event.", paramName);
        }

        var unknown = events.Where(e => !Enum.IsDefined(e)).ToList();
        if (unknown.Count > 0)
        {
            throw new ArgumentException(
                $"Unknown webhook event: {string.Join(", ", unknown.Select(e => ((int)e).ToString(CultureInfo.InvariantCulture)))}. " +
                $"FlexOps sends {string.Join(", ", Enum.GetNames<WebhookEventType>())}.",
                paramName);
        }

        return string.Join(",", events.Distinct().Select(e => e.ToString()));
    }

    /// <summary>
    /// The Gateway answers a create with <c>201</c> and the bare subscription (not an <see cref="ApiResponse{T}"/>
    /// envelope). Both shapes are read so the secret is never lost.
    /// </summary>
    private async Task<ApiResponse<WebhookSubscription>?> CreateCoreAsync(object request, CancellationToken ct)
    {
        var json = await _client.PostAsync<JsonElement>(_client.WsPath("webhooks"), request, ct);
        if (json.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (json.TryGetProperty("success", out _) && json.TryGetProperty("data", out _))
        {
            return json.Deserialize<ApiResponse<WebhookSubscription>>(ResponseJsonOptions);
        }

        return new ApiResponse<WebhookSubscription>
        {
            Success = true,
            Data = json.Deserialize<WebhookSubscription>(ResponseJsonOptions),
        };
    }

    /// <summary>Delete a webhook subscription. Returns <c>null</c> (204 No Content) on success.</summary>
    public async Task<ApiResponse<object>?> DeleteAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"webhooks/{webhookId}"), ct);
    }

    /// <summary>Rotate the signing secret for a webhook.</summary>
    public async Task<ApiResponse<object>?> RotateSecretAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"webhooks/{webhookId}/rotate-secret"), null, ct);
    }

    /// <summary>Send a test delivery to a webhook. It arrives with <c>X-FlexOps-Event: test</c>.</summary>
    public async Task<ApiResponse<object>?> TestAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"webhooks/{webhookId}/test"), null, ct);
    }

    /// <summary>List delivery logs for a webhook.</summary>
    public async Task<ApiResponse<object[]>?> ListDeliveryLogsAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath($"webhooks/{webhookId}/deliveries"), ct);
    }

    /// <summary>
    /// The default maximum distance between <c>X-FlexOps-Timestamp</c> and now that
    /// <see cref="VerifySignature"/> accepts: five minutes.
    /// </summary>
    public static readonly TimeSpan DefaultSignatureTolerance = TimeSpan.FromMinutes(5);

    private const string SignaturePrefix = "sha256=";
    private const int Sha256HexLength = 64;

    /// <summary>
    /// Verify that an incoming request is a genuine FlexOps webhook delivery.
    /// </summary>
    /// <remarks>
    /// FlexOps signs <c>{X-FlexOps-Timestamp}.{raw body}</c> with HMAC-SHA256, keyed with the UTF-8
    /// bytes of the whole <c>whsec_...</c> secret, and sends
    /// <c>X-FlexOps-Signature: sha256=&lt;lowercase hex&gt;</c>. The body is the event data with no
    /// envelope; the event name is in the <c>X-FlexOps-Event</c> header.
    /// </remarks>
    /// <param name="payload">The raw request body exactly as received. Re-serialized JSON will not match.</param>
    /// <param name="signature">The <c>X-FlexOps-Signature</c> header. <c>sha256=&lt;hex&gt;</c> and bare hex are accepted.</param>
    /// <param name="secret">The webhook signing secret.</param>
    /// <param name="timestamp">The <c>X-FlexOps-Timestamp</c> header (Unix seconds).</param>
    /// <param name="tolerance">
    /// Maximum distance between <paramref name="timestamp"/> and now, in either direction.
    /// Defaults to <see cref="DefaultSignatureTolerance"/>.
    /// </param>
    /// <param name="now">The current time. Defaults to <see cref="DateTimeOffset.UtcNow"/>; set it in tests.</param>
    /// <returns>
    /// <c>true</c> only when the signature matches and the timestamp is within tolerance. A missing,
    /// malformed or wrong-length signature, or a missing, non-numeric or out-of-tolerance timestamp,
    /// returns <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="payload"/> or <paramref name="secret"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException"><paramref name="secret"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tolerance"/> is negative.</exception>
    public static bool VerifySignature(
        string payload,
        string? signature,
        string secret,
        string? timestamp,
        TimeSpan? tolerance = null,
        DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentException.ThrowIfNullOrEmpty(secret);
        var window = tolerance ?? DefaultSignatureTolerance;
        ArgumentOutOfRangeException.ThrowIfLessThan(window, TimeSpan.Zero, nameof(tolerance));

        if (signature is null || timestamp is null)
        {
            return false;
        }

        // NumberStyles.None accepts ASCII digits only: no sign, whitespace, decimal point or exponent.
        var signedTimestamp = timestamp.Trim();
        if (!long.TryParse(signedTimestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var signedAt))
        {
            return false;
        }

        var current = (now ?? DateTimeOffset.UtcNow).ToUnixTimeSeconds();
        var windowSeconds = (long)window.TotalSeconds;
        if (signedAt < current - windowSeconds || signedAt > current + windowSeconds)
        {
            return false;
        }

        var hex = signature.Trim();
        if (hex.StartsWith(SignaturePrefix, StringComparison.OrdinalIgnoreCase))
        {
            hex = hex[SignaturePrefix.Length..];
        }

        // Any other length or a non-hex character cannot match; return before decoding.
        if (hex.Length != Sha256HexLength || !hex.All(char.IsAsciiHexDigit))
        {
            return false;
        }

        var provided = Convert.FromHexString(hex);
        var expected = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes($"{signedTimestamp}.{payload}"));
        return CryptographicOperations.FixedTimeEquals(provided, expected);
    }
}

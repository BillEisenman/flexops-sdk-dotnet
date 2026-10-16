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

    /// <summary>Get a webhook subscription by ID.</summary>
    public async Task<ApiResponse<WebhookSubscription>?> GetAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<WebhookSubscription>>(_client.WsPath($"webhooks/{webhookId}"), ct);
    }

    /// <summary>Create a webhook subscription.</summary>
    public async Task<ApiResponse<WebhookSubscription>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<WebhookSubscription>>(_client.WsPath("webhooks"), request, ct);
    }

    /// <summary>Update a webhook subscription.</summary>
    public async Task<ApiResponse<WebhookSubscription>?> UpdateAsync(string webhookId, object data, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<WebhookSubscription>>(_client.WsPath($"webhooks/{webhookId}"), data, ct);
    }

    /// <summary>Delete a webhook subscription.</summary>
    public async Task<ApiResponse<object>?> DeleteAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"webhooks/{webhookId}"), ct);
    }

    /// <summary>Rotate the signing secret for a webhook.</summary>
    public async Task<ApiResponse<object>?> RotateSecretAsync(string webhookId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"webhooks/{webhookId}/rotate-secret"), null, ct);
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

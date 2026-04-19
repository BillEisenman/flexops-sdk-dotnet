// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

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
    /// Verify a webhook signature from an incoming request.
    /// Use this in your webhook handler to validate authenticity.
    /// </summary>
    /// <param name="payload">The raw request body as a string.</param>
    /// <param name="signature">The signature from the <c>X-Webhook-Signature</c> header (hex-encoded).</param>
    /// <param name="secret">The webhook signing secret.</param>
    /// <returns><c>true</c> if the signature is valid; otherwise <c>false</c>.</returns>
    public static bool VerifySignature(string payload, string signature, string secret)
    {
        try
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            var expectedBytes = HMACSHA256.HashData(keyBytes, payloadBytes);
            var expected = Convert.ToHexStringLower(expectedBytes);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signature),
                Encoding.UTF8.GetBytes(expected));
        }
        catch
        {
            return false;
        }
    }
}

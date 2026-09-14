// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Text.Json;
using System.Text.Json.Nodes;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Normalized shipping operations: rate shopping, labels, tracking, batch, and address validation.
/// </summary>
/// <remarks>
/// The <c>api/shipping/*</c> endpoints are NOT workspace-scoped and return the raw DTO (no
/// <c>ApiResponse&lt;T&gt;</c> envelope) — the workspace is resolved from the API key. Only the
/// batch-label endpoints are workspace-scoped and enveloped.
/// </remarks>
public sealed class ShippingResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ShippingResource"/>.</summary>
    public ShippingResource(FlexOpsClient client) => _client = client;

    // -----------------------------------------------------------------------
    // Rate Shopping
    // -----------------------------------------------------------------------

    /// <summary>Get shipping rates from all configured carriers.</summary>
    public async Task<RateShoppingResponse?> GetRatesAsync(RateShoppingRequest request, CancellationToken ct = default)
    {
        return await _client.PostAsync<RateShoppingResponse>("api/shipping/rates", request, ct);
    }

    /// <summary>Get shipping rates using an untyped request payload.</summary>
    [Obsolete("Use the RateShoppingRequest overload.")]
    public async Task<RateShoppingResponse?> GetRatesAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<RateShoppingResponse>("api/shipping/rates", request, ct);
    }

    /// <summary>Get the single cheapest rate across all carriers.</summary>
    public async Task<ShippingRate?> GetCheapestRateAsync(RateShoppingRequest request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ShippingRate>("api/shipping/rates/cheapest", request, ct);
    }

    /// <summary>Get the single cheapest rate using an untyped request payload.</summary>
    [Obsolete("Use the RateShoppingRequest overload.")]
    public async Task<ShippingRate?> GetCheapestRateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ShippingRate>("api/shipping/rates/cheapest", request, ct);
    }

    /// <summary>Get the single fastest rate across all carriers.</summary>
    public async Task<ShippingRate?> GetFastestRateAsync(RateShoppingRequest request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ShippingRate>("api/shipping/rates/fastest", request, ct);
    }

    /// <summary>Get the single fastest rate using an untyped request payload.</summary>
    [Obsolete("Use the RateShoppingRequest overload.")]
    public async Task<ShippingRate?> GetFastestRateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ShippingRate>("api/shipping/rates/fastest", request, ct);
    }

    // -----------------------------------------------------------------------
    // Labels
    // -----------------------------------------------------------------------

    /// <summary>
    /// Create a shipping label. Supply a <c>LabelRequest</c> body; set <c>orderId</c> to buy
    /// against an existing order (server-side ownership / status / ship-method validation +
    /// atomic postage settlement). Returns the raw label (HTTP 201).
    /// </summary>
    /// <remarks>Live purchases require PrepareLabelAsync and explicit PurchaseLabelAsync approval. This legacy method never accepts a preview as a label.</remarks>
    public async Task<Label?> CreateLabelAsync(object request, CancellationToken ct = default)
    {
        var response = await _client.PostAsync<JsonElement>("api/shipping/labels", request, ct);
        if (response.TryGetProperty("status", out var status) && status.GetString() == "Preview")
            throw new FlexOpsException("Approval required. Use PrepareLabelAsync, review the preview, then PurchaseLabelAsync.") { ErrorCode = "ApprovalRequired" };
        return response.Deserialize<Label>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    /// <summary>Prepare a bounded purchase without approving it. Retain the returned operation for every retry.</summary>
    public async Task<LabelPurchaseApproval> PrepareLabelAsync(object request, decimal maximumPostageAmount, string idempotencyKey, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (maximumPostageAmount <= 0 || maximumPostageAmount > 1000000 || decimal.Round(maximumPostageAmount, 2) != maximumPostageAmount)
            throw new ArgumentOutOfRangeException(nameof(maximumPostageAmount));
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var body = JsonSerializer.SerializeToNode(request, options) as JsonObject ?? throw new ArgumentException("A label request object is required.", nameof(request));
        foreach (var key in body.Select(p => p.Key).Where(k => k.Equals("confirmationToken", StringComparison.OrdinalIgnoreCase) || k.Equals("maximumPostageAmount", StringComparison.OrdinalIgnoreCase)).ToArray())
            body.Remove(key);
        body["maximumPostageAmount"] = maximumPostageAmount;
        var result = await _client.PostAsync<JsonElement>("api/shipping/labels", body, ct, idempotencyKey);
        if (result.ValueKind == JsonValueKind.Object && result.TryGetProperty("isSandbox", out var sandbox) && sandbox.ValueKind == JsonValueKind.True)
            return new LabelPurchaseApproval(idempotencyKey, body.ToJsonString(), result, result.Deserialize<Label>(options));
        if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty("status", out var status) || status.ValueKind != JsonValueKind.String || status.GetString() != "Preview" ||
            !result.TryGetProperty("confirmationToken", out var token) || token.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(token.GetString()) ||
            !result.TryGetProperty("currency", out var currency) || currency.ValueKind != JsonValueKind.String || currency.GetString() != "USD" ||
            !result.TryGetProperty("quotedPostageAmount", out var quote) || quote.ValueKind != JsonValueKind.Number || !quote.TryGetDecimal(out var amount) || amount <= 0 || amount > maximumPostageAmount ||
            !result.TryGetProperty("maximumPostageAmount", out var limit) || limit.ValueKind != JsonValueKind.Number || !limit.TryGetDecimal(out var approvedMaximum) || approvedMaximum != maximumPostageAmount ||
            !result.TryGetProperty("expiresAt", out var expiry) || expiry.ValueKind != JsonValueKind.String || !expiry.TryGetDateTimeOffset(out _))
            throw new FlexOpsException("Gateway did not return a valid bounded preview.");
        body["confirmationToken"] = token.GetString();
        return new LabelPurchaseApproval(idempotencyKey, body.ToJsonString(), result, null);
    }

    /// <summary>Purchase only after explicit approval. Retry this same operation; never replace its key after an unknown outcome.</summary>
    public async Task<Label?> PurchaseLabelAsync(LabelPurchaseApproval approval, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(approval);
        if (approval.SandboxLabel is not null) return approval.SandboxLabel;
        // Gateway decides expiry: a completed purchase remains replayable after its token expires.
        var response = await _client.PostAsync<JsonElement>("api/shipping/labels", JsonNode.Parse(approval.RequestJson), ct, approval.IdempotencyKey);
        if (!response.TryGetProperty("trackingNumber", out var tracking) || string.IsNullOrWhiteSpace(tracking.GetString()))
            throw new FlexOpsException("Purchase outcome is unresolved. Retain this operation and reconcile before creating another label.") { ErrorCode = "OutcomeUnknown" };
        return response.Deserialize<Label>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    /// <summary>Cancel (void) a shipping label. <paramref name="carrierCode"/> is required.</summary>
    public async Task<object?> CancelLabelAsync(string labelId, string carrierCode, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<object>(
            $"api/shipping/labels/{labelId}?carrierCode={Uri.EscapeDataString(carrierCode)}", ct);
    }

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Track a shipment by tracking number.</summary>
    public async Task<TrackingInfo?> TrackAsync(string trackingNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<TrackingInfo>($"api/shipping/track/{Uri.EscapeDataString(trackingNumber)}", ct);
    }

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate and correct a shipping address.</summary>
    public async Task<object?> ValidateAddressAsync(object address, CancellationToken ct = default)
    {
        return await _client.PostAsync<object>("api/shipping/addresses/validate", address, ct);
    }

    // -----------------------------------------------------------------------
    // Batch Labels (workspace-scoped, ApiResponse-enveloped)
    // -----------------------------------------------------------------------

    /// <summary>Create labels in batch.</summary>
    public async Task<ApiResponse<object>?> CreateBatchAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("labels/batch"), request, ct);
    }

    /// <summary>Preview a batch without purchasing (dry-run).</summary>
    public async Task<ApiResponse<object>?> PreviewBatchAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("labels/batch/preview"), request, ct);
    }

    /// <summary>Get batch job status.</summary>
    public async Task<ApiResponse<object>?> GetBatchStatusAsync(string jobId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"labels/batch/{jobId}"), ct);
    }

    /// <summary>Download a label PDF from a batch job. Returns the raw PDF bytes.</summary>
    public async Task<byte[]?> DownloadBatchLabelAsync(string jobId, string itemId, CancellationToken ct = default)
    {
        return await _client.GetBytesAsync(_client.WsPath($"labels/batch/{jobId}/items/{itemId}/label"), ct);
    }

    // -----------------------------------------------------------------------
    // Carriers
    // -----------------------------------------------------------------------

    /// <summary>List available carriers and their services.</summary>
    public async Task<object?> GetCarriersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<object>("api/shipping/carriers", ct);
    }

    // -----------------------------------------------------------------------
    // AI / Savings
    // -----------------------------------------------------------------------

    /// <summary>Get AI-powered carrier and service recommendations for a shipment.</summary>
    public async Task<object?> GetRecommendationsAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<object>("api/shipping/recommendations", request, ct);
    }

    /// <summary>Predict the delivery date and confidence for a shipment.</summary>
    public async Task<object?> PredictDeliveryAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<object>("api/shipping/predictions/delivery", request, ct);
    }

    /// <summary>Get an aggregate savings summary for the workspace.</summary>
    public async Task<object?> GetSavingsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<object>("api/shipping/savings", ct);
    }
}

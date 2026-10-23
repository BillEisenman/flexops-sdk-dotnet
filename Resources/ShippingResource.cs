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
        if (response.ValueKind != JsonValueKind.Object)
            return null;
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
        if (response.ValueKind != JsonValueKind.Object || !response.TryGetProperty("trackingNumber", out var tracking) || string.IsNullOrWhiteSpace(tracking.GetString()))
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

    /// <summary>Validate an address with the carriers (<c>POST api/addresses/validate</c>).</summary>
    /// <remarks>
    /// Needs the <c>shipping:read</c> scope. A result with <see cref="AddressValidationResult.ValidationUnavailable"/>
    /// set means no carrier check completed; it does not reject the address.
    /// </remarks>
    public async Task<AddressValidationResult?> ValidateAddressAsync(AddressValidationRequest address, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(address);
        return await _client.PostAsync<AddressValidationResult>("api/addresses/validate", address, ct);
    }

    /// <summary>Validate a <see cref="ShippingAddress"/>, sent as the Gateway's flat address (<c>line1</c>, <c>city</c>, <c>state</c>, ...).</summary>
    public Task<AddressValidationResult?> ValidateAddressAsync(ShippingAddress address, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(address);
        return ValidateAddressAsync(new AddressValidationRequest
        {
            Name = address.Name,
            Line1 = address.AddressLine1,
            City = address.City,
            State = address.StateProvince,
            PostalCode = address.PostalCode,
            Country = address.CountryCode,
        }, ct);
    }

    // -----------------------------------------------------------------------
    // Batch Labels (workspace-scoped, ApiResponse-enveloped)
    // -----------------------------------------------------------------------

    /// <summary>Create labels in batch.</summary>
    /// <remarks>The payload is sent as given. Set <c>hazmatClassification</c> (a
    /// <see cref="HazmatClassification"/> number) on an item only when you have a declaration; the SDK never adds one.</remarks>
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
    // Multi-Package Labels
    // -----------------------------------------------------------------------

    /// <summary>Buy labels for one order shipped in 1 to 99 boxes with the same carrier and service.</summary>
    /// <param name="request">The order, carrier, service and boxes.</param>
    /// <param name="idempotencyKey">Sent as <c>Idempotency-Key</c>; a new GUID when <c>null</c>. Reuse it to retry the same shipment.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The envelope when at least one label was bought; check <see cref="MultiPackageShipmentResponse.FailedPackages"/>.</returns>
    /// <remarks>
    /// Every box's label slot is reserved before any label is bought; a box that fails does not stop the others, and
    /// each box reports its own result. When no label was bought the Gateway answers 422 <c>AllPackagesFailed</c>, or
    /// 409 <c>OutcomeUnknown</c> when a box's carrier outcome is unknown and a label may exist: both throw a
    /// <see cref="FlexOpsException"/> whose <see cref="FlexOpsException.ResponseData"/> holds the per-box results. After
    /// <c>OutcomeUnknown</c>, check the order before buying again. <see cref="MultiPackageShipmentRequest.MaximumPostageAmount"/>
    /// is required: the most the whole shipment may cost in USD. The Gateway refuses to buy any box that would take the
    /// total above it (that box reports <c>PriceCeilingExceeded</c>; boxes after the ceiling is used up report
    /// <c>PriceCeilingReached</c>), and a request without a valid one is 400 <c>ApprovalRequired</c>.
    /// <see cref="MultiPackageShipmentRequest.HazmatClassification"/> accepts only <see cref="HazmatClassification.None"/> or <c>null</c>.
    /// </remarks>
    /// <exception cref="FlexOpsException">The Gateway refused the shipment or bought no label.</exception>
    public async Task<ApiResponse<MultiPackageShipmentResponse>?> CreateMultiPackageAsync(
        MultiPackageShipmentRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<MultiPackageShipmentResponse>>("api/shipping/multi-package", request, ct, idempotencyKey);
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

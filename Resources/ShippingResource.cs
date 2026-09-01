// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

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
    public async Task<Label?> CreateLabelAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<Label>("api/shipping/labels", request, ct);
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

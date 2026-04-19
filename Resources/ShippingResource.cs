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
public sealed class ShippingResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ShippingResource"/>.</summary>
    public ShippingResource(FlexOpsClient client) => _client = client;

    // -----------------------------------------------------------------------
    // Rate Shopping
    // -----------------------------------------------------------------------

    /// <summary>Get shipping rates from all configured carriers.</summary>
    public async Task<ApiResponse<ShippingRate[]>?> GetRatesAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<ShippingRate[]>>(_client.WsPath("shipping/rates"), request, ct);
    }

    /// <summary>Get the single cheapest rate across all carriers.</summary>
    public async Task<ApiResponse<ShippingRate>?> GetCheapestRateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<ShippingRate>>(_client.WsPath("shipping/rates/cheapest"), request, ct);
    }

    /// <summary>Get the single fastest rate across all carriers.</summary>
    public async Task<ApiResponse<ShippingRate>?> GetFastestRateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<ShippingRate>>(_client.WsPath("shipping/rates/fastest"), request, ct);
    }

    // -----------------------------------------------------------------------
    // Labels
    // -----------------------------------------------------------------------

    /// <summary>Create a shipping label.</summary>
    public async Task<ApiResponse<Label>?> CreateLabelAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<Label>>(_client.WsPath("shipping/labels"), request, ct);
    }

    /// <summary>Cancel (void) a shipping label.</summary>
    public async Task<ApiResponse<object>?> CancelLabelAsync(string labelId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"shipping/labels/{labelId}"), ct);
    }

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Track a shipment by tracking number.</summary>
    public async Task<ApiResponse<TrackingInfo>?> TrackAsync(string trackingNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<TrackingInfo>>(_client.WsPath($"shipping/track/{trackingNumber}"), ct);
    }

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate and correct a shipping address.</summary>
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object address, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping/addresses/validate"), address, ct);
    }

    // -----------------------------------------------------------------------
    // Batch Labels
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

    /// <summary>Download a label from a batch job.</summary>
    public async Task<ApiResponse<object>?> DownloadBatchLabelAsync(string jobId, string itemId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"labels/batch/{jobId}/items/{itemId}/label"), ct);
    }

    // -----------------------------------------------------------------------
    // Carriers
    // -----------------------------------------------------------------------

    /// <summary>List available carriers and their services.</summary>
    public async Task<ApiResponse<object>?> GetCarriersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath("shipping/carriers"), ct);
    }

    // -----------------------------------------------------------------------
    // AI / Savings
    // -----------------------------------------------------------------------

    /// <summary>Get AI-powered carrier and service recommendations for a shipment.</summary>
    /// <param name="request">Shipment details used to generate recommendations.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetRecommendationsAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping/recommendations"), request, ct);
    }

    /// <summary>Predict the delivery date and confidence for a shipment.</summary>
    /// <param name="request">Shipment details used for delivery prediction.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> PredictDeliveryAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping/predictions/delivery"), request, ct);
    }

    /// <summary>Get an aggregate savings summary for the workspace.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetSavingsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath("shipping/savings"), ct);
    }
}

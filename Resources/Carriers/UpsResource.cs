// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// UPS carrier operations via the VisionSuite API proxy (V2 endpoints).
/// All methods proxy through <c>/api/ApiProxy/api/v2/ShippingLabel/...</c>.
/// </summary>
public sealed class UpsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="UpsResource"/>.</summary>
    public UpsResource(FlexOpsClient client) => _client = client;

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    /// <summary>Verify an address via UPS.</summary>
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsVerifyAddress"), body, ct);
    }

    /// <summary>Get UPS rate quotes.</summary>
    public async Task<ApiResponse<object>?> GetRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsRateCheck"), body, ct);
    }

    /// <summary>Generate a UPS shipping label.</summary>
    public async Task<ApiResponse<object>?> CreateLabelAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/generateNewUpsShipLabel"), body, ct);
    }

    /// <summary>Track a UPS shipment.</summary>
    public async Task<ApiResponse<object>?> TrackAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getSingleUpsTrackingDetail"), parameters, ct);
    }

    /// <summary>Create a UPS pickup.</summary>
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsCreatePickup"), body, ct);
    }

    /// <summary>Cancel a UPS pickup.</summary>
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/deleteUpsPickup"), ct);
    }

    /// <summary>Get UPS transit times.</summary>
    public async Task<ApiResponse<object>?> GetTransitTimesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetTransitTimes"), body, ct);
    }

    /// <summary>Get UPS landed cost quote (international duties/taxes).</summary>
    public async Task<ApiResponse<object>?> GetLandedCostAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetLandedCostQuote"), body, ct);
    }

    /// <summary>Search UPS locations.</summary>
    public async Task<ApiResponse<object>?> SearchLocationsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsSearchLocations"), body, ct);
    }

    /// <summary>Upload a paperless trade document.</summary>
    public async Task<ApiResponse<object>?> UploadDocumentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsUploadPaperlessDocument"), body, ct);
    }

    /// <summary>Create a UPS freight shipment.</summary>
    public async Task<ApiResponse<object>?> CreateFreightShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsCreateFreightShipment"), body, ct);
    }

    /// <summary>Get a UPS freight rate quote.</summary>
    public async Task<ApiResponse<object>?> GetFreightRateAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetFreightRate"), body, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// UPS operations via the VisionSuite API proxy (V2 endpoints): tracking as a GET lookup, and deprecated proxy writes.
/// </summary>
/// <remarks>
/// Tracking works with API keys that have <c>tracking:read</c> (otherwise 403 <c>API_KEY_SCOPE_REQUIRED</c>). The <see cref="ObsoleteAttribute">obsolete</see> writes need a user token (API keys
/// get 403 <c>API_KEY_SCOPE_REQUIRED</c>) and stop working when the proxy is retired on 2027-06-30; use
/// <see cref="ShippingResource"/> for rates and labels and <see cref="PickupsResource"/> for pickups. The proxy
/// label route is closed (410 Gone).
/// </remarks>
public sealed class UpsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="UpsResource"/>.</summary>
    public UpsResource(FlexOpsClient client) => _client = client;

    /// <summary>Track a UPS shipment.</summary>
    public async Task<ApiResponse<object>?> TrackAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(CarriersResource.ProxyPath("api/v2/ShippingLabel/getSingleUpsTrackingDetail", parameters), ct);
    }

    // -----------------------------------------------------------------------
    // Deprecated: legacy proxy writes (user token only, sunset 2027-06-30)
    // -----------------------------------------------------------------------

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    /// <summary>Verify an address via UPS.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsVerifyAddress"), body, ct);
    }

    /// <summary>Get UPS rate quotes.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsRateCheck"), body, ct);
    }

    /// <summary>Create a UPS pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsCreatePickup"), body, ct);
    }

    /// <summary>Cancel a UPS pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/deleteUpsPickup"), ct);
    }

    /// <summary>Get UPS transit times.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetTransitTimesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetTransitTimes"), body, ct);
    }

    /// <summary>Get UPS landed cost quote (international duties/taxes).</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetLandedCostAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetLandedCostQuote"), body, ct);
    }

    /// <summary>Search UPS locations.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> SearchLocationsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsSearchLocations"), body, ct);
    }

    /// <summary>Upload a paperless trade document.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> UploadDocumentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsUploadPaperlessDocument"), body, ct);
    }

    /// <summary>Create a UPS freight shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreateFreightShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsCreateFreightShipment"), body, ct);
    }

    /// <summary>Get a UPS freight rate quote.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetFreightRateAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postUpsGetFreightRate"), body, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// DHL operations via the VisionSuite API proxy (V2 endpoints): GET lookups with the parameters in the query
/// string, and deprecated proxy writes.
/// </summary>
/// <remarks>
/// The lookups work with API keys. The <see cref="ObsoleteAttribute">obsolete</see> writes need a user token (API
/// keys get 403 <c>API_KEY_SCOPE_REQUIRED</c>) and stop working when the proxy is retired on 2027-06-30; use
/// <see cref="ShippingResource"/> for rates and labels and <see cref="PickupsResource"/> for pickups. The proxy
/// shipment route is closed (410 Gone), and there is no pickup update (the proxy has no PATCH).
/// </remarks>
public sealed class DhlResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="DhlResource"/>.</summary>
    public DhlResource(FlexOpsClient client) => _client = client;

    private Task<ApiResponse<object>?> GetAsync(string endpoint, IReadOnlyDictionary<string, string>? parameters, CancellationToken ct)
        => _client.GetAsync<ApiResponse<object>>(CarriersResource.ProxyPath($"api/v2/ShippingLabel/{endpoint}", parameters), ct);

    /// <summary>Validate an address via DHL.</summary>
    public Task<ApiResponse<object>?> ValidateAddressAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlValidateAddress", parameters, ct);

    /// <summary>Get DHL shipping rates.</summary>
    public Task<ApiResponse<object>?> GetRatesAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlRates", parameters, ct);

    /// <summary>Get DHL products/services.</summary>
    public Task<ApiResponse<object>?> GetProductsAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlProducts", parameters, ct);

    /// <summary>Track a single DHL shipment.</summary>
    public Task<ApiResponse<object>?> TrackAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlTrackSingleShipment", parameters, ct);

    /// <summary>Track multiple DHL shipments.</summary>
    public Task<ApiResponse<object>?> TrackMultipleAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlTrackMultipleShipments", parameters, ct);

    /// <summary>Get electronic proof of delivery.</summary>
    public Task<ApiResponse<object>?> GetProofOfDeliveryAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlElectronicProofOfDelivery", parameters, ct);

    /// <summary>Get DHL reference data (countries, services, etc.).</summary>
    public Task<ApiResponse<object>?> GetReferenceDataAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlReferenceData", parameters, ct);

    /// <summary>Find DHL service points.</summary>
    public Task<ApiResponse<object>?> FindServicePointsAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("getDhlServicePoints", parameters, ct);

    // -----------------------------------------------------------------------
    // Deprecated: legacy proxy writes (user token only, sunset 2027-06-30)
    // -----------------------------------------------------------------------

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    /// <summary>Get multi-piece DHL rates.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetMultiPieceRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlMultiPieceRates"), body, ct);
    }

    /// <summary>Create a DHL pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlCreatePickup"), body, ct);
    }

    /// <summary>Cancel a DHL pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/deleteDhlPickup"), ct);
    }

    /// <summary>Calculate landed cost (duties/taxes).</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CalculateLandedCostAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlCalculateLandedCost"), body, ct);
    }

    /// <summary>Screen a shipment for compliance.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ScreenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlScreenShipment"), body, ct);
    }

    /// <summary>Upload an invoice to DHL.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> UploadInvoiceAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlUploadInvoice"), body, ct);
    }
}

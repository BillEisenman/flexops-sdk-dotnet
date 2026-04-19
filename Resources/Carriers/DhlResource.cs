// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// DHL carrier operations via the VisionSuite API proxy (V2 endpoints).
/// All methods proxy through <c>/api/ApiProxy/api/v2/ShippingLabel/...</c>.
/// </summary>
public sealed class DhlResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="DhlResource"/>.</summary>
    public DhlResource(FlexOpsClient client) => _client = client;

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate an address via DHL.</summary>
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlValidateAddress"), parameters, ct);
    }

    // -----------------------------------------------------------------------
    // Rates & Products
    // -----------------------------------------------------------------------

    /// <summary>Get DHL shipping rates.</summary>
    public async Task<ApiResponse<object>?> GetRatesAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlRates"), parameters, ct);
    }

    /// <summary>Get multi-piece DHL rates.</summary>
    public async Task<ApiResponse<object>?> GetMultiPieceRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlMultiPieceRates"), body, ct);
    }

    /// <summary>Get DHL products/services.</summary>
    public async Task<ApiResponse<object>?> GetProductsAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlProducts"), parameters, ct);
    }

    // -----------------------------------------------------------------------
    // Shipping / Labels
    // -----------------------------------------------------------------------

    /// <summary>Create a DHL shipment (label).</summary>
    public async Task<ApiResponse<object>?> CreateShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlCreateShipment"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Track a single DHL shipment.</summary>
    public async Task<ApiResponse<object>?> TrackAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlTrackSingleShipment"), parameters, ct);
    }

    /// <summary>Track multiple DHL shipments.</summary>
    public async Task<ApiResponse<object>?> TrackMultipleAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlTrackMultipleShipments"), parameters, ct);
    }

    // -----------------------------------------------------------------------
    // Pickup
    // -----------------------------------------------------------------------

    /// <summary>Create a DHL pickup.</summary>
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlCreatePickup"), body, ct);
    }

    /// <summary>Update a DHL pickup.</summary>
    public async Task<ApiResponse<object>?> UpdatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/patchDhlUpdatePickup"), body, ct);
    }

    /// <summary>Cancel a DHL pickup.</summary>
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/deleteDhlPickup"), ct);
    }

    // -----------------------------------------------------------------------
    // Landed Cost & Compliance
    // -----------------------------------------------------------------------

    /// <summary>Calculate landed cost (duties/taxes).</summary>
    public async Task<ApiResponse<object>?> CalculateLandedCostAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlCalculateLandedCost"), body, ct);
    }

    /// <summary>Screen a shipment for compliance.</summary>
    public async Task<ApiResponse<object>?> ScreenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlScreenShipment"), body, ct);
    }

    /// <summary>Upload an invoice to DHL.</summary>
    public async Task<ApiResponse<object>?> UploadInvoiceAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/postDhlUploadInvoice"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Proof of Delivery & Reference Data
    // -----------------------------------------------------------------------

    /// <summary>Get electronic proof of delivery.</summary>
    public async Task<ApiResponse<object>?> GetProofOfDeliveryAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlElectronicProofOfDelivery"), parameters, ct);
    }

    /// <summary>Get DHL reference data (countries, services, etc.).</summary>
    public async Task<ApiResponse<object>?> GetReferenceDataAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlReferenceData"), parameters, ct);
    }

    /// <summary>Find DHL service points.</summary>
    public async Task<ApiResponse<object>?> FindServicePointsAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v2/ShippingLabel/getDhlServicePoints"), parameters, ct);
    }
}

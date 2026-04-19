// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// FedEx carrier operations via the VisionSuite API proxy (V3 endpoints).
/// All methods proxy through <c>/api/ApiProxy/api/v3/...</c>.
/// </summary>
public sealed class FedExResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="FedExResource"/>.</summary>
    public FedExResource(FlexOpsClient client) => _client = client;

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate and correct a domestic address via FedEx.</summary>
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/postFedExValidateAndCorrectDomesticAddress"), body, ct);
    }

    /// <summary>Validate a postal code via FedEx.</summary>
    public async Task<ApiResponse<object>?> ValidatePostalCodeAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/postFedExValidatePostalCode"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Rates
    // -----------------------------------------------------------------------

    /// <summary>Get FedEx rate and transit times.</summary>
    public async Task<ApiResponse<object>?> GetRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postRetrieveFedExRateAndTransitTimesAsync"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Shipping / Labels
    // -----------------------------------------------------------------------

    /// <summary>Create a new FedEx shipment (label).</summary>
    public async Task<ApiResponse<object>?> CreateShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postFedExCreateNewShipment"), body, ct);
    }

    /// <summary>Cancel a FedEx shipment.</summary>
    public async Task<ApiResponse<object>?> CancelShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/putFedExCancelShipment"), body, ct);
    }

    /// <summary>Validate a shipment (dry run).</summary>
    public async Task<ApiResponse<object>?> ValidateShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postFedExValidateShipment"), body, ct);
    }

    /// <summary>Create a FedEx return shipment.</summary>
    public async Task<ApiResponse<object>?> CreateReturnShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postFedExCreateNewReturnShipment"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Track a shipment by tracking number.</summary>
    public async Task<ApiResponse<object>?> TrackAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRetrieveTrackingInfoByTrackingNumber"), body, ct);
    }

    /// <summary>Track a multi-piece shipment.</summary>
    public async Task<ApiResponse<object>?> TrackMultiPieceAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRetrieveTrackingInfoForMultiPieceShipment"), body, ct);
    }

    /// <summary>Register for tracking notifications.</summary>
    public async Task<ApiResponse<object>?> RegisterTrackingNotificationAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRegisterForTrackingNotification"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Carrier Pickup
    // -----------------------------------------------------------------------

    /// <summary>Create a FedEx carrier pickup request.</summary>
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/postFedExCreateCarrierPickupRequest"), body, ct);
    }

    /// <summary>Cancel a FedEx carrier pickup request.</summary>
    public async Task<ApiResponse<object>?> CancelPickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/putFedExCancelCarrierPickupRequest"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Location Search
    // -----------------------------------------------------------------------

    /// <summary>Search valid FedEx locations.</summary>
    public async Task<ApiResponse<object>?> SearchLocationsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/LocationSearch/postFedExSearchValidLocations"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Service Standards
    // -----------------------------------------------------------------------

    /// <summary>Get FedEx service standards and transit times.</summary>
    public async Task<ApiResponse<object>?> GetServiceStandardsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/ServiceStandards/postFedExRetrieveServicesAndTransitTimes"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Freight
    // -----------------------------------------------------------------------

    /// <summary>Get a FedEx freight rate quote.</summary>
    public async Task<ApiResponse<object>?> GetFreightRateAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Freight/postFedExGetFreightRateQuote"), body, ct);
    }

    /// <summary>Create a FedEx freight shipment.</summary>
    public async Task<ApiResponse<object>?> CreateFreightShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Freight/postFedExCreateFreightShipment"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Ground Close
    // -----------------------------------------------------------------------

    /// <summary>FedEx Ground close with documents.</summary>
    public async Task<ApiResponse<object>?> GroundCloseAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/GroundClose/postFedExCloseWithDocuments"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Trade Documents
    // -----------------------------------------------------------------------

    /// <summary>Upload trade documents to FedEx.</summary>
    public async Task<ApiResponse<object>?> UploadTradeDocumentsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Trade/postFedExUploadTradeDocuments"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Open Ship (Multi-Package)
    // -----------------------------------------------------------------------

    /// <summary>Create a FedEx open shipment.</summary>
    public async Task<ApiResponse<object>?> CreateOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExCreateOpenShipment"), body, ct);
    }

    /// <summary>Add packages to a FedEx open shipment.</summary>
    public async Task<ApiResponse<object>?> AddPackagesToOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExAddPackagesToOpenShipment"), body, ct);
    }

    /// <summary>Confirm and finalize a FedEx open shipment.</summary>
    public async Task<ApiResponse<object>?> ConfirmOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExConfirmOpenShipment"), body, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// FedEx carrier operations via the VisionSuite API proxy (V3 endpoints). All of them are deprecated writes:
/// they need a user token (API keys get 403 <c>API_KEY_SCOPE_REQUIRED</c>) and stop working when the proxy is
/// retired on 2027-06-30. Use <see cref="ShippingResource"/> for FedEx rates and labels.
/// </summary>
public sealed class FedExResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="FedExResource"/>.</summary>
    public FedExResource(FlexOpsClient client) => _client = client;

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    /// <summary>Validate and correct a domestic address via FedEx.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/postFedExValidateAndCorrectDomesticAddress"), body, ct);
    }

    /// <summary>Validate a postal code via FedEx.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ValidatePostalCodeAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/postFedExValidatePostalCode"), body, ct);
    }

    /// <summary>Get FedEx rate and transit times.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postRetrieveFedExRateAndTransitTimesAsync"), body, ct);
    }

    /// <summary>Cancel a FedEx shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/putFedExCancelShipment"), body, ct);
    }

    /// <summary>Validate a shipment (dry run).</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ValidateShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postFedExValidateShipment"), body, ct);
    }

    /// <summary>Track a shipment by tracking number.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> TrackAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRetrieveTrackingInfoByTrackingNumber"), body, ct);
    }

    /// <summary>Track a multi-piece shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> TrackMultiPieceAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRetrieveTrackingInfoForMultiPieceShipment"), body, ct);
    }

    /// <summary>Register for tracking notifications.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> RegisterTrackingNotificationAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/postFedExRegisterForTrackingNotification"), body, ct);
    }

    /// <summary>Create a FedEx carrier pickup request.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/postFedExCreateCarrierPickupRequest"), body, ct);
    }

    /// <summary>Cancel a FedEx carrier pickup request.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelPickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/putFedExCancelCarrierPickupRequest"), body, ct);
    }

    /// <summary>Search valid FedEx locations.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> SearchLocationsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/LocationSearch/postFedExSearchValidLocations"), body, ct);
    }

    /// <summary>Get FedEx service standards and transit times.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetServiceStandardsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/ServiceStandards/postFedExRetrieveServicesAndTransitTimes"), body, ct);
    }

    /// <summary>Get a FedEx freight rate quote.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetFreightRateAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Freight/postFedExGetFreightRateQuote"), body, ct);
    }

    /// <summary>Create a FedEx freight shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreateFreightShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Freight/postFedExCreateFreightShipment"), body, ct);
    }

    /// <summary>FedEx Ground close with documents.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GroundCloseAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/GroundClose/postFedExCloseWithDocuments"), body, ct);
    }

    /// <summary>Upload trade documents to FedEx.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> UploadTradeDocumentsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Trade/postFedExUploadTradeDocuments"), body, ct);
    }

    /// <summary>Create a FedEx open shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreateOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExCreateOpenShipment"), body, ct);
    }

    /// <summary>Add packages to a FedEx open shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> AddPackagesToOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExAddPackagesToOpenShipment"), body, ct);
    }

    /// <summary>Confirm and finalize a FedEx open shipment.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> ConfirmOpenShipmentAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/OpenShip/postFedExConfirmOpenShipment"), body, ct);
    }
}

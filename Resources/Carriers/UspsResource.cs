// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// USPS carrier operations via the VisionSuite API proxy (V3 endpoints).
/// All methods proxy through <c>/api/ApiProxy/api/v3/...</c>.
/// </summary>
public sealed class UspsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="UspsResource"/>.</summary>
    public UspsResource(FlexOpsClient client) => _client = client;

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate and correct a US address via USPS.</summary>
    public async Task<ApiResponse<object>?> ValidateAddressAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/getUspsValidateAndCorrectAddress"), parameters, ct);
    }

    /// <summary>City/State lookup by ZIP code.</summary>
    public async Task<ApiResponse<object>?> CityStateLookupAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/getUspsCityStateLookupByZipCode"), parameters, ct);
    }

    /// <summary>ZIP code lookup by address.</summary>
    public async Task<ApiResponse<object>?> ZipCodeLookupAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/AddressValidation/getUspsZipCodeLookupByAddress"), parameters, ct);
    }

    // -----------------------------------------------------------------------
    // Rate Calculator
    // -----------------------------------------------------------------------

    /// <summary>Search domestic shipping base rates.</summary>
    public async Task<ApiResponse<object>?> GetDomesticRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchDomesticBaseRates"), body, ct);
    }

    /// <summary>Search eligible domestic products.</summary>
    public async Task<ApiResponse<object>?> GetDomesticProductsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleDomesticProducts"), body, ct);
    }

    /// <summary>Search eligible domestic prices.</summary>
    public async Task<ApiResponse<object>?> GetDomesticPricesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleDomesticPrices"), body, ct);
    }

    /// <summary>Search international base rates.</summary>
    public async Task<ApiResponse<object>?> GetInternationalRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchInternationalBaseRates"), body, ct);
    }

    /// <summary>Search eligible international prices.</summary>
    public async Task<ApiResponse<object>?> GetInternationalPricesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleInternationalPrices"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Shipping / Labels
    // -----------------------------------------------------------------------

    /// <summary>
    /// Generate a domestic USPS shipping label for an order via the reliable Gateway path
    /// (<c>POST api/shipping/labels</c>). Supply a <c>LabelRequest</c> body with
    /// <c>carrierCode: "USPS"</c> and <c>orderId</c> set: the order's ownership, status,
    /// ship-method and addresses are validated server-side and postage is settled atomically.
    /// Physical package fields (weight, dimensions, alcohol/dry-ice, confirmation, insurance)
    /// are supplied here — they are not stored on the order. Returns the raw label.
    /// </summary>
    /// <param name="request">A <c>LabelRequest</c>: origin, destination, package,
    /// <c>carrierCode: "USPS"</c>, <c>serviceCode</c>, and <c>orderId</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<Label?> CreateDomesticLabelAsync(object request, CancellationToken ct = default)
    {
        return await new ShippingResource(_client).CreateLabelAsync(request, ct);
    }

    /// <summary>Generate a domestic return shipping label.</summary>
    public async Task<ApiResponse<object>?> CreateReturnLabelAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postUspsGenerateDomesticReturnsShippingLabel"), body, ct);
    }

    /// <summary>Generate an international shipping label.</summary>
    public async Task<ApiResponse<object>?> CreateInternationalLabelAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/postUspsGenerateInternationalShippingLabel"), body, ct);
    }

    /// <summary>Cancel a domestic shipment label.</summary>
    public async Task<ApiResponse<object>?> CancelDomesticLabelAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/cancelUspsDomesticShipmentLabel"), ct);
    }

    /// <summary>Cancel an international shipment label.</summary>
    public async Task<ApiResponse<object>?> CancelInternationalLabelAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/cancelUspsInternationalShipmentLabel"), ct);
    }

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Get tracking summary information.</summary>
    public async Task<ApiResponse<object>?> TrackSummaryAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/getUspsTrackingSummaryInformation"), parameters, ct);
    }

    /// <summary>Get detailed tracking information.</summary>
    public async Task<ApiResponse<object>?> TrackDetailAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/Tracking/getUspsTrackingDetailInformation"), parameters, ct);
    }

    // -----------------------------------------------------------------------
    // Carrier Pickup
    // -----------------------------------------------------------------------

    /// <summary>Schedule a USPS carrier pickup.</summary>
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/postUspsCreateCarrierPickupSchedule"), body, ct);
    }

    /// <summary>Cancel a USPS carrier pickup.</summary>
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/cancelUspsCarrierPickupSchedule"), ct);
    }

    // -----------------------------------------------------------------------
    // Scan Form
    // -----------------------------------------------------------------------

    /// <summary>Create a USPS scan form (SCAN/manifest).</summary>
    public async Task<ApiResponse<object>?> CreateScanFormAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/ScanForm/postUspsCreateScanFormLabelShipment"), body, ct);
    }

    // -----------------------------------------------------------------------
    // Service Standards & Location Search
    // -----------------------------------------------------------------------

    /// <summary>Get USPS delivery standards estimates.</summary>
    public async Task<ApiResponse<object>?> GetDeliveryStandardsAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/ServiceStandards/getUspsGetDeliveryStandardsEstimates"), parameters, ct);
    }

    /// <summary>Find valid USPS drop-off locations.</summary>
    public async Task<ApiResponse<object>?> FindDropOffLocationsAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/LocationSearch/getUspsFindValidDropOffLocations"), parameters, ct);
    }

    /// <summary>Find valid USPS post office locations.</summary>
    public async Task<ApiResponse<object>?> FindPostOfficesAsync(object parameters, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/LocationSearch/getUspsFindValidPostOfficeLocations"), parameters, ct);
    }
}

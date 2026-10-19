// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources.Carriers;

/// <summary>
/// USPS operations via the VisionSuite API proxy (V3 endpoints): GET lookups with the parameters in the query
/// string, domestic labels through the Gateway label route, and deprecated proxy writes.
/// </summary>
/// <remarks>
/// The lookups work with API keys. The <see cref="ObsoleteAttribute">obsolete</see> writes need a user token (API
/// keys get 403 <c>API_KEY_SCOPE_REQUIRED</c>) and stop working when the proxy is retired on 2027-06-30; use
/// <see cref="ShippingResource"/> for rates and labels, <see cref="PickupsResource"/> for pickups and
/// <see cref="ScanFormsResource"/> for scan forms. Return and international proxy labels are closed (410 Gone).
/// </remarks>
public sealed class UspsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="UspsResource"/>.</summary>
    public UspsResource(FlexOpsClient client) => _client = client;

    private Task<ApiResponse<object>?> GetAsync(string path, IReadOnlyDictionary<string, string>? parameters, CancellationToken ct)
        => _client.GetAsync<ApiResponse<object>>(CarriersResource.ProxyPath(path, parameters), ct);

    // -----------------------------------------------------------------------
    // Address Validation
    // -----------------------------------------------------------------------

    /// <summary>Validate and correct a US address via USPS.</summary>
    public Task<ApiResponse<object>?> ValidateAddressAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/AddressValidation/getUspsValidateAndCorrectAddress", parameters, ct);

    /// <summary>City/State lookup by ZIP code.</summary>
    public Task<ApiResponse<object>?> CityStateLookupAsync(string zipCode, CancellationToken ct = default)
        => GetAsync("api/v3/AddressValidation/getUspsCityStateLookupByZipCode", new Dictionary<string, string> { ["zipCode"] = zipCode }, ct);

    /// <summary>ZIP code lookup by address.</summary>
    public Task<ApiResponse<object>?> ZipCodeLookupAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/AddressValidation/getUspsZipCodeLookupByAddress", parameters, ct);

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

    // -----------------------------------------------------------------------
    // Tracking
    // -----------------------------------------------------------------------

    /// <summary>Get tracking summary information.</summary>
    public Task<ApiResponse<object>?> TrackSummaryAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/Tracking/getUspsTrackingSummaryInformation", parameters, ct);

    /// <summary>Get detailed tracking information.</summary>
    public Task<ApiResponse<object>?> TrackDetailAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/Tracking/getUspsTrackingDetailInformation", parameters, ct);

    // -----------------------------------------------------------------------
    // Service Standards & Location Search
    // -----------------------------------------------------------------------

    /// <summary>Get USPS delivery standards estimates.</summary>
    public Task<ApiResponse<object>?> GetDeliveryStandardsAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/ServiceStandards/getUspsGetDeliveryStandardsEstimates", parameters, ct);

    /// <summary>Find valid USPS drop-off locations.</summary>
    public Task<ApiResponse<object>?> FindDropOffLocationsAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/LocationSearch/getUspsFindValidDropOffLocations", parameters, ct);

    /// <summary>Find valid USPS post office locations.</summary>
    public Task<ApiResponse<object>?> FindPostOfficesAsync(IReadOnlyDictionary<string, string>? parameters = null, CancellationToken ct = default)
        => GetAsync("api/v3/LocationSearch/getUspsFindValidPostOfficeLocations", parameters, ct);

    // -----------------------------------------------------------------------
    // Deprecated: legacy proxy writes (user token only, sunset 2027-06-30)
    // -----------------------------------------------------------------------

    private static string Proxy(string path) => $"api/ApiProxy/{path}";

    /// <summary>Search domestic shipping base rates.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetDomesticRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchDomesticBaseRates"), body, ct);
    }

    /// <summary>Search eligible domestic products.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetDomesticProductsAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleDomesticProducts"), body, ct);
    }

    /// <summary>Search eligible domestic prices.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetDomesticPricesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleDomesticPrices"), body, ct);
    }

    /// <summary>Search international base rates.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetInternationalRatesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchInternationalBaseRates"), body, ct);
    }

    /// <summary>Search eligible international prices.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> GetInternationalPricesAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/RateCalculator/postUspsSearchEligibleInternationalPrices"), body, ct);
    }

    /// <summary>Cancel a domestic shipment label.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelDomesticLabelAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/cancelUspsDomesticShipmentLabel"), ct);
    }

    /// <summary>Cancel an international shipment label.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelInternationalLabelAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/Shipping/cancelUspsInternationalShipmentLabel"), ct);
    }

    /// <summary>Schedule a USPS carrier pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreatePickupAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/postUspsCreateCarrierPickupSchedule"), body, ct);
    }

    /// <summary>Cancel a USPS carrier pickup.</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CancelPickupAsync(CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(Proxy("api/v3/CarrierPickup/cancelUspsCarrierPickupSchedule"), ct);
    }

    /// <summary>Create a USPS scan form (SCAN/manifest).</summary>
    [Obsolete(CarriersResource.LegacyProxyWriteMessage)]
    public async Task<ApiResponse<object>?> CreateScanFormAsync(object body, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(Proxy("api/v3/ScanForm/postUspsCreateScanFormLabelShipment"), body, ct);
    }
}

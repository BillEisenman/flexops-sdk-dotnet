// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Harmonized System (HS) code search, lookup, and landed cost estimation for international shipments.
/// </summary>
public sealed class HsCodesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="HsCodesResource"/>.</summary>
    public HsCodesResource(FlexOpsClient client) => _client = client;

    /// <summary>Search for HS codes by keyword, optionally filtered by destination country.</summary>
    /// <param name="query">Search terms describing the product.</param>
    /// <param name="destinationCountry">Optional ISO-3166-1 alpha-2 destination country code.</param>
    /// <param name="maxResults">Maximum number of results to return (default 10).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> SearchAsync(
        string query,
        string? destinationCountry = null,
        int maxResults = 10,
        CancellationToken ct = default)
    {
        var queryString = $"shipping/hs-codes/search?query={Uri.EscapeDataString(query)}&maxResults={maxResults}";
        if (!string.IsNullOrEmpty(destinationCountry))
        {
            queryString += $"&destinationCountry={Uri.EscapeDataString(destinationCountry)}";
        }

        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath(queryString), ct);
    }

    /// <summary>Look up details for a specific HS code.</summary>
    /// <param name="code">The HS code to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> LookupAsync(string code, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"shipping/hs-codes/{Uri.EscapeDataString(code)}"), ct);
    }

    /// <summary>Estimate the landed cost (duties, taxes, fees) for an international shipment.</summary>
    /// <param name="request">Landed cost estimation request including items, origin, and destination.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> EstimateLandedCostAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping/landed-cost"), request, ct);
    }
}

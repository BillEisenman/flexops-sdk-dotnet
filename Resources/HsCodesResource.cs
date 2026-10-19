// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Globalization;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Harmonized System (HS) code search, lookup, and landed cost estimation for international shipments.
/// </summary>
/// <remarks>
/// These <c>api/shipping/*</c> routes are not workspace-scoped (the workspace comes from the API key) and answer
/// with the bare result, not an <see cref="ApiResponse{T}"/> envelope. They need international shipping enabled
/// on the workspace; otherwise the Gateway answers 400 <c>FeatureDisabled</c>.
/// </remarks>
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
    public async Task<List<HsCodeResult>?> SearchAsync(
        string query,
        string? destinationCountry = null,
        int maxResults = 10,
        CancellationToken ct = default)
    {
        var path = FlexOpsClient.WithQuery("api/shipping/hs-codes/search",
        [
            new("query", query),
            new("destinationCountry", destinationCountry),
            new("maxResults", maxResults.ToString(CultureInfo.InvariantCulture)),
        ]);
        return await _client.GetAsync<List<HsCodeResult>>(path, ct);
    }

    /// <summary>Look up details for a specific HS code.</summary>
    /// <param name="code">The HS code to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<HsCodeResult?> LookupAsync(string code, CancellationToken ct = default)
    {
        return await _client.GetAsync<HsCodeResult>($"api/shipping/hs-codes/{Uri.EscapeDataString(code)}", ct);
    }

    /// <summary>Estimate the landed cost (duties, taxes, fees) for an international shipment.</summary>
    /// <param name="request">Destination country, items and shipping cost.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<LandedCostEstimate?> EstimateLandedCostAsync(LandedCostRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<LandedCostEstimate>("api/shipping/landed-cost", request, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Shipping insurance quotes, purchases, and claims.
/// </summary>
public sealed class InsuranceResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="InsuranceResource"/>.</summary>
    public InsuranceResource(FlexOpsClient client) => _client = client;

    /// <summary>Get available insurance providers for this workspace.</summary>
    public async Task<ApiResponse<string[]>?> GetProvidersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<string[]>>(_client.WsPath("insurance/providers"), ct);
    }

    /// <summary>Get an insurance quote.</summary>
    public async Task<ApiResponse<object>?> GetQuoteAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("insurance/quote"), request, ct);
    }

    /// <summary>Purchase insurance for a shipment.</summary>
    public async Task<ApiResponse<object>?> PurchaseAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("insurance/purchase"), request, ct);
    }

    /// <summary>Void an insurance policy.</summary>
    public async Task<ApiResponse<object>?> VoidAsync(string policyId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"insurance/policies/{policyId}"), ct);
    }

    /// <summary>File an insurance claim.</summary>
    public async Task<ApiResponse<object>?> FileClaimAsync(string policyId, object claim, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"insurance/policies/{policyId}/claims"), claim, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Shipping insurance quotes, purchases, and claims. Responses use the <see cref="ApiResponse{T}"/> envelope.
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
    public async Task<ApiResponse<object>?> GetQuoteAsync(InsuranceQuoteRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("insurance/quote"), request, ct);
    }

    /// <summary>Purchase an insurance policy for a shipment (201 Created).</summary>
    /// <remarks>
    /// Pass <paramref name="idempotencyKey"/> and reuse it if you retry after a timeout or unknown outcome, so a
    /// retried purchase is not charged twice. Without one, the SDK sends a new key for this call only.
    /// </remarks>
    public async Task<ApiResponse<object>?> PurchaseAsync(PurchaseInsuranceRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("insurance/policies"), request, ct, idempotencyKey);
    }

    /// <summary>Void (cancel) an insurance policy.</summary>
    public async Task<ApiResponse<object>?> VoidAsync(string policyId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"insurance/policies/{policyId}"), ct);
    }

    /// <summary>File a claim against a policy (201 Created).</summary>
    public async Task<ApiResponse<object>?> FileClaimAsync(FileInsuranceClaimRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("insurance/claims"), request, ct);
    }
}

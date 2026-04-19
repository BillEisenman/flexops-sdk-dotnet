// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Return authorization (RMA) management.
/// </summary>
public sealed class ReturnsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ReturnsResource"/>.</summary>
    public ReturnsResource(FlexOpsClient client) => _client = client;

    /// <summary>List all return authorizations.</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("returns"), ct);
    }

    /// <summary>Get a return authorization by ID.</summary>
    public async Task<ApiResponse<object>?> GetAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}"), ct);
    }

    /// <summary>Create a return authorization (RMA).</summary>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("returns"), request, ct);
    }

    /// <summary>Approve a return authorization.</summary>
    public async Task<ApiResponse<object>?> ApproveAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/approve"), null, ct);
    }

    /// <summary>Reject a return authorization.</summary>
    public async Task<ApiResponse<object>?> RejectAsync(string returnId, string reason, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/reject"), new { reason }, ct);
    }

    /// <summary>Cancel a return authorization.</summary>
    public async Task<ApiResponse<object>?> CancelAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/cancel"), null, ct);
    }

    /// <summary>Generate a return label for an approved RMA.</summary>
    public async Task<ApiResponse<object>?> GenerateLabelAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/label"), null, ct);
    }

    /// <summary>Mark items as received.</summary>
    public async Task<ApiResponse<object>?> MarkReceivedAsync(string returnId, object items, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/receive"), items, ct);
    }

    /// <summary>Process refund for a return.</summary>
    public async Task<ApiResponse<object>?> ProcessRefundAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/refund"), null, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Carbon offset operations: offset labels, query emissions, and batch offset.
/// </summary>
public sealed class OffsetResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="OffsetResource"/>.</summary>
    public OffsetResource(FlexOpsClient client) => _client = client;

    /// <summary>Purchase a carbon offset for a specific label.</summary>
    /// <param name="labelId">The label ID to offset.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> OffsetAsync(string labelId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"shipping/labels/{labelId}/offset"), null, ct);
    }

    /// <summary>Get the estimated CO2 emissions for a specific label.</summary>
    /// <param name="labelId">The label ID to query.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetEmissionsAsync(string labelId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"shipping/labels/{labelId}/emissions"), ct);
    }

    /// <summary>Purchase carbon offsets for multiple labels in a single request.</summary>
    /// <param name="labelIds">The label IDs to offset.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> BatchOffsetAsync(IEnumerable<string> labelIds, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping/labels/offset/batch"), new { labelIds }, ct);
    }
}

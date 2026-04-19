// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Carrier pickup scheduling and management.
/// </summary>
public sealed class PickupsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="PickupsResource"/>.</summary>
    public PickupsResource(FlexOpsClient client) => _client = client;

    /// <summary>Schedule a carrier pickup.</summary>
    public async Task<ApiResponse<object>?> ScheduleAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("pickups"), request, ct);
    }

    /// <summary>List scheduled pickups.</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("pickups"), ct);
    }

    /// <summary>Get pickup details.</summary>
    public async Task<ApiResponse<object>?> GetAsync(string pickupId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"pickups/{pickupId}"), ct);
    }

    /// <summary>Cancel a scheduled pickup.</summary>
    public async Task<ApiResponse<object>?> CancelAsync(string pickupId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"pickups/{pickupId}"), ct);
    }
}

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
    public async Task<ApiResponse<object>?> ScheduleAsync(SchedulePickupRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("pickups"), request, ct);
    }

    /// <summary>List scheduled pickups (<c>GET pickups/scheduled</c>).</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("pickups/scheduled"), ct);
    }

    /// <summary>Get pickup details from the carrier.</summary>
    /// <param name="pickupId">The carrier's pickup confirmation number.</param>
    /// <param name="carrierCode">The pickup's carrier. The Gateway defaults to <c>usps</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetAsync(string pickupId, string carrierCode = "usps", CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(
            FlexOpsClient.WithQuery(_client.WsPath($"pickups/{Uri.EscapeDataString(pickupId)}"), [new("carrierCode", carrierCode)]), ct);
    }

    /// <summary>Cancel a scheduled pickup.</summary>
    /// <param name="pickupId">The carrier's pickup confirmation number.</param>
    /// <param name="carrierCode">The pickup's carrier. The Gateway defaults to <c>usps</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> CancelAsync(string pickupId, string carrierCode = "usps", CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(
            FlexOpsClient.WithQuery(_client.WsPath($"pickups/{Uri.EscapeDataString(pickupId)}"), [new("carrierCode", carrierCode)]), ct);
    }
}

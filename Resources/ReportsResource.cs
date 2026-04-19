// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Scheduled report management: create, configure, and manage report schedules.
/// </summary>
public sealed class ReportsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ReportsResource"/>.</summary>
    public ReportsResource(FlexOpsClient client) => _client = client;

    /// <summary>List all report schedules for the workspace.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("report-schedules"), ct);
    }

    /// <summary>Get a report schedule by ID.</summary>
    /// <param name="id">The report schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetAsync(string id, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"report-schedules/{id}"), ct);
    }

    /// <summary>Create a new report schedule.</summary>
    /// <param name="request">The report schedule configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("report-schedules"), request, ct);
    }

    /// <summary>Update an existing report schedule.</summary>
    /// <param name="id">The report schedule ID.</param>
    /// <param name="request">The updated schedule configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> UpdateAsync(string id, object request, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath($"report-schedules/{id}"), request, ct);
    }

    /// <summary>Delete a report schedule.</summary>
    /// <param name="id">The report schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"report-schedules/{id}"), ct);
    }
}

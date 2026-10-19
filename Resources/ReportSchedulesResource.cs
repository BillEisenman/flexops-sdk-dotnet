// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-10-19
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Scheduled reports emailed to recipients (<c>/api/workspaces/{workspaceId}/report-schedules</c>). Responses use the
/// <see cref="ApiResponse{T}"/> envelope, except delete (204 No Content).
/// </summary>
/// <remarks>
/// Reads work with any API key or a user token. Writes (create, update, delete, toggle) need a user token
/// (<see cref="AuthResource.LoginAsync"/>) or an API key granted the <c>report-schedules:write</c> scope; an API key
/// without it gets 403 <c>API_KEY_SCOPE_REQUIRED</c> with <c>requiredScopes</c>.
/// </remarks>
public sealed class ReportSchedulesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ReportSchedulesResource"/>.</summary>
    public ReportSchedulesResource(FlexOpsClient client) => _client = client;

    /// <summary>List the workspace's report schedules.</summary>
    public async Task<ApiResponse<ReportSchedule[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<ReportSchedule[]>>(_client.WsPath("report-schedules"), ct);
    }

    /// <summary>Get a report schedule.</summary>
    public async Task<ApiResponse<ReportSchedule>?> GetAsync(string id, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<ReportSchedule>>(_client.WsPath($"report-schedules/{id}"), ct);
    }

    /// <summary>Create a report schedule (201 Created).</summary>
    public async Task<ApiResponse<ReportSchedule>?> CreateAsync(CreateReportScheduleRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<ReportSchedule>>(_client.WsPath("report-schedules"), request, ct);
    }

    /// <summary>Update a report schedule. Only the properties set on <paramref name="request"/> change.</summary>
    public async Task<ApiResponse<ReportSchedule>?> UpdateAsync(string id, UpdateReportScheduleRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PutAsync<ApiResponse<ReportSchedule>>(_client.WsPath($"report-schedules/{id}"), request, ct);
    }

    /// <summary>Delete a report schedule (204 No Content).</summary>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await _client.DeleteAsync<object>(_client.WsPath($"report-schedules/{id}"), ct);
    }

    /// <summary>Enable a disabled schedule, or disable an enabled one. The envelope carries no data.</summary>
    public async Task<ApiResponse<object>?> ToggleAsync(string id, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"report-schedules/{id}/toggle"), null, ct);
    }
}

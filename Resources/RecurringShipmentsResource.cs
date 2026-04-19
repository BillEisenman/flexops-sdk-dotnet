// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Recurring shipment schedule management: CRUD operations plus pause, resume, and on-demand trigger.
/// </summary>
public sealed class RecurringShipmentsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="RecurringShipmentsResource"/>.</summary>
    public RecurringShipmentsResource(FlexOpsClient client) => _client = client;

    // -----------------------------------------------------------------------
    // CRUD
    // -----------------------------------------------------------------------

    /// <summary>List all recurring shipment schedules for the workspace.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("recurring-shipments"), ct);
    }

    /// <summary>Get a recurring shipment schedule by ID.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetAsync(string id, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}"), ct);
    }

    /// <summary>Create a new recurring shipment schedule.</summary>
    /// <param name="request">The schedule configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("recurring-shipments"), request, ct);
    }

    /// <summary>Update an existing recurring shipment schedule.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="request">The updated schedule configuration.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> UpdateAsync(string id, object request, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}"), request, ct);
    }

    /// <summary>Delete a recurring shipment schedule.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}"), ct);
    }

    // -----------------------------------------------------------------------
    // Actions
    // -----------------------------------------------------------------------

    /// <summary>Pause a recurring shipment schedule.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> PauseAsync(string id, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}/pause"), null, ct);
    }

    /// <summary>Resume a paused recurring shipment schedule.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> ResumeAsync(string id, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}/resume"), null, ct);
    }

    /// <summary>Trigger an immediate run of a recurring shipment schedule outside its normal cadence.</summary>
    /// <param name="id">The recurring shipment schedule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> TriggerAsync(string id, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"recurring-shipments/{id}/trigger"), null, ct);
    }
}

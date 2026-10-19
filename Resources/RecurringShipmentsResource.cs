// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-10-19
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Recurring shipment schedules (<c>/api/workspaces/{workspaceId}/recurring-shipments</c>). A run needs a destination
/// address; a shipping preset supplies the package and service. Responses are the bare JSON, not an
/// <see cref="ApiResponse{T}"/> envelope.
/// </summary>
/// <remarks>
/// Reads work with any API key or a user token. Writes (create, update, delete, pause, resume, trigger) need a user
/// token (<see cref="AuthResource.LoginAsync"/>) or an API key granted the <c>recurring-shipments:write</c> scope;
/// an API key without it gets 403 <c>API_KEY_SCOPE_REQUIRED</c> with <c>requiredScopes</c>.
/// </remarks>
public sealed class RecurringShipmentsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="RecurringShipmentsResource"/>.</summary>
    public RecurringShipmentsResource(FlexOpsClient client) => _client = client;

    /// <summary>List the workspace's recurring shipment schedules.</summary>
    public async Task<List<RecurringShipment>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<List<RecurringShipment>>(_client.WsPath("recurring-shipments"), ct);
    }

    /// <summary>Get a schedule. A missing schedule throws <see cref="FlexOpsException"/> with status 404.</summary>
    public async Task<RecurringShipment?> GetAsync(string id, CancellationToken ct = default)
    {
        return await _client.GetAsync<RecurringShipment>(_client.WsPath($"recurring-shipments/{id}"), ct);
    }

    /// <summary>Create a schedule (201 Created).</summary>
    public async Task<RecurringShipment?> CreateAsync(CreateRecurringShipmentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<RecurringShipment>(_client.WsPath("recurring-shipments"), request, ct);
    }

    /// <summary>Update a schedule. Only the properties set on <paramref name="request"/> change.</summary>
    public async Task<RecurringShipment?> UpdateAsync(string id, UpdateRecurringShipmentRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PutAsync<RecurringShipment>(_client.WsPath($"recurring-shipments/{id}"), request, ct);
    }

    /// <summary>Delete a schedule (204 No Content).</summary>
    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        await _client.DeleteAsync<object>(_client.WsPath($"recurring-shipments/{id}"), ct);
    }

    /// <summary>Pause a schedule (204 No Content).</summary>
    public async Task PauseAsync(string id, CancellationToken ct = default)
    {
        await _client.PostAsync<object>(_client.WsPath($"recurring-shipments/{id}/pause"), null, ct);
    }

    /// <summary>Resume a paused schedule (204 No Content).</summary>
    public async Task ResumeAsync(string id, CancellationToken ct = default)
    {
        await _client.PostAsync<object>(_client.WsPath($"recurring-shipments/{id}/resume"), null, ct);
    }

    /// <summary>Run a schedule now: it creates an order immediately (204 No Content).</summary>
    /// <remarks>
    /// Pass <paramref name="idempotencyKey"/> and reuse it if you retry after a timeout or unknown outcome, so a
    /// retried trigger does not create a second order. Without one, the SDK sends a new key for this call only.
    /// </remarks>
    public async Task TriggerAsync(string id, string? idempotencyKey = null, CancellationToken ct = default)
    {
        await _client.PostAsync<object>(_client.WsPath($"recurring-shipments/{id}/trigger"), null, ct, idempotencyKey);
    }
}

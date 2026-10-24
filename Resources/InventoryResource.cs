// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Read-only inventory lookups via the VisionSuite API proxy. The proxy refuses writes from API keys.
/// </summary>
/// <remarks>
/// With an API key, every method requires the <c>inventory:read</c> permission; a key without it gets 403
/// <c>API_KEY_SCOPE_REQUIRED</c>. Keys created with the default permissions include <c>inventory:read</c>.
/// </remarks>
public sealed class InventoryResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="InventoryResource"/>.</summary>
    public InventoryResource(FlexOpsClient client) => _client = client;

    /// <summary>Get a warehouse inventory snapshot.</summary>
    public async Task<ApiResponse<object>?> GetWarehouseSnapshotAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>("api/ApiProxy/api/v1/Inventory/getWarehouseInventorySnapshot", ct);
    }

    /// <summary>Get a complete inventory snapshot across all warehouses.</summary>
    public async Task<ApiResponse<object>?> GetCompleteSnapshotAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>("api/ApiProxy/api/v1/Inventory/getCompleteInventorySnapshot", ct);
    }

    /// <summary>Get the list of all part numbers.</summary>
    public async Task<ApiResponse<object>?> GetPartNumbersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>("api/ApiProxy/api/v1/Inventory/getPartNumberList", ct);
    }
}

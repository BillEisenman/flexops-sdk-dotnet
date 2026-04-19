// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Inventory management via the VisionSuite API proxy.
/// </summary>
public sealed class InventoryResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="InventoryResource"/>.</summary>
    public InventoryResource(FlexOpsClient client) => _client = client;

    /// <summary>Post a new ASN (Advance Shipment Notice) receipt.</summary>
    public async Task<ApiResponse<object>?> PostAsnReceiptAsync(object receipt, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>("api/ApiProxy/api/v1/Inventory/postNewAsnReceipt", receipt, ct);
    }

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

    /// <summary>Update customer inventory (V2).</summary>
    public async Task<ApiResponse<object>?> UpdateInventoryAsync(object data, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>("api/ApiProxy/api/v2/Inventory/postCustomerInventoryUpdate", data, ct);
    }
}

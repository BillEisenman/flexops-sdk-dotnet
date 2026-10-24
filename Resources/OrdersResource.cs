// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Read-only order lookups via the VisionSuite API proxy (V1 Order endpoints). The proxy refuses writes from API keys.
/// All methods proxy through <c>/api/ApiProxy/api/v1/Order/...</c>.
/// </summary>
/// <remarks>
/// With an API key, every method requires the <c>orders:read</c> permission; a key without it gets 403
/// <c>API_KEY_SCOPE_REQUIRED</c>. Keys created with the default permissions include <c>orders:read</c>.
/// </remarks>
public sealed class OrdersResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="OrdersResource"/>.</summary>
    public OrdersResource(FlexOpsClient client) => _client = client;

    private static string Path(string endpoint) => $"api/ApiProxy/api/v1/Order/{endpoint}";

    /// <summary>Get new orders awaiting processing.</summary>
    public async Task<ApiResponse<object>?> GetNewOrdersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getNewOrderList"), ct);
    }

    /// <summary>Get all orders filtered by status.</summary>
    public async Task<ApiResponse<object>?> GetByStatusAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getAllOrderListByStatus"), ct);
    }

    /// <summary>Get complete order details by order number.</summary>
    public async Task<ApiResponse<object>?> GetDetailsAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path($"getCompleteOrderDetailsByOrderNumber?orderNumber={Uri.EscapeDataString(orderNumber)}"), ct);
    }

    /// <summary>Get extended order details by order number.</summary>
    public async Task<ApiResponse<object>?> GetExtendedDetailsAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path($"getExtendedOrderDetailsByOrderNumber?orderNumber={Uri.EscapeDataString(orderNumber)}"), ct);
    }

    /// <summary>Get order status by order number.</summary>
    public async Task<ApiResponse<object>?> GetStatusAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path($"getIndividualOrderStatusByOrderNumber?orderNumber={Uri.EscapeDataString(orderNumber)}"), ct);
    }

    /// <summary>Get all items for an order.</summary>
    public async Task<ApiResponse<object>?> GetItemsAsync(string orderNumber, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path($"getAllOrderItemsByOrderNumber?orderNumber={Uri.EscapeDataString(orderNumber)}"), ct);
    }

    /// <summary>Get available ship methods.</summary>
    public async Task<ApiResponse<object>?> GetShipMethodsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getAvailableShipMethodsList"), ct);
    }

    /// <summary>Get active warehouse list.</summary>
    public async Task<ApiResponse<object>?> GetWarehousesAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getActiveWarehouseList"), ct);
    }

    /// <summary>Get country name/code list.</summary>
    public async Task<ApiResponse<object>?> GetCountryCodesAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getCountryNameCodeList"), ct);
    }

    /// <summary>Get order status types.</summary>
    public async Task<ApiResponse<object>?> GetStatusTypesAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("getOrderStatusTypesList"), ct);
    }
}

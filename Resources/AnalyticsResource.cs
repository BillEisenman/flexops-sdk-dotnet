// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Analytics and reporting via the VisionSuite API proxy (V4 Analytics endpoints).
/// All methods proxy through <c>/api/ApiProxy/api/v4/Analytics/...</c>.
/// </summary>
public sealed class AnalyticsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="AnalyticsResource"/>.</summary>
    public AnalyticsResource(FlexOpsClient client) => _client = client;

    private static string Path(string endpoint) => $"api/ApiProxy/api/v4/Analytics/{endpoint}";

    /// <summary>Shipments trend over time.</summary>
    public async Task<ApiResponse<object>?> ShipmentsTrendAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("ShipmentsTrend"), ct);
    }

    /// <summary>Carrier usage summary.</summary>
    public async Task<ApiResponse<object>?> CarrierSummaryAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("CarrierSummary"), ct);
    }

    /// <summary>Top shipping destinations.</summary>
    public async Task<ApiResponse<object>?> TopDestinationsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("TopDestinations"), ct);
    }

    /// <summary>Inventory metrics (stock levels, low-stock alerts).</summary>
    public async Task<ApiResponse<object>?> InventoryMetricsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("InventoryMetrics"), ct);
    }

    /// <summary>Stock levels by warehouse.</summary>
    public async Task<ApiResponse<object>?> StockByWarehouseAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("StockByWarehouse"), ct);
    }

    /// <summary>Order metrics (volume, revenue).</summary>
    public async Task<ApiResponse<object>?> OrderMetricsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("OrderMetrics"), ct);
    }

    /// <summary>Order trend over time.</summary>
    public async Task<ApiResponse<object>?> OrderTrendAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("OrderTrend"), ct);
    }

    /// <summary>Top selling products.</summary>
    public async Task<ApiResponse<object>?> TopSellingProductsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("TopSellingProducts"), ct);
    }

    /// <summary>Returns metrics.</summary>
    public async Task<ApiResponse<object>?> ReturnsMetricsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("ReturnsMetrics"), ct);
    }

    /// <summary>Returns trend over time.</summary>
    public async Task<ApiResponse<object>?> ReturnsTrendAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("ReturnsTrend"), ct);
    }

    /// <summary>Return reasons breakdown.</summary>
    public async Task<ApiResponse<object>?> ReturnReasonsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("ReturnReasons"), ct);
    }

    /// <summary>Fulfillment performance metrics.</summary>
    public async Task<ApiResponse<object>?> PerformanceMetricsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("PerformanceMetrics"), ct);
    }

    /// <summary>Carrier delivery performance.</summary>
    public async Task<ApiResponse<object>?> CarrierPerformanceAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("CarrierPerformance"), ct);
    }

    /// <summary>Shipping cost analytics.</summary>
    public async Task<ApiResponse<object>?> ShippingCostAnalyticsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("ShippingCostAnalytics"), ct);
    }

    /// <summary>Delivery performance (on-time percentage).</summary>
    public async Task<ApiResponse<object>?> DeliveryPerformanceAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(Path("DeliveryPerformance"), ct);
    }
}

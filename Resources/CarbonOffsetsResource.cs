// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-10-19
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Globalization;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Carbon offsets for shipments: quotes, purchases, history and the workspace auto-offset setting
/// (<c>/api/workspaces/{workspaceId}/carbon-offsets</c>). Responses use the <see cref="ApiResponse{T}"/> envelope.
/// </summary>
public sealed class CarbonOffsetsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="CarbonOffsetsResource"/>.</summary>
    public CarbonOffsetsResource(FlexOpsClient client) => _client = client;

    /// <summary>Quote the carbon and cost of offsetting a shipment. Nothing is charged.</summary>
    public async Task<ApiResponse<object>?> QuoteAsync(CarbonOffsetQuoteRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("carbon-offsets/quote"), request, ct);
    }

    /// <summary>Buy a carbon offset for a shipment; the cost is debited from the wallet (201 Created).</summary>
    /// <remarks>
    /// Pass <paramref name="idempotencyKey"/> and reuse it if you retry after a timeout or unknown outcome, so a
    /// retried purchase is not charged twice. Without one, the SDK sends a new key for this call only.
    /// </remarks>
    public async Task<ApiResponse<object>?> PurchaseAsync(CarbonOffsetPurchaseRequest request, string? idempotencyKey = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("carbon-offsets/purchase"), request, ct, idempotencyKey);
    }

    /// <summary>List purchased offsets, one page at a time.</summary>
    /// <param name="page">Page number, from 1.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListAsync(int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var path = FlexOpsClient.WithQuery(_client.WsPath("carbon-offsets"),
        [
            new("page", page.ToString(CultureInfo.InvariantCulture)),
            new("pageSize", pageSize.ToString(CultureInfo.InvariantCulture)),
        ]);
        return await _client.GetAsync<ApiResponse<object[]>>(path, ct);
    }

    /// <summary>Get a purchased offset.</summary>
    public async Task<ApiResponse<object>?> GetAsync(string offsetId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"carbon-offsets/{offsetId}"), ct);
    }

    /// <summary>Get the workspace totals: carbon offset, cost and number of offsets.</summary>
    public async Task<ApiResponse<object>?> GetSummaryAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath("carbon-offsets/summary"), ct);
    }

    /// <summary>Get the workspace carbon offset settings.</summary>
    public async Task<ApiResponse<CarbonOffsetSettings>?> GetSettingsAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<CarbonOffsetSettings>>(_client.WsPath("carbon-offsets/settings"), ct);
    }

    /// <summary>Update the workspace carbon offset settings.</summary>
    public async Task<ApiResponse<CarbonOffsetSettings>?> UpdateSettingsAsync(CarbonOffsetSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return await _client.PutAsync<ApiResponse<CarbonOffsetSettings>>(_client.WsPath("carbon-offsets/settings"), settings, ct);
    }
}

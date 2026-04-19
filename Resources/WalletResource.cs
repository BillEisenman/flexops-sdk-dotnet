// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Wallet balance and transaction management.
/// </summary>
public sealed class WalletResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="WalletResource"/>.</summary>
    public WalletResource(FlexOpsClient client) => _client = client;

    /// <summary>Get the current wallet balance.</summary>
    public async Task<ApiResponse<WalletBalance>?> GetBalanceAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<WalletBalance>>(_client.WsPath("wallet/balance"), ct);
    }

    /// <summary>Add funds to the wallet.</summary>
    public async Task<ApiResponse<object>?> AddFundsAsync(decimal amount, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("wallet/add-funds"), new { amount }, ct);
    }

    /// <summary>List wallet transactions.</summary>
    public async Task<ApiResponse<object>?> ListTransactionsAsync(object? query = null, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath("wallet/transactions"), ct);
    }

    /// <summary>Configure auto-reload settings.</summary>
    public async Task<ApiResponse<object>?> ConfigureAutoReloadAsync(object config, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath("wallet/auto-reload"), config, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Wallet balance of the configured workspace. Funding and auto-reload are managed in the FlexOps portal.
/// </summary>
public sealed class WalletResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="WalletResource"/>.</summary>
    public WalletResource(FlexOpsClient client) => _client = client;

    /// <summary>
    /// Get the wallet balance of the configured workspace (<c>GET api/wallet/my-balance</c> with
    /// <c>X-Workspace-Id</c>). The Gateway answers with the bare balance, not an <see cref="ApiResponse{T}"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException"><see cref="FlexOpsClient.WorkspaceId"/> is not set.</exception>
    public async Task<WalletBalance?> GetBalanceAsync(CancellationToken ct = default)
    {
        var headers = new Dictionary<string, string> { ["X-Workspace-Id"] = _client.RequireWorkspaceId() };
        return await _client.GetAsync<WalletBalance>("api/wallet/my-balance", headers, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// API key management (create, revoke, rotate).
/// </summary>
public sealed class ApiKeysResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ApiKeysResource"/>.</summary>
    public ApiKeysResource(FlexOpsClient client) => _client = client;

    /// <summary>List all API keys for the workspace.</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("api-keys"), ct);
    }

    /// <summary>Create a new API key. The full key is only returned once.</summary>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("api-keys"), request, ct);
    }

    /// <summary>Revoke an API key.</summary>
    public async Task<ApiResponse<object>?> RevokeAsync(string keyId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"api-keys/{keyId}"), ct);
    }

    /// <summary>Rotate an API key (revoke + create new).</summary>
    public async Task<ApiResponse<object>?> RotateAsync(string keyId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"api-keys/{keyId}/rotate"), null, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// USPS scan form (SCAN/manifest) management.
/// </summary>
public sealed class ScanFormsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ScanFormsResource"/>.</summary>
    public ScanFormsResource(FlexOpsClient client) => _client = client;

    /// <summary>Create a USPS scan form (SCAN/manifest).</summary>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("scan-forms"), request, ct);
    }

    /// <summary>List scan forms.</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("scan-forms"), ct);
    }

    /// <summary>Get a scan form by ID.</summary>
    public async Task<ApiResponse<object>?> GetAsync(string scanFormId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"scan-forms/{scanFormId}"), ct);
    }
}

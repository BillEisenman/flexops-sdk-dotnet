// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Shipping automation rules management. Max 100 rules per workspace.
/// </summary>
public sealed class RulesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="RulesResource"/>.</summary>
    public RulesResource(FlexOpsClient client) => _client = client;

    /// <summary>List all shipping automation rules.</summary>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("shipping-rules"), ct);
    }

    /// <summary>Get a rule by ID.</summary>
    public async Task<ApiResponse<object>?> GetAsync(string ruleId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"shipping-rules/{ruleId}"), ct);
    }

    /// <summary>Create a shipping rule.</summary>
    public async Task<ApiResponse<object>?> CreateAsync(object rule, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping-rules"), rule, ct);
    }

    /// <summary>Update a shipping rule.</summary>
    public async Task<ApiResponse<object>?> UpdateAsync(string ruleId, object rule, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath($"shipping-rules/{ruleId}"), rule, ct);
    }

    /// <summary>Delete a shipping rule.</summary>
    public async Task<ApiResponse<object>?> DeleteAsync(string ruleId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"shipping-rules/{ruleId}"), ct);
    }

    /// <summary>Reorder rules (set priority).</summary>
    public async Task<ApiResponse<object>?> ReorderAsync(string[] ruleIds, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath("shipping-rules/reorder"), new { ruleIds }, ct);
    }
}

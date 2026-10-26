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

    /// <summary>Create a shipping rule (201 Created). Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.</summary>
    public async Task<ApiResponse<object>?> CreateAsync(CreateShippingRuleRequest rule, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping-rules"), rule, ct);
    }

    /// <summary>
    /// Update a shipping rule. Only the properties set on <paramref name="rule"/> change. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.
    /// </summary>
    public async Task<ApiResponse<object>?> UpdateAsync(string ruleId, UpdateShippingRuleRequest rule, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(rule);
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath($"shipping-rules/{ruleId}"), rule, ct);
    }

    /// <summary>
    /// Delete a shipping rule. Returns <c>null</c> (204 No Content) on success. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.
    /// </summary>
    public async Task<ApiResponse<object>?> DeleteAsync(string ruleId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"shipping-rules/{ruleId}"), ct);
    }

    /// <summary>Set the priority of several rules at once. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.</summary>
    /// <param name="rules">Each rule and its new priority.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> ReorderAsync(IEnumerable<RulePriority> rules, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(rules);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping-rules/reorder"), new { rules = rules.ToList() }, ct);
    }

    /// <summary>
    /// Dry-run the workspace's rules against a hypothetical shipment; nothing is changed. API keys need
    /// <c>shipping:read</c>; the public demo keys are refused.
    /// </summary>
    /// <returns>Which rules matched, condition by condition, and the actions the winning rule would apply.</returns>
    public async Task<ApiResponse<object>?> TestAsync(TestShippingRuleRequest shipment, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(shipment);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipping-rules/test"), shipment, ct);
    }
}

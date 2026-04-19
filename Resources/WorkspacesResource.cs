// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Workspace management operations (CRUD, members, invitations).
/// </summary>
public sealed class WorkspacesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="WorkspacesResource"/>.</summary>
    public WorkspacesResource(FlexOpsClient client) => _client = client;

    /// <summary>List all workspaces the current user belongs to.</summary>
    public async Task<ApiResponse<Workspace[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<Workspace[]>>("api/workspaces", ct);
    }

    /// <summary>Get details for a specific workspace.</summary>
    public async Task<ApiResponse<Workspace>?> GetAsync(string? workspaceId = null, CancellationToken ct = default)
    {
        var id = workspaceId ?? _client.WorkspaceId;
        return await _client.GetAsync<ApiResponse<Workspace>>($"api/workspaces/{id}", ct);
    }

    /// <summary>Create a new workspace.</summary>
    public async Task<ApiResponse<Workspace>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<Workspace>>("api/workspaces", request, ct);
    }

    /// <summary>Update workspace settings.</summary>
    public async Task<ApiResponse<Workspace>?> UpdateAsync(object data, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<Workspace>>(_client.WsPath(""), ct);
    }

    /// <summary>List members of the current workspace.</summary>
    public async Task<ApiResponse<object[]>?> ListMembersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("members"), ct);
    }

    /// <summary>Invite a user to the workspace.</summary>
    public async Task<ApiResponse<object>?> InviteMemberAsync(string email, string role = "Member", CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            _client.WsPath("members/invite"),
            new { email, role },
            ct);
    }

    /// <summary>Remove a member from the workspace.</summary>
    public async Task<ApiResponse<object>?> RemoveMemberAsync(string userId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"members/{userId}"), ct);
    }

    /// <summary>Update a member's role.</summary>
    public async Task<ApiResponse<object>?> UpdateMemberRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(
            _client.WsPath($"members/{userId}/role"),
            new { role },
            ct);
    }
}

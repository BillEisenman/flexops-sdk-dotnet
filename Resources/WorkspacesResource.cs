// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Workspaces the caller belongs to, and their members and invitations.
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

    /// <summary>Get details for a workspace; the configured workspace when <paramref name="workspaceId"/> is <c>null</c>.</summary>
    public async Task<ApiResponse<WorkspaceDetail>?> GetAsync(string? workspaceId = null, CancellationToken ct = default)
    {
        var id = workspaceId ?? _client.RequireWorkspaceId();
        return await _client.GetAsync<ApiResponse<WorkspaceDetail>>($"api/workspaces/{id}", ct);
    }

    /// <summary>List the members and pending invitations of the current workspace.</summary>
    public async Task<ApiResponse<WorkspaceMemberList>?> ListMembersAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<WorkspaceMemberList>>(_client.WsPath("members"), ct);
    }

    /// <summary>Invite a user to the workspace. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.</summary>
    /// <param name="email">The invitee's email address.</param>
    /// <param name="role">The role granted on acceptance; sent as the Gateway <c>WorkspaceRole</c> number.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> InviteMemberAsync(string email, WorkspaceRole role = WorkspaceRole.Member, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            _client.WsPath("members/invite"),
            new { email, role = (int)role },
            ct);
    }

    /// <summary>
    /// Remove a member from the workspace. Returns <c>null</c> (204 No Content) on success. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.
    /// </summary>
    public async Task<ApiResponse<object>?> RemoveMemberAsync(string userId, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"members/{userId}"), ct);
    }

    /// <summary>Change a member's role. Signed-in users only: API keys get 403 <c>SIGNED_IN_USER_REQUIRED</c>, whatever their scopes.</summary>
    /// <param name="userId">The member's user ID (<see cref="WorkspaceMember.UserId"/>).</param>
    /// <param name="newRole">The new role; sent as the Gateway <c>WorkspaceRole</c> number.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> UpdateMemberRoleAsync(string userId, WorkspaceRole newRole, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(
            _client.WsPath($"members/{userId}/role"),
            new { newRole = (int)newRole },
            ct);
    }
}

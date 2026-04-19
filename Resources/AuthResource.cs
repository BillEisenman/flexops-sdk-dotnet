// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Authentication and account management operations.
/// </summary>
public sealed class AuthResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="AuthResource"/>.</summary>
    public AuthResource(FlexOpsClient client) => _client = client;

    /// <summary>Authenticate with email and password. Returns JWT tokens.</summary>
    public async Task<ApiResponse<object>?> LoginAsync(object request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<ApiResponse<object>>("api/Account/login", request, ct);
        return result;
    }

    /// <summary>Register a new account.</summary>
    public async Task<ApiResponse<object>?> RegisterAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>("api/Account/register", request, ct);
    }

    /// <summary>Refresh an expired access token using a refresh token.</summary>
    public async Task<ApiResponse<object>?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // The refresh token is sent via X-Current-Session-Token header;
        // PostAsync doesn't support custom headers, so we pass it in the body
        // and rely on the server accepting it. For header-based refresh, use
        // the underlying FlexOpsClient directly.
        return await _client.PostAsync<ApiResponse<object>>(
            "api/Account/refresh-token",
            new { refreshToken },
            ct);
    }

    /// <summary>Log out and invalidate the current session.</summary>
    public async Task<ApiResponse<object>?> LogoutAsync(CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>("api/Account/logout", null, ct);
    }

    /// <summary>Get the current user's profile.</summary>
    public async Task<ApiResponse<object>?> GetProfileAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>("api/Account/profile", ct);
    }

    /// <summary>Update the current user's profile.</summary>
    public async Task<ApiResponse<object>?> UpdateProfileAsync(object data, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>("api/Account/profile", data, ct);
    }

    /// <summary>Change the current user's password.</summary>
    public async Task<ApiResponse<object>?> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            "api/Account/change-password",
            new { currentPassword, newPassword },
            ct);
    }

    /// <summary>Request a password reset email.</summary>
    public async Task<ApiResponse<object>?> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            "api/Account/forgot-password",
            new { email },
            ct);
    }

    /// <summary>Reset password using a reset token.</summary>
    public async Task<ApiResponse<object>?> ResetPasswordAsync(string token, string newPassword, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            "api/Account/reset-password",
            new { token, newPassword },
            ct);
    }

    /// <summary>Verify email with the verification token.</summary>
    public async Task<ApiResponse<object>?> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(
            "api/Account/verify-email",
            new { token },
            ct);
    }
}

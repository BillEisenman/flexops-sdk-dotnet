// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Token acquisition for operations that need a signed-in user (login, refresh, logout).
/// </summary>
public sealed class AuthResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="AuthResource"/>.</summary>
    public AuthResource(FlexOpsClient client) => _client = client;

    /// <summary>
    /// Authenticate with email and password (<c>{ email, password, rememberMe? }</c>). On success the client sends
    /// the returned <see cref="LoginResponse.Token"/> as its Bearer token.
    /// </summary>
    /// <remarks>When <see cref="LoginResponse.RequiresTwoFactor"/> is set, no access token is issued yet.</remarks>
    public async Task<ApiResponse<LoginResponse>?> LoginAsync(object request, CancellationToken ct = default)
    {
        var result = await _client.PostAsync<ApiResponse<LoginResponse>>("api/Account/login", request, ct);
        UseToken(result);
        return result;
    }

    /// <summary>
    /// Exchange a refresh token for a new token pair (<c>{ token, refreshToken }</c>; the Gateway requires both).
    /// On success the client sends the new <see cref="LoginResponse.Token"/> as its Bearer token.
    /// </summary>
    /// <param name="refreshToken">The refresh token issued with the current access token.</param>
    /// <param name="accessToken">The current (possibly expired) access token. Defaults to the client's Bearer token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="InvalidOperationException">No access token was passed and the client has none. Nothing is sent.</exception>
    public async Task<ApiResponse<LoginResponse>?> RefreshTokenAsync(string refreshToken, string? accessToken = null, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        var token = accessToken ?? _client.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException(
                "Refreshing needs the current access token: pass accessToken, or log in or call SetAccessToken first.");
        }

        var result = await _client.PostAsync<ApiResponse<LoginResponse>>(
            "api/Account/refresh-token",
            new { token, refreshToken },
            ct);
        UseToken(result);
        return result;
    }

    /// <summary>Log out and invalidate the current session.</summary>
    public async Task<ApiResponse<object>?> LogoutAsync(CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>("api/Account/logout", null, ct);
    }

    private void UseToken(ApiResponse<LoginResponse>? result)
    {
        if (result?.Data?.Token is { Length: > 0 } token)
        {
            _client.SetAccessToken(token);
        }
    }
}

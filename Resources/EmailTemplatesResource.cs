// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-31
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Shipment email template management: CRUD operations and rendered preview.
/// </summary>
public sealed class EmailTemplatesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="EmailTemplatesResource"/>.</summary>
    public EmailTemplatesResource(FlexOpsClient client) => _client = client;

    // -----------------------------------------------------------------------
    // CRUD
    // -----------------------------------------------------------------------

    /// <summary>List all shipment email templates for the workspace.</summary>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object[]>>(_client.WsPath("shipment-email-templates"), ct);
    }

    /// <summary>Get a shipment email template by ID.</summary>
    /// <param name="id">The email template ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetAsync(string id, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"shipment-email-templates/{id}"), ct);
    }

    /// <summary>Create a new shipment email template.</summary>
    /// <param name="request">The template definition.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> CreateAsync(object request, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("shipment-email-templates"), request, ct);
    }

    /// <summary>Update an existing shipment email template.</summary>
    /// <param name="id">The email template ID.</param>
    /// <param name="request">The updated template definition.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> UpdateAsync(string id, object request, CancellationToken ct = default)
    {
        return await _client.PutAsync<ApiResponse<object>>(_client.WsPath($"shipment-email-templates/{id}"), request, ct);
    }

    /// <summary>Delete a shipment email template.</summary>
    /// <param name="id">The email template ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> DeleteAsync(string id, CancellationToken ct = default)
    {
        return await _client.DeleteAsync<ApiResponse<object>>(_client.WsPath($"shipment-email-templates/{id}"), ct);
    }

    // -----------------------------------------------------------------------
    // Actions
    // -----------------------------------------------------------------------

    /// <summary>Render a preview of the email template with optional merge context.</summary>
    /// <param name="id">The email template ID.</param>
    /// <param name="context">Optional merge context (e.g., sample shipment data) used to render the preview.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> PreviewAsync(string id, object? context = null, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"shipment-email-templates/{id}/preview"), context, ct);
    }
}

// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-10-19
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Shipment notification email templates, one per tracking event (<c>/api/workspaces/{workspaceId}/email-templates</c>).
/// <see cref="UpsertAsync"/> creates or replaces the template for an event. Responses are the bare JSON, not an
/// <see cref="ApiResponse{T}"/> envelope.
/// </summary>
/// <remarks>
/// Reads work with any API key or a user token. Writes (upsert, delete and preview) need a user token
/// (<see cref="AuthResource.LoginAsync"/>) or an API key granted the <c>email-templates:write</c> scope; an API key
/// without it gets 403 <c>API_KEY_SCOPE_REQUIRED</c> with <c>requiredScopes</c>.
/// </remarks>
public sealed class EmailTemplatesResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="EmailTemplatesResource"/>.</summary>
    public EmailTemplatesResource(FlexOpsClient client) => _client = client;

    /// <summary>List the workspace's email templates.</summary>
    public async Task<List<EmailTemplate>?> ListAsync(CancellationToken ct = default)
    {
        return await _client.GetAsync<List<EmailTemplate>>(_client.WsPath("email-templates"), ct);
    }

    /// <summary>Get an email template.</summary>
    public async Task<EmailTemplate?> GetAsync(string templateId, CancellationToken ct = default)
    {
        return await _client.GetAsync<EmailTemplate>(_client.WsPath($"email-templates/{templateId}"), ct);
    }

    /// <summary>Create or replace the template for <see cref="UpsertEmailTemplateRequest.EventType"/>.</summary>
    public async Task<EmailTemplate?> UpsertAsync(UpsertEmailTemplateRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PutAsync<EmailTemplate>(_client.WsPath("email-templates"), request, ct);
    }

    /// <summary>Delete an email template (204 No Content).</summary>
    public async Task DeleteAsync(string templateId, CancellationToken ct = default)
    {
        await _client.DeleteAsync<object>(_client.WsPath($"email-templates/{templateId}"), ct);
    }

    /// <summary>Render a subject and body template with sample data. Nothing is saved or sent.</summary>
    public async Task<EmailTemplatePreview?> PreviewAsync(EmailTemplatePreviewRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<EmailTemplatePreview>(_client.WsPath("email-templates/preview"), request, ct);
    }
}

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

    /// <summary>
    /// Create a USPS scan form (SCAN/manifest). API keys need <c>shipping:write</c>; the public demo keys are refused.
    /// </summary>
    /// <remarks>
    /// Sends <see cref="CreateScanFormRequest.RequestId"/>, or a new ID generated for this call. A retrying handler
    /// in the <see cref="HttpClient"/> pipeline resends the same body, so every attempt carries the same ID and the
    /// Gateway returns the first attempt's form instead of creating a second one.
    /// </remarks>
    /// <param name="request">The labels and the mailing date.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> CreateAsync(CreateScanFormRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = request.RequestId is null ? request with { RequestId = Guid.NewGuid() } : request;
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("scan-forms"), body, ct);
    }

    /// <summary>
    /// Create a USPS scan form from an untyped body, sent as is. API keys need <c>shipping:write</c>; the public demo
    /// keys are refused. Prefer the <see cref="CreateScanFormRequest"/> overload, which sends a <c>requestId</c> so a
    /// retry cannot create a second form.
    /// </summary>
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

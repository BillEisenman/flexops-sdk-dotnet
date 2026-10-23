// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Globalization;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Return authorization (RMA) management.
/// </summary>
public sealed class ReturnsResource
{
    private readonly FlexOpsClient _client;

    /// <summary>Initializes a new instance of <see cref="ReturnsResource"/>.</summary>
    public ReturnsResource(FlexOpsClient client) => _client = client;

    /// <summary>List received returns in a date range, one page at a time. To list return authorizations, use <see cref="ListRmasAsync"/>.</summary>
    /// <param name="startDate">Earliest creation date; the Gateway default applies when <c>null</c>.</param>
    /// <param name="endDate">Latest creation date; the Gateway default applies when <c>null</c>.</param>
    /// <param name="page">Page number, from 1.</param>
    /// <param name="pageSize">Items per page.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 25,
        CancellationToken ct = default)
    {
        var path = FlexOpsClient.WithQuery(_client.WsPath("returns"),
        [
            new("startDate", startDate?.ToString("O", CultureInfo.InvariantCulture)),
            new("endDate", endDate?.ToString("O", CultureInfo.InvariantCulture)),
            new("page", page.ToString(CultureInfo.InvariantCulture)),
            new("pageSize", pageSize.ToString(CultureInfo.InvariantCulture)),
        ]);
        return await _client.GetAsync<ApiResponse<object[]>>(path, ct);
    }

    /// <summary>Get a received return by its numeric return ID.</summary>
    /// <param name="returnId">The numeric return ID (<c>returnId</c> on a listed return or an RMA).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>An RMA number such as <c>RMA-1001</c> is not a return ID and matches no Gateway route; use <see cref="GetRmaAsync"/>.</remarks>
    public async Task<ApiResponse<object>?> GetAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}"), ct);
    }

    /// <summary>Get a received return by its numeric return ID.</summary>
    /// <param name="returnId">The numeric return ID.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task<ApiResponse<object>?> GetAsync(long returnId, CancellationToken ct = default)
        => GetAsync(returnId.ToString(CultureInfo.InvariantCulture), ct);

    /// <summary>List return authorizations (RMAs), one page at a time.</summary>
    /// <param name="page">Page number, from 1.</param>
    /// <param name="pageSize">Items per page (1 to 100).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object[]>?> ListRmasAsync(int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        var path = FlexOpsClient.WithQuery(_client.WsPath("returns/rma"),
        [
            new("page", page.ToString(CultureInfo.InvariantCulture)),
            new("pageSize", pageSize.ToString(CultureInfo.InvariantCulture)),
        ]);
        return await _client.GetAsync<ApiResponse<object[]>>(path, ct);
    }

    /// <summary>Get a return authorization by its RMA number, for example <c>RMA-1001</c>.</summary>
    /// <param name="rmaNumber">The RMA number.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GetRmaAsync(string rmaNumber, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rmaNumber);
        return await _client.GetAsync<ApiResponse<object>>(_client.WsPath($"returns/rma/{Uri.EscapeDataString(rmaNumber)}"), ct);
    }

    /// <summary>Create a return authorization (RMA). The Gateway answers 201 with the RMA in the envelope.</summary>
    public async Task<ApiResponse<object>?> CreateAsync(CreateRmaRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath("returns/rma"), request, ct);
    }

    /// <summary>Generate a return label for an approved RMA.</summary>
    /// <remarks>Sends an empty JSON object: the Gateway binds a required <c>GenerateReturnLabelRequest</c>
    /// body and refuses a <c>null</c> body with 400. An empty object carries no package, so this only works
    /// for a workspace with a default shipping preset; otherwise the Gateway answers 400
    /// <c>PackageMeasurementsRequired</c>. Prefer <see cref="GenerateLabelAsync(string, ReturnLabelRequest, CancellationToken)"/>
    /// and send the weight and size.</remarks>
    public async Task<ApiResponse<object>?> GenerateLabelAsync(string returnId, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/label"), new { }, ct);
    }

    /// <summary>Generate a return label for an approved RMA with a declared HAZMAT class.</summary>
    /// <param name="returnId">The RMA number.</param>
    /// <param name="hazmatClassification">What the parcel holds; <see cref="HazmatClassification.None"/> for no dangerous goods.
    /// Return labels cannot carry HAZMAT yet, so any other class is refused (400 <c>HazmatNotSupported</c>).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GenerateLabelAsync(string returnId, HazmatClassification hazmatClassification, CancellationToken ct = default)
    {
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/label"), new { hazmatClassification }, ct);
    }

    /// <summary>Generate a return label for an approved RMA with the package weight and size.</summary>
    /// <param name="returnId">The RMA number.</param>
    /// <param name="request">The package (<see cref="ReturnLabelRequest.WeightOz"/>, <see cref="ReturnLabelRequest.LengthIn"/>,
    /// <see cref="ReturnLabelRequest.WidthIn"/>, <see cref="ReturnLabelRequest.HeightIn"/>) and options.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> GenerateLabelAsync(string returnId, ReturnLabelRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{returnId}/label"), request, ct);
    }

    /// <summary>Receive an RMA's items and record what happens to each.</summary>
    /// <param name="rmaId">The numeric RMA ID.</param>
    /// <param name="request">The RMA number, receiving warehouse and inspected items.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task<ApiResponse<object>?> MarkReceivedAsync(long rmaId, ReceiveRmaRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _client.PostAsync<ApiResponse<object>>(_client.WsPath($"returns/{rmaId}/receive"), request, ct);
    }
}

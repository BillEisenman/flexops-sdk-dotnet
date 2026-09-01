// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-04
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

namespace FlexOps.Sdk;

/// <summary>Standard API response wrapper.</summary>
public sealed class ApiResponse<T>
{
    /// <summary>Whether the request succeeded.</summary>
    public bool Success { get; set; }

    /// <summary>The response data.</summary>
    public T? Data { get; set; }

    /// <summary>Optional message (usually on errors).</summary>
    public string? Message { get; set; }

    /// <summary>Optional validation errors.</summary>
    public List<string>? Errors { get; set; }
}

/// <summary>Paginated response wrapper.</summary>
public sealed class PaginatedResponse<T>
{
    /// <summary>The items on this page.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Total items across all pages.</summary>
    public int TotalCount { get; set; }

    /// <summary>Current page number.</summary>
    public int Page { get; set; }

    /// <summary>Items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages { get; set; }
}

/// <summary>Canonical Gateway rate-shopping request.</summary>
public sealed class RateShoppingRequest
{
    /// <summary>Ship-from address.</summary>
    public required ShippingAddress Origin { get; init; }

    /// <summary>Ship-to address.</summary>
    public required ShippingAddress Destination { get; init; }

    /// <summary>Parcel dimensions and weight.</summary>
    public required ShippingPackage Package { get; init; }

    /// <summary>Optional carrier-code filter.</summary>
    public List<string>? Carriers { get; init; }
}

/// <summary>Address used by the normalized Gateway shipping API.</summary>
public sealed class ShippingAddress
{
    /// <summary>Optional recipient or company name.</summary>
    public string? Name { get; init; }

    /// <summary>Primary street line.</summary>
    public required string AddressLine1 { get; init; }

    /// <summary>City or locality.</summary>
    public required string City { get; init; }

    /// <summary>State or province code.</summary>
    public required string StateProvince { get; init; }

    /// <summary>Postal code.</summary>
    public required string PostalCode { get; init; }

    /// <summary>ISO country code.</summary>
    public string CountryCode { get; init; } = "US";
}

/// <summary>Package used by the normalized Gateway shipping API.</summary>
public sealed class ShippingPackage
{
    /// <summary>Package weight.</summary>
    public decimal Weight { get; init; }

    /// <summary>Weight unit.</summary>
    public string WeightUnit { get; init; } = "oz";

    /// <summary>Package length.</summary>
    public decimal? Length { get; init; }

    /// <summary>Package width.</summary>
    public decimal? Width { get; init; }

    /// <summary>Package height.</summary>
    public decimal? Height { get; init; }

    /// <summary>Dimension unit.</summary>
    public string DimensionUnit { get; init; } = "in";

    /// <summary>Optional carrier-defined package name.</summary>
    public string? PredefinedPackage { get; init; }
}

/// <summary>Shipping rate from a carrier. Matches the Gateway <c>ShippingRate</c> wire shape.</summary>
public sealed class ShippingRate
{
    /// <summary>Carrier code (USPS, UPS, FEDEX, DHL).</summary>
    public string CarrierCode { get; set; } = string.Empty;

    /// <summary>Human-readable carrier name.</summary>
    public string CarrierName { get; set; } = string.Empty;

    /// <summary>Service code.</summary>
    public string ServiceCode { get; set; } = string.Empty;

    /// <summary>Human-readable service name.</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>Rate in the given currency.</summary>
    public decimal Rate { get; set; }

    /// <summary>Currency code (e.g., USD).</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Estimated transit days, when known.</summary>
    public int? EstimatedDays { get; set; }

    /// <summary>Estimated delivery date (ISO 8601), when known.</summary>
    public string? EstimatedDeliveryDate { get; set; }

    /// <summary>Whether tracking is included in the service.</summary>
    public bool TrackingIncluded { get; set; }
}

/// <summary>Response from a rate-shopping request. Matches the Gateway <c>RateShoppingResponse</c>.</summary>
public sealed class RateShoppingResponse
{
    /// <summary>The rates returned across all requested carriers.</summary>
    public List<ShippingRate> Rates { get; set; } = [];

    /// <summary>Currency the rates are expressed in.</summary>
    public string Currency { get; set; } = "USD";
}

/// <summary>Shipping label. Matches the Gateway <c>ShippingLabel</c> wire shape.</summary>
public sealed class Label
{
    /// <summary>Unique label identifier.</summary>
    public string LabelId { get; set; } = string.Empty;

    /// <summary>Carrier code the label was created for.</summary>
    public string CarrierCode { get; set; } = string.Empty;

    /// <summary>Carrier tracking number.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>Label data (base64 or URL).</summary>
    public string LabelData { get; set; } = string.Empty;

    /// <summary>Label format (PDF, PNG, ZPL).</summary>
    public string LabelFormat { get; set; } = "PDF";

    /// <summary>Rate charged for the label.</summary>
    public decimal Rate { get; set; }

    /// <summary>Currency the rate is expressed in.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>Return-label data, when a return label was also generated.</summary>
    public string? ReturnLabelData { get; set; }

    /// <summary>Originating order id when the label was purchased against an order.</summary>
    public long? OrderId { get; set; }

    /// <summary>Whether the label was generated in sandbox mode (no real postage).</summary>
    public bool IsSandbox { get; set; }

    /// <summary>Creation timestamp (ISO 8601).</summary>
    public string CreatedAt { get; set; } = string.Empty;
}

/// <summary>Tracking event.</summary>
public sealed class TrackingEvent
{
    /// <summary>Event timestamp.</summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>Event status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Event description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Location of the event.</summary>
    public string? Location { get; set; }
}

/// <summary>Shipment tracking information.</summary>
public sealed class TrackingInfo
{
    /// <summary>Tracking number.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>Carrier name.</summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>Current status.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Status detail.</summary>
    public string StatusDetail { get; set; } = string.Empty;

    /// <summary>Estimated delivery date.</summary>
    public string? EstimatedDelivery { get; set; }

    /// <summary>Tracking events.</summary>
    public List<TrackingEvent> Events { get; set; } = [];
}

/// <summary>Workspace details.</summary>
public sealed class Workspace
{
    /// <summary>Workspace ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Plan ID.</summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>Whether the workspace is active.</summary>
    public bool IsActive { get; set; }
}

/// <summary>Wallet balance.</summary>
public sealed class WalletBalance
{
    /// <summary>Current balance.</summary>
    public decimal Balance { get; set; }

    /// <summary>Currency code.</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Whether auto-reload is enabled.</summary>
    public bool AutoReloadEnabled { get; set; }
}

/// <summary>Webhook subscription.</summary>
public sealed class WebhookSubscription
{
    /// <summary>Subscription ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Delivery URL.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Subscribed event types.</summary>
    public List<string> Events { get; set; } = [];

    /// <summary>Whether the subscription is active.</summary>
    public bool IsActive { get; set; }
}

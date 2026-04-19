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

/// <summary>Shipping rate from a carrier.</summary>
public sealed class ShippingRate
{
    /// <summary>Carrier name (USPS, UPS, FedEx, DHL).</summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>Service name.</summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>Rate in the given currency.</summary>
    public decimal Rate { get; set; }

    /// <summary>Currency code (e.g., USD).</summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Estimated transit days.</summary>
    public int EstimatedDays { get; set; }

    /// <summary>Estimated delivery date.</summary>
    public string? DeliveryDate { get; set; }
}

/// <summary>Shipping label.</summary>
public sealed class Label
{
    /// <summary>Unique label identifier.</summary>
    public string LabelId { get; set; } = string.Empty;

    /// <summary>Carrier tracking number.</summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>Carrier name.</summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>Service name.</summary>
    public string Service { get; set; } = string.Empty;

    /// <summary>Label data (base64 or URL).</summary>
    public string LabelData { get; set; } = string.Empty;

    /// <summary>Label format (PDF, PNG, ZPL).</summary>
    public string LabelFormat { get; set; } = string.Empty;

    /// <summary>Rate charged.</summary>
    public decimal Rate { get; set; }

    /// <summary>Creation timestamp.</summary>
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

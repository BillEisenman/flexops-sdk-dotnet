// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-04
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using System.Text.Json.Serialization;

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
    /// <summary>Optional service selection.</summary>
    public List<string>? ServiceLevels { get; init; }
    /// <summary>Explicit international mailing date.</summary>
    public DateTime? ShipDate { get; init; }
    /// <summary>Total declared USD value.</summary>
    public decimal? DeclaredValue { get; init; }
    /// <summary>Pricing currency.</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>
    /// Declared USPS HAZMAT class, so quotes match what the label route will accept.
    /// Sent only when set; the SDK never fills it in.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HazmatClassification? HazmatClassification { get; init; }
}

/// <summary>
/// USPS HAZMAT classification, sent as the Gateway <c>HazmatClassification</c> number.
/// There is no default: leave the property <c>null</c> when you have no answer, or send
/// <see cref="None"/> (0) to declare that the parcel holds no dangerous goods.
/// </summary>
public enum HazmatClassification
{
    /// <summary>Not hazardous.</summary>
    None = 0,

    /// <summary>Limited Quantity (consumer-commodity small quantities). USPS Ground Advantage only.</summary>
    LimitedQuantity = 1,

    /// <summary>Lithium batteries contained in equipment.</summary>
    LithiumBatteryContainedInEquipment = 2,

    /// <summary>Lithium batteries packed with equipment.</summary>
    LithiumBatteryPackedWithEquipment = 3,

    /// <summary>Stand-alone lithium batteries.</summary>
    LithiumBatteryStandalone = 4,

    /// <summary>Other restricted material.</summary>
    OtherRestricted = 5,

    /// <summary>Lithium batteries in a used, damaged or defective device.</summary>
    LithiumBatteryUsedDevice = 6
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

/// <summary>
/// Body of <c>POST /api/workspaces/{workspaceId}/returns/{rmaNumber}/label</c>.
/// </summary>
/// <remarks>
/// The carrier needs the package weight and all three dimensions. Set <see cref="WeightOz"/>,
/// <see cref="LengthIn"/>, <see cref="WidthIn"/> and <see cref="HeightIn"/>; any you leave out come
/// from the workspace's default shipping preset. With no preset the Gateway refuses the call with
/// a 400 (code <c>PackageMeasurementsRequired</c>) that names the missing fields.
/// </remarks>
public sealed class ReturnLabelRequest
{
    /// <summary>Carrier for the return label. Defaults to <c>usps</c> on the Gateway (the only carrier supported).</summary>
    public string? CarrierCode { get; init; }

    /// <summary>Carrier service for the return label.</summary>
    public string? ServiceType { get; init; }

    /// <summary>Package weight in ounces.</summary>
    public decimal? WeightOz { get; init; }

    /// <summary>Package length in inches.</summary>
    public decimal? LengthIn { get; init; }

    /// <summary>Package width in inches.</summary>
    public decimal? WidthIn { get; init; }

    /// <summary>Package height in inches.</summary>
    public decimal? HeightIn { get; init; }

    /// <summary>Set when the workspace return-cost policy is CustomerPays and the customer has paid.</summary>
    public bool? PaymentConfirmed { get; init; }

    /// <summary>
    /// Declared HAZMAT class. Leave <c>null</c> for no answer, or <see cref="HazmatClassification.None"/>.
    /// Return labels cannot carry HAZMAT yet, so any other class is refused (400 <c>HazmatNotSupported</c>).
    /// </summary>
    public HazmatClassification? HazmatClassification { get; init; }
}

/// <summary>Shipping rate from a carrier. Matches the Gateway <c>ShippingRate</c> wire shape.</summary>
public sealed class ShippingRate
{
    /// <summary>Exact USPS parcel product.</summary>
    public string? RateIndicator { get; set; }
    /// <summary>Mail class priced by USPS.</summary>
    public string? UspsMailClass { get; set; }
    /// <summary>Whether the rate is international.</summary>
    public bool IsInternational { get; set; }
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

/// <summary>
/// Events FlexOps sends in the <c>X-FlexOps-Event</c> header and matches subscriptions against.
/// </summary>
/// <remarks>
/// The wire name is the member name (<c>ToString()</c>), in PascalCase, and it is matched exactly: a dotted
/// name such as <c>label.created</c> matches nothing. <see cref="TrackingUpdated"/> and
/// <see cref="TrackingDelivered"/> are sent only to workspaces in the USPS push-tracking pilot.
/// </remarks>
public enum WebhookEventType
{
    /// <summary>A live label was purchased. Sandbox labels send nothing.</summary>
    LabelCreated,

    /// <summary>A live label was voided.</summary>
    LabelCanceled,

    /// <summary>A batch label job finished.</summary>
    BatchCompleted,

    /// <summary>A USPS tracking scan arrived (USPS push-tracking pilot workspaces only).</summary>
    TrackingUpdated,

    /// <summary>A USPS package was delivered (USPS push-tracking pilot workspaces only).</summary>
    TrackingDelivered,
}

/// <summary>A webhook subscription as the Gateway returns it.</summary>
public sealed class WebhookSubscription
{
    /// <summary>Subscription ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Delivery URL.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Optional description.</summary>
    public string? Description { get; set; }

    /// <summary>Subscribed event names, comma-separated (for example <c>LabelCreated,LabelCanceled</c>), or <c>*</c> for every event.</summary>
    public string EventTypes { get; set; } = string.Empty;

    /// <summary><c>Active</c>, <c>Paused</c> or <c>Disabled</c>.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Consecutive failed deliveries. The Gateway pauses the subscription after too many.</summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>When the subscription was created (UTC).</summary>
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>When the subscription was last updated (UTC), if ever.</summary>
    public string? UpdatedAt { get; set; }

    /// <summary>The <c>whsec_...</c> signing secret. Returned only when the subscription is created; store it then.</summary>
    public string? Secret { get; set; }
}

/// <summary>Request to create a webhook subscription.</summary>
public sealed class CreateWebhookRequest
{
    /// <summary>The HTTPS URL that receives deliveries.</summary>
    public required string Url { get; init; }

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Events to receive. Leave it <c>null</c> to receive every event. Sent as the Gateway's comma-separated
    /// <c>eventTypes</c>; an empty list is refused.
    /// </summary>
    public IReadOnlyCollection<WebhookEventType>? Events { get; init; }
}

/// <summary>Request to update a webhook subscription. Only the properties you set are changed.</summary>
public sealed class UpdateWebhookRequest
{
    /// <summary>New HTTPS delivery URL.</summary>
    public string? Url { get; init; }

    /// <summary>New description.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Replaces the subscribed events. Sent as the Gateway's comma-separated <c>eventTypes</c>; an empty
    /// list is refused.
    /// </summary>
    public IReadOnlyCollection<WebhookEventType>? Events { get; init; }

    /// <summary>New status: <c>Active</c>, <c>Paused</c> or <c>Disabled</c>. <c>Active</c> resets the failure count.</summary>
    public string? Status { get; init; }
}

/// <summary>Normalized label input for preview and explicit purchase. International access is server gated.</summary>
public sealed class LabelRequest
{
    /// <summary>Origin.</summary>
    public required ShippingAddress Origin { get; init; }

    /// <summary>Destination.</summary>
    public required ShippingAddress Destination { get; init; }

    /// <summary>Package.</summary>
    public required ShippingPackage Package { get; init; }

    /// <summary>CarrierCode.</summary>
    public required string CarrierCode { get; init; }

    /// <summary>ServiceCode.</summary>
    public required string ServiceCode { get; init; }

    /// <summary>OrderId.</summary>
    public long? OrderId { get; init; }

    /// <summary>LabelFormat.</summary>
    public string LabelFormat { get; init; } = "PDF";

    /// <summary>ShipDate.</summary>
    public DateTime? ShipDate { get; init; }

    /// <summary>DeclaredValue.</summary>
    public decimal? DeclaredValue { get; init; }

    /// <summary>MaximumPostageAmount.</summary>
    public decimal? MaximumPostageAmount { get; init; }

    /// <summary>ConfirmationToken.</summary>
    public string? ConfirmationToken { get; init; }

    /// <summary>CustomsDeclaration.</summary>
    public CustomsDeclaration? CustomsDeclaration { get; init; }

    /// <summary>RateIndicator.</summary>
    public string? RateIndicator { get; init; }

    /// <summary>
    /// Declared USPS HAZMAT class. A label without an <see cref="OrderId"/> must declare one:
    /// send <see cref="Sdk.HazmatClassification.None"/> for an ordinary parcel. Sent only when set;
    /// the SDK never fills it in.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public HazmatClassification? HazmatClassification { get; init; }
}
/// <summary>USPS customs declaration with per-unit item quantities and values.</summary>
public sealed class CustomsDeclaration
{
    /// <summary>ContentsType.</summary>
    public string ContentsType { get; init; } = "merchandise";

    /// <summary>ContentsExplanation.</summary>
    public string? ContentsExplanation { get; init; }

    /// <summary>NonDeliveryOption.</summary>
    public string NonDeliveryOption { get; init; } = "return";

    /// <summary>Currency.</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>DeclaredValue.</summary>
    public decimal DeclaredValue { get; init; }

    /// <summary>AesItn.</summary>
    public required string AesItn { get; init; }

    /// <summary>InvoiceNumber.</summary>
    public string? InvoiceNumber { get; init; }

    /// <summary>LicenseNumber.</summary>
    public string? LicenseNumber { get; init; }

    /// <summary>CertificateNumber.</summary>
    public string? CertificateNumber { get; init; }

    /// <summary>RestrictionType.</summary>
    public string? RestrictionType { get; init; }

    /// <summary>RestrictionComments.</summary>
    public string? RestrictionComments { get; init; }

    /// <summary>Items.</summary>
    public List<CustomsItem> Items { get; init; } = [];
}
/// <summary>One customs line. Value and WeightOz are per unit.</summary>
public sealed class CustomsItem
{
    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Quantity.</summary>
    public int Quantity { get; init; }

    /// <summary>Value.</summary>
    public decimal Value { get; init; }

    /// <summary>WeightOz.</summary>
    public decimal WeightOz { get; init; }

    /// <summary>HsCode.</summary>
    public string? HsCode { get; init; }

    /// <summary>OriginCountry.</summary>
    public required string OriginCountry { get; init; }
}

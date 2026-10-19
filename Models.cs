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
    /// <summary>Carrier for the return label. Optional: the Gateway defaults it to <c>usps</c> (the only carrier supported).</summary>
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

    /// <summary>
    /// USPS flat-rate product the customer returns in (for example <c>FS</c> Small Flat Rate Box or <c>FE</c> Flat Rate
    /// Envelope). Leave <c>null</c> for an ordinary return priced on the package. Needs <see cref="ServiceType"/>
    /// <c>PRIORITY</c> or <c>PRIORITY_EXPRESS</c>.
    /// </summary>
    public string? RateIndicator { get; init; }
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

/// <summary>
/// A member's role in a workspace, sent and received as the Gateway <c>WorkspaceRole</c> number.
/// </summary>
public enum WorkspaceRole
{
    /// <summary>Workspace owner.</summary>
    Owner = 0,

    /// <summary>Administrator: manages members and settings.</summary>
    Admin = 1,

    /// <summary>Regular member.</summary>
    Member = 2,

    /// <summary>Merchant: returns-only access, optionally limited to specific stores.</summary>
    Merchant = 3
}

/// <summary>A workspace the caller belongs to, as <c>GET api/workspaces</c> lists it.</summary>
public sealed class Workspace
{
    /// <summary>Workspace ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>The caller's role in the workspace.</summary>
    public WorkspaceRole MyRole { get; set; }

    /// <summary>Plan ID.</summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>Number of members.</summary>
    public int MemberCount { get; set; }

    /// <summary>Whether the caller created the workspace.</summary>
    public bool IsCreator { get; set; }

    /// <summary>Product type of the workspace plan, when known.</summary>
    public string? ProductType { get; set; }
}

/// <summary>Workspace details, as <c>GET api/workspaces/{id}</c> returns them.</summary>
public sealed class WorkspaceDetail
{
    /// <summary>Workspace ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Custom domain, if any.</summary>
    public string? Domain { get; set; }

    /// <summary>Plan ID.</summary>
    public string PlanId { get; set; } = string.Empty;

    /// <summary>Workspace status number: 0 Active, 1 Suspended, 2 Archived, 3 PendingDelete.</summary>
    public int Status { get; set; }

    /// <summary>Maximum number of members.</summary>
    public int UserLimit { get; set; }

    /// <summary>Current number of members.</summary>
    public int CurrentUserCount { get; set; }

    /// <summary>Monthly label limit of the plan.</summary>
    public int MonthlyLabelLimit { get; set; }

    /// <summary>Labels created this month.</summary>
    public int LabelsUsedThisMonth { get; set; }

    /// <summary>Whether the workspace is in a trial.</summary>
    public bool IsTrialing { get; set; }

    /// <summary>When the trial ends (UTC), if trialing.</summary>
    public DateTime? TrialExpiresAt { get; set; }

    /// <summary>The caller's role in the workspace.</summary>
    public WorkspaceRole MyRole { get; set; }

    /// <summary>Whether the caller can manage members.</summary>
    public bool CanManageMembers { get; set; }

    /// <summary>Whether the caller can manage billing.</summary>
    public bool CanManageBilling { get; set; }

    /// <summary>When the workspace was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Members and pending invitations of a workspace, as <c>GET api/workspaces/{id}/members</c> returns them.</summary>
public sealed class WorkspaceMemberList
{
    /// <summary>Current members.</summary>
    public List<WorkspaceMember> Members { get; set; } = [];

    /// <summary>Invitations not yet accepted.</summary>
    public List<WorkspaceInvite> PendingInvites { get; set; } = [];

    /// <summary>Maximum number of members.</summary>
    public int UserLimit { get; set; }

    /// <summary>Number of active members.</summary>
    public int ActiveCount { get; set; }

    /// <summary>Whether another member can be invited within the limit.</summary>
    public bool CanInviteMore { get; set; }
}

/// <summary>A workspace member.</summary>
public sealed class WorkspaceMember
{
    /// <summary>Membership ID.</summary>
    public string MembershipId { get; set; } = string.Empty;

    /// <summary>User ID; pass it to <c>RemoveMemberAsync</c> and <c>UpdateMemberRoleAsync</c>.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Display name, if set.</summary>
    public string? Name { get; set; }

    /// <summary>Role in the workspace.</summary>
    public WorkspaceRole Role { get; set; }

    /// <summary>Membership status number: 0 Active, 1 Pending, 2 Deactivated.</summary>
    public int Status { get; set; }

    /// <summary>When the member joined (UTC).</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Whether the member created the workspace.</summary>
    public bool IsCreator { get; set; }

    /// <summary>Whether the member is deactivated.</summary>
    public bool IsDeactivated { get; set; }
}

/// <summary>A pending workspace invitation.</summary>
public sealed class WorkspaceInvite
{
    /// <summary>Invitation ID.</summary>
    public string InviteId { get; set; } = string.Empty;

    /// <summary>Invited email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Role granted on acceptance.</summary>
    public WorkspaceRole Role { get; set; }

    /// <summary>When the invitation expires (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Name of the member who sent the invitation.</summary>
    public string InvitedByName { get; set; } = string.Empty;
}

/// <summary>Wallet balance of the configured workspace, as <c>GET api/wallet/my-balance</c> returns it.</summary>
public sealed class WalletBalance
{
    /// <summary>Whether the workspace has a wallet.</summary>
    public bool HasWallet { get; set; }

    /// <summary>Balance in US cents.</summary>
    public long BalanceCents { get; set; }

    /// <summary>Balance in US dollars.</summary>
    public decimal Balance { get; set; }
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

    /// <summary>Optional product reference (SKU or product ID).</summary>
    public string? SkuOrProductId { get; init; }
}

/// <summary>Body of <c>POST /api/addresses/validate</c>.</summary>
public sealed class AddressValidationRequest
{
    /// <summary>Recipient name.</summary>
    public string? Name { get; init; }

    /// <summary>Address line 1.</summary>
    public required string Line1 { get; init; }

    /// <summary>Address line 2.</summary>
    public string? Line2 { get; init; }

    /// <summary>City.</summary>
    public required string City { get; init; }

    /// <summary>State or province.</summary>
    public string? State { get; init; }

    /// <summary>Postal or ZIP code.</summary>
    public required string PostalCode { get; init; }

    /// <summary>ISO 3166-1 alpha-2 country code.</summary>
    public string Country { get; init; } = "US";
}

/// <summary>Result of an address validation check (Gateway <c>AddressValidationResult</c>).</summary>
public sealed class AddressValidationResult
{
    /// <summary>Whether the address is valid.</summary>
    public bool IsValid { get; set; }

    /// <summary>The carrier's corrected address, if it suggests one.</summary>
    public SuggestedAddress? SuggestedAddress { get; set; }

    /// <summary><c>true</c> residential, <c>false</c> commercial, <c>null</c> unknown.</summary>
    public bool? IsResidential { get; set; }

    /// <summary>Validation warnings, for example a missing suite number.</summary>
    public List<string> Warnings { get; set; } = [];

    /// <summary>The carrier that validated the address.</summary>
    public string? ValidatedBy { get; set; }

    /// <summary><c>true</c> when no carrier check could complete. This is not a rejection of the address.</summary>
    public bool ValidationUnavailable { get; set; }

    /// <summary>When the check ran (UTC); kept when a cached result is reused.</summary>
    public DateTimeOffset? CheckedAt { get; set; }
}

/// <summary>A carrier-suggested address correction.</summary>
public sealed class SuggestedAddress
{
    /// <summary>Address line 1.</summary>
    public string Line1 { get; set; } = string.Empty;

    /// <summary>Address line 2.</summary>
    public string? Line2 { get; set; }

    /// <summary>City.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>State or province.</summary>
    public string State { get; set; } = string.Empty;

    /// <summary>Postal or ZIP code.</summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>ISO country code.</summary>
    public string Country { get; set; } = string.Empty;
}

/// <summary>An HS (Harmonized System) code (Gateway <c>HsCodeResult</c>).</summary>
public sealed class HsCodeResult
{
    /// <summary>The HS code.</summary>
    public string HsCode { get; set; } = string.Empty;

    /// <summary>Description of the code.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Chapter (first two digits).</summary>
    public string Chapter { get; set; } = string.Empty;

    /// <summary>Description of the chapter.</summary>
    public string ChapterDescription { get; set; } = string.Empty;

    /// <summary>Duty rate in percent, when known for the destination.</summary>
    public decimal? DutyRatePercent { get; set; }

    /// <summary>Notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>Body of <c>POST /api/shipping/landed-cost</c>.</summary>
public sealed class LandedCostRequest
{
    /// <summary>ISO 3166-1 alpha-2 destination country code.</summary>
    public required string DestinationCountry { get; init; }

    /// <summary>The items shipped; <see cref="CustomsItem.Value"/> and <see cref="CustomsItem.WeightOz"/> are per unit.</summary>
    public required List<CustomsItem> Items { get; init; }

    /// <summary>Shipping cost in US dollars.</summary>
    public decimal ShippingCost { get; init; }
}

/// <summary>Estimated duties, taxes and total landed cost (Gateway <c>LandedCostEstimate</c>).</summary>
public sealed class LandedCostEstimate
{
    /// <summary>Shipping cost.</summary>
    public decimal ShippingCost { get; set; }

    /// <summary>Estimated duty.</summary>
    public decimal EstimatedDuty { get; set; }

    /// <summary>Estimated tax.</summary>
    public decimal EstimatedTax { get; set; }

    /// <summary>Shipping cost plus duty and tax.</summary>
    public decimal TotalLandedCost { get; set; }

    /// <summary>Currency code.</summary>
    public string Currency { get; set; } = "USD";

    /// <summary>The duty rate applied, as text.</summary>
    public string? DutyRateApplied { get; set; }

    /// <summary>Whether the figures are an estimate.</summary>
    public bool IsEstimate { get; set; }

    /// <summary>Disclaimer to show with the estimate.</summary>
    public string? Disclaimer { get; set; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/insurance/quote</c>.</summary>
public sealed class InsuranceQuoteRequest
{
    /// <summary>Declared value in US dollars (0.01 to 999,999.99).</summary>
    public required decimal DeclaredValue { get; init; }

    /// <summary>Carrier code, for example <c>usps</c>.</summary>
    public required string CarrierCode { get; init; }

    /// <summary>Tracking number, when the label already exists.</summary>
    public string? TrackingNumber { get; init; }

    /// <summary>Insurance provider; see <c>GetProvidersAsync</c>.</summary>
    public string? Provider { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/insurance/policies</c>.</summary>
public sealed class PurchaseInsuranceRequest
{
    /// <summary>Tracking number of the insured shipment.</summary>
    public required string TrackingNumber { get; init; }

    /// <summary>Carrier code, for example <c>usps</c>.</summary>
    public required string CarrierCode { get; init; }

    /// <summary>Declared value in US dollars (0.01 to 999,999.99).</summary>
    public required decimal DeclaredValue { get; init; }

    /// <summary>Coverage tier.</summary>
    public string? CoverageTier { get; init; }

    /// <summary>Insurance provider; see <c>GetProvidersAsync</c>.</summary>
    public string? Provider { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/insurance/claims</c>.</summary>
public sealed class FileInsuranceClaimRequest
{
    /// <summary>The insured policy.</summary>
    public required Guid PolicyId { get; init; }

    /// <summary>Why the claim is filed (up to 1,000 characters).</summary>
    public required string ClaimReason { get; init; }

    /// <summary>Amount claimed in US dollars (0.01 to 999,999.99).</summary>
    public required decimal ClaimAmount { get; init; }

    /// <summary>Supporting evidence as a JSON string.</summary>
    public string? EvidenceJson { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/returns/rma</c>.</summary>
public sealed class CreateRmaRequest
{
    /// <summary>The original order's numeric ID.</summary>
    public required long OrderId { get; init; }

    /// <summary>The order number (for example <c>SO16479</c>) the RMA is recorded against.</summary>
    public string? OrderNumber { get; init; }

    /// <summary>Why the items are returned (up to 500 characters).</summary>
    public required string Reason { get; init; }

    /// <summary>The returned items; at least one.</summary>
    public required List<RmaItem> Items { get; init; }

    /// <summary>Notes (up to 2,000 characters).</summary>
    public string? Notes { get; init; }

    /// <summary>Customer email for return notifications.</summary>
    public string? CustomerEmail { get; init; }

    /// <summary>A return reason ID from the workspace's reason types.</summary>
    public int? ReasonId { get; init; }
}

/// <summary>An item on a new RMA.</summary>
public sealed class RmaItem
{
    /// <summary>SKU.</summary>
    public required string Sku { get; init; }

    /// <summary>Quantity returned (1 to 9,999).</summary>
    public int Quantity { get; init; } = 1;

    /// <summary>Why this item is returned.</summary>
    public string? Reason { get; init; }
}

/// <summary>What happens to a received return item, sent as the Gateway disposition number.</summary>
public enum ReturnDisposition
{
    /// <summary>Put the item back in stock.</summary>
    Restock = 0,

    /// <summary>Dispose of the item.</summary>
    Dispose = 1,

    /// <summary>Forward the item elsewhere.</summary>
    Forward = 2
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/returns/{rmaId}/receive</c>.</summary>
public sealed class ReceiveRmaRequest
{
    /// <summary>The RMA number.</summary>
    public required string RmaNumber { get; init; }

    /// <summary>The receiving warehouse.</summary>
    public int WarehouseId { get; init; } = 1;

    /// <summary>The inspected items and what to do with each.</summary>
    public required List<ReceivedRmaItem> InspectedItems { get; init; }
}

/// <summary>An inspected return item.</summary>
public sealed class ReceivedRmaItem
{
    /// <summary>Part number (SKU).</summary>
    public required string PartNumber { get; init; }

    /// <summary>Part description.</summary>
    public string? PartDescription { get; init; }

    /// <summary>Quantity received.</summary>
    public required int Quantity { get; init; }

    /// <summary>What happens to the item.</summary>
    public ReturnDisposition Disposition { get; init; }

    /// <summary>Inspection notes.</summary>
    public string? InspectionNotes { get; init; }
}

/// <summary>A shipping rule condition.</summary>
public sealed class RuleCondition
{
    /// <summary>The shipment field compared, for example <c>weightOz</c>.</summary>
    public required string Field { get; init; }

    /// <summary>The comparison operator.</summary>
    public required string Operator { get; init; }

    /// <summary>The value compared against.</summary>
    public required string Value { get; init; }

    /// <summary>Condition group; conditions in one group are combined.</summary>
    public int? Group { get; init; }
}

/// <summary>A shipping rule action.</summary>
public sealed class RuleAction
{
    /// <summary>The action type.</summary>
    public required string Type { get; init; }

    /// <summary>The action value.</summary>
    public required string Value { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/shipping-rules</c>.</summary>
public sealed class CreateShippingRuleRequest
{
    /// <summary>Rule name (up to 200 characters).</summary>
    public required string Name { get; init; }

    /// <summary>Priority (0 to 10,000); rules are evaluated in priority order.</summary>
    public int Priority { get; init; }

    /// <summary>Whether the rule is enabled.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Limits the rule to one warehouse.</summary>
    public string? WarehouseId { get; init; }

    /// <summary>Conditions; at least one.</summary>
    public required List<RuleCondition> Conditions { get; init; }

    /// <summary>Actions; at least one.</summary>
    public required List<RuleAction> Actions { get; init; }
}

/// <summary>Body of <c>PUT /api/workspaces/{workspaceId}/shipping-rules/{ruleId}</c>. Only the set properties change.</summary>
public sealed class UpdateShippingRuleRequest
{
    /// <summary>New name.</summary>
    public string? Name { get; init; }

    /// <summary>New priority.</summary>
    public int? Priority { get; init; }

    /// <summary>Enables or disables the rule.</summary>
    public bool? IsEnabled { get; init; }

    /// <summary>Replaces the conditions.</summary>
    public List<RuleCondition>? Conditions { get; init; }

    /// <summary>Replaces the actions.</summary>
    public List<RuleAction>? Actions { get; init; }
}

/// <summary>A rule's new priority, for <c>ReorderAsync</c>.</summary>
public sealed class RulePriority
{
    /// <summary>The rule.</summary>
    public required Guid RuleId { get; init; }

    /// <summary>Its new priority.</summary>
    public required int Priority { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/shipping-rules/test</c>: a hypothetical shipment.</summary>
public sealed class TestShippingRuleRequest
{
    /// <summary>Package weight in ounces.</summary>
    public required decimal WeightOz { get; init; }

    /// <summary>Declared value in US dollars.</summary>
    public decimal DeclaredValue { get; init; }

    /// <summary>Destination country code.</summary>
    public string DestinationCountry { get; init; } = "US";

    /// <summary>Destination state.</summary>
    public string? DestinationState { get; init; }

    /// <summary>Destination postal code.</summary>
    public string? DestinationPostalCode { get; init; }

    /// <summary>Order source, for example <c>Shopify</c>.</summary>
    public string? OrderSource { get; init; }

    /// <summary>Number of items.</summary>
    public int ItemCount { get; init; } = 1;

    /// <summary>Requested shipping method.</summary>
    public string? ShipMethod { get; init; }

    /// <summary>Destination zone.</summary>
    public string? DestinationZone { get; init; }

    /// <summary>Shipment tags.</summary>
    public List<string> Tags { get; init; } = [];
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/pickups</c>.</summary>
public sealed class SchedulePickupRequest
{
    /// <summary>Carrier code: <c>usps</c>, <c>ups</c>, <c>fedex</c> or <c>dhl</c>.</summary>
    public required string CarrierCode { get; init; }

    /// <summary>Pickup date.</summary>
    public required DateTime PickupDate { get; init; }

    /// <summary>Pickup street address.</summary>
    public required string Street { get; init; }

    /// <summary>City.</summary>
    public required string City { get; init; }

    /// <summary>State.</summary>
    public required string State { get; init; }

    /// <summary>Postal code.</summary>
    public required string PostalCode { get; init; }

    /// <summary>ISO country code.</summary>
    public string Country { get; init; } = "US";

    /// <summary>Number of packages (1 to 999).</summary>
    public int PackageCount { get; init; } = 1;

    /// <summary>Total weight in ounces.</summary>
    public decimal? TotalWeightOz { get; init; }

    /// <summary>Instructions for the driver.</summary>
    public string? SpecialInstructions { get; init; }

    /// <summary>When the packages are ready.</summary>
    public DateTime? ReadyTime { get; init; }

    /// <summary>When the pickup location closes.</summary>
    public DateTime? CloseTime { get; init; }

    /// <summary>Contact name.</summary>
    public string? ContactName { get; init; }

    /// <summary>Contact phone number.</summary>
    public string? ContactPhone { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/carbon-offsets/quote</c>.</summary>
public sealed class CarbonOffsetQuoteRequest
{
    /// <summary>Shipment weight in pounds.</summary>
    public required decimal WeightLbs { get; init; }

    /// <summary>Shipping distance in miles.</summary>
    public required decimal DistanceMiles { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/carbon-offsets/purchase</c>.</summary>
public sealed class CarbonOffsetPurchaseRequest
{
    /// <summary>Tracking number of the offset shipment.</summary>
    public required string TrackingNumber { get; init; }

    /// <summary>Shipment weight in pounds.</summary>
    public required decimal WeightLbs { get; init; }

    /// <summary>Shipping distance in miles.</summary>
    public required decimal DistanceMiles { get; init; }
}

/// <summary>Carbon offset settings of a workspace.</summary>
public sealed class CarbonOffsetSettings
{
    /// <summary>Whether every new label is offset automatically.</summary>
    public bool AutoOffsetEnabled { get; set; }
}

/// <summary>Tokens and profile returned by login and token refresh (Gateway <c>LoginResponse</c>).</summary>
public sealed class LoginResponse
{
    /// <summary>The access (Bearer) token. <c>null</c> while a second factor is still required.</summary>
    public string? Token { get; set; }

    /// <summary>The refresh token; pass it to <c>RefreshTokenAsync</c>.</summary>
    public string? RefreshToken { get; set; }

    /// <summary>When the access token expires (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>User ID.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>First name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>The user's roles.</summary>
    public List<string> Roles { get; set; } = [];

    /// <summary>Whether a second factor must be verified before a token is issued.</summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>Token for the two-factor step, when one is required.</summary>
    public string? TwoFactorToken { get; set; }

    /// <summary>The two-factor method, when one is required.</summary>
    public string? TwoFactorMethod { get; set; }
}

/// <summary>How often a recurring shipment runs, sent as the Gateway <c>RecurrenceInterval</c> number.</summary>
public enum RecurrenceInterval
{
    /// <summary>Every day.</summary>
    Daily = 0,

    /// <summary>Every week.</summary>
    Weekly = 1,

    /// <summary>Every two weeks.</summary>
    Biweekly = 2,

    /// <summary>Every month.</summary>
    Monthly = 3,

    /// <summary>On a cron schedule; set <c>CronExpression</c>.</summary>
    Custom = 4
}

/// <summary>A recurring shipment schedule (Gateway <c>RecurringShipmentDto</c>).</summary>
public sealed class RecurringShipment
{
    /// <summary>Schedule ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace ID.</summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>Shipping preset that supplies the package and service.</summary>
    public string? PresetId { get; set; }

    /// <summary>Preset name, for display.</summary>
    public string? PresetName { get; set; }

    /// <summary>Destination address.</summary>
    public string? AddressId { get; set; }

    /// <summary>Destination address summary, for display.</summary>
    public string? AddressSummary { get; set; }

    /// <summary>Schedule name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>How often it runs.</summary>
    public RecurrenceInterval Interval { get; set; }

    /// <summary>Interval name, for display.</summary>
    public string IntervalName { get; set; } = string.Empty;

    /// <summary>Cron expression of a <see cref="RecurrenceInterval.Custom"/> schedule.</summary>
    public string? CronExpression { get; set; }

    /// <summary>Next run (UTC).</summary>
    public DateTime NextRunUtc { get; set; }

    /// <summary>Last run (UTC), if any.</summary>
    public DateTime? LastRunUtc { get; set; }

    /// <summary>Whether the schedule is active (not paused).</summary>
    public bool IsActive { get; set; }

    /// <summary>Consecutive failed runs.</summary>
    public int FailureCount { get; set; }

    /// <summary>Failed runs after which the schedule pauses itself.</summary>
    public int MaxFailures { get; set; }

    /// <summary>Error of the last failed run.</summary>
    public string? LastError { get; set; }

    /// <summary>When the schedule was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/recurring-shipments</c>.</summary>
public sealed class CreateRecurringShipmentRequest
{
    /// <summary>Schedule name (up to 200 characters).</summary>
    public required string Name { get; init; }

    /// <summary>Shipping preset that supplies the package and service.</summary>
    public Guid? PresetId { get; init; }

    /// <summary>Destination address; a run needs one.</summary>
    public Guid? AddressId { get; init; }

    /// <summary>How often it runs.</summary>
    public RecurrenceInterval Interval { get; init; }

    /// <summary>Cron expression (up to 100 characters); required for <see cref="RecurrenceInterval.Custom"/>.</summary>
    public string? CronExpression { get; init; }

    /// <summary>First run (UTC).</summary>
    public required DateTime FirstRunUtc { get; init; }

    /// <summary>Failed runs (1 to 100) after which the schedule pauses itself; the Gateway default is 3.</summary>
    public int? MaxFailures { get; init; }
}

/// <summary>Body of <c>PUT /api/workspaces/{workspaceId}/recurring-shipments/{id}</c>. Only the set properties change.</summary>
public sealed class UpdateRecurringShipmentRequest
{
    /// <summary>New name.</summary>
    public string? Name { get; init; }

    /// <summary>New shipping preset.</summary>
    public Guid? PresetId { get; init; }

    /// <summary>New destination address.</summary>
    public Guid? AddressId { get; init; }

    /// <summary>New interval.</summary>
    public RecurrenceInterval? Interval { get; init; }

    /// <summary>New cron expression.</summary>
    public string? CronExpression { get; init; }

    /// <summary>New failure limit (1 to 100).</summary>
    public int? MaxFailures { get; init; }
}

/// <summary>File format of a scheduled report, sent as the Gateway number.</summary>
public enum ReportFormat
{
    /// <summary>Comma-separated values.</summary>
    Csv = 0,

    /// <summary>JSON.</summary>
    Json = 1
}

/// <summary>How often a scheduled report runs, sent as the Gateway number.</summary>
public enum ReportFrequency
{
    /// <summary>Every day.</summary>
    Daily = 0,

    /// <summary>Every week, on <c>DayOfWeek</c>.</summary>
    Weekly = 1,

    /// <summary>Every month, on <c>DayOfMonth</c>.</summary>
    Monthly = 2
}

/// <summary>A scheduled report (Gateway <c>ReportScheduleDto</c>).</summary>
public sealed class ReportSchedule
{
    /// <summary>Schedule ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace ID.</summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>Report type, for example <c>shipments</c>.</summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>File format.</summary>
    public ReportFormat Format { get; set; }

    /// <summary>Report filters as a JSON string.</summary>
    public string? FilterParametersJson { get; set; }

    /// <summary>How often it runs.</summary>
    public ReportFrequency FrequencyType { get; set; }

    /// <summary>Frequency name, for display.</summary>
    public string FrequencyName { get; set; } = string.Empty;

    /// <summary>Day of a weekly schedule.</summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>Day (1 to 28) of a monthly schedule.</summary>
    public int? DayOfMonth { get; set; }

    /// <summary>Time of day (UTC) it runs.</summary>
    public TimeSpan TimeOfDayUtc { get; set; }

    /// <summary>Next run (UTC).</summary>
    public DateTime NextRunUtc { get; set; }

    /// <summary>Last run (UTC), if any.</summary>
    public DateTime? LastRunUtc { get; set; }

    /// <summary>Whether the schedule is enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Recipient email addresses, comma-separated.</summary>
    public string RecipientEmails { get; set; } = string.Empty;

    /// <summary>Consecutive failed runs.</summary>
    public int FailureCount { get; set; }

    /// <summary>Failed runs after which the schedule disables itself.</summary>
    public int MaxFailures { get; set; }

    /// <summary>Error of the last failed run.</summary>
    public string? LastError { get; set; }

    /// <summary>When the schedule was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/report-schedules</c>.</summary>
public sealed class CreateReportScheduleRequest
{
    /// <summary>Report type (up to 50 characters), for example <c>shipments</c>.</summary>
    public required string ReportType { get; init; }

    /// <summary>File format.</summary>
    public ReportFormat Format { get; init; }

    /// <summary>Report filters as a JSON string.</summary>
    public string? FilterParametersJson { get; init; }

    /// <summary>How often it runs.</summary>
    public ReportFrequency FrequencyType { get; init; }

    /// <summary>Day of a weekly schedule.</summary>
    public DayOfWeek? DayOfWeek { get; init; }

    /// <summary>Day (1 to 28) of a monthly schedule.</summary>
    public int? DayOfMonth { get; init; }

    /// <summary>Time of day (UTC) it runs; sent as <c>HH:mm:ss</c>.</summary>
    public required TimeSpan TimeOfDayUtc { get; init; }

    /// <summary>Recipient email addresses, comma-separated (up to 2,000 characters).</summary>
    public required string RecipientEmails { get; init; }

    /// <summary>Failed runs (1 to 100) after which the schedule disables itself; the Gateway default is 3.</summary>
    public int? MaxFailures { get; init; }
}

/// <summary>Body of <c>PUT /api/workspaces/{workspaceId}/report-schedules/{id}</c>. Only the set properties change.</summary>
public sealed class UpdateReportScheduleRequest
{
    /// <summary>New report type.</summary>
    public string? ReportType { get; init; }

    /// <summary>New file format.</summary>
    public ReportFormat? Format { get; init; }

    /// <summary>New report filters as a JSON string.</summary>
    public string? FilterParametersJson { get; init; }

    /// <summary>New frequency.</summary>
    public ReportFrequency? FrequencyType { get; init; }

    /// <summary>New day of a weekly schedule.</summary>
    public DayOfWeek? DayOfWeek { get; init; }

    /// <summary>New day of a monthly schedule.</summary>
    public int? DayOfMonth { get; init; }

    /// <summary>New time of day (UTC).</summary>
    public TimeSpan? TimeOfDayUtc { get; init; }

    /// <summary>New recipients, comma-separated.</summary>
    public string? RecipientEmails { get; init; }

    /// <summary>New failure limit (1 to 100).</summary>
    public int? MaxFailures { get; init; }
}

/// <summary>Tracking event an email template is sent for, as the Gateway <c>TrackingEmailEvent</c> number.</summary>
public enum TrackingEmailEvent
{
    /// <summary>A label was created.</summary>
    LabelCreated = 0,

    /// <summary>The shipment is in transit.</summary>
    InTransit = 1,

    /// <summary>The shipment is out for delivery.</summary>
    OutForDelivery = 2,

    /// <summary>The shipment was delivered.</summary>
    Delivered = 3,

    /// <summary>A delivery exception occurred.</summary>
    Exception = 4,

    /// <summary>The shipment is returning to the sender.</summary>
    ReturnToSender = 5
}

/// <summary>A shipment email template (Gateway <c>ShipmentEmailTemplateDto</c>).</summary>
public sealed class EmailTemplate
{
    /// <summary>Template ID.</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Workspace ID.</summary>
    public string WorkspaceId { get; set; } = string.Empty;

    /// <summary>The tracking event it is sent for.</summary>
    public TrackingEmailEvent EventType { get; set; }

    /// <summary>Event name, for display.</summary>
    public string EventTypeName { get; set; } = string.Empty;

    /// <summary>Subject template, with placeholders such as <c>{{tracking_number}}</c>.</summary>
    public string SubjectTemplate { get; set; } = string.Empty;

    /// <summary>HTML body template.</summary>
    public string HtmlBodyTemplate { get; set; } = string.Empty;

    /// <summary>Whether the template is sent.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>When the template was created (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the template was last changed (UTC).</summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Body of <c>PUT /api/workspaces/{workspaceId}/email-templates</c>: creates or replaces the template for <see cref="EventType"/>.</summary>
public sealed class UpsertEmailTemplateRequest
{
    /// <summary>The tracking event the template is sent for.</summary>
    public TrackingEmailEvent EventType { get; init; }

    /// <summary>Subject template (up to 500 characters).</summary>
    public required string SubjectTemplate { get; init; }

    /// <summary>HTML body template.</summary>
    public required string HtmlBodyTemplate { get; init; }

    /// <summary>Whether the template is sent; the Gateway default is <c>true</c>.</summary>
    public bool? IsEnabled { get; init; }
}

/// <summary>Body of <c>POST /api/workspaces/{workspaceId}/email-templates/preview</c>.</summary>
public sealed class EmailTemplatePreviewRequest
{
    /// <summary>Subject template.</summary>
    public required string SubjectTemplate { get; init; }

    /// <summary>HTML body template.</summary>
    public required string HtmlBodyTemplate { get; init; }
}

/// <summary>A template rendered with sample data.</summary>
public sealed class EmailTemplatePreview
{
    /// <summary>Rendered subject.</summary>
    public string RenderedSubject { get; set; } = string.Empty;

    /// <summary>Rendered HTML body.</summary>
    public string RenderedHtmlBody { get; set; } = string.Empty;
}

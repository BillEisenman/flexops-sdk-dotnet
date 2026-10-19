// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using FlexOps.Sdk.Resources;

namespace FlexOps.Sdk;

/// <summary>
/// Typed wrapper around <see cref="FlexOpsClient"/> that exposes domain-specific
/// resource properties for a fluent, discoverable API surface.
/// </summary>
/// <example>
/// <code>
/// using var client = new FlexOpsTypedClient("https://gateway.flexops.io", apiKey: "fxk_live_...");
/// client.WorkspaceId = "ws_abc123";
///
/// var rates = await client.Shipping.GetRatesAsync(new RateShoppingRequest
/// {
///     Origin = new() { AddressLine1 = "123 Main St", City = "New York", StateProvince = "NY", PostalCode = "10001" },
///     Destination = new() { AddressLine1 = "456 Oak Ave", City = "Los Angeles", StateProvince = "CA", PostalCode = "90210" },
///     Package = new() { Weight = 16, WeightUnit = "oz" }
/// });
/// // For live postage, use Shipping.PrepareLabelAsync and call PurchaseLabelAsync only after explicit approval.
///
/// // Direct carrier operations
/// var uspsLabel = await client.Carriers.Usps.CreateDomesticLabelAsync(fullUspsPayload);
/// </code>
/// </example>
public sealed class FlexOpsTypedClient : IDisposable
{
    private readonly FlexOpsClient _client;

    /// <summary>Gets or sets the active workspace ID used to scope requests.</summary>
    public string? WorkspaceId
    {
        get => _client.WorkspaceId;
        set => _client.WorkspaceId = value;
    }

    /// <summary>Token acquisition for signed-in user operations (login, refresh, logout).</summary>
    public AuthResource Auth { get; }

    /// <summary>Workspaces the caller belongs to, their members and invitations.</summary>
    public WorkspacesResource Workspaces { get; }

    /// <summary>Normalized shipping operations (rates, labels, tracking, batch).</summary>
    public ShippingResource Shipping { get; }

    /// <summary>Carrier-specific lookups (USPS, UPS, DHL) and deprecated legacy proxy writes (USPS, UPS, FedEx, DHL).</summary>
    public CarriersResource Carriers { get; }

    /// <summary>Webhook subscription management and signature verification.</summary>
    public WebhooksResource Webhooks { get; }

    /// <summary>Wallet balance of the configured workspace.</summary>
    public WalletResource Wallet { get; }

    /// <summary>Shipping insurance quotes, purchases, and claims.</summary>
    public InsuranceResource Insurance { get; }

    /// <summary>Return authorization (RMA) management.</summary>
    public ReturnsResource Returns { get; }

    /// <summary>Analytics (shipments, carriers, orders, returns).</summary>
    public AnalyticsResource Analytics { get; }

    /// <summary>Read-only order lookups via the VisionSuite proxy.</summary>
    public OrdersResource Orders { get; }

    /// <summary>Read-only inventory lookups via the VisionSuite proxy.</summary>
    public InventoryResource Inventory { get; }

    /// <summary>Carrier pickup scheduling.</summary>
    public PickupsResource Pickups { get; }

    /// <summary>USPS scan form (manifest) management.</summary>
    public ScanFormsResource ScanForms { get; }

    /// <summary>Shipping automation rules.</summary>
    public RulesResource Rules { get; }

    /// <summary>Harmonized System (HS) code search, lookup, and landed cost estimation.</summary>
    public HsCodesResource HsCodes { get; }

    /// <summary>Carbon offsets: quotes, purchases, history and the auto-offset setting.</summary>
    public CarbonOffsetsResource CarbonOffsets { get; }

    /// <summary>Recurring shipment schedules.</summary>
    public RecurringShipmentsResource RecurringShipments { get; }

    /// <summary>Scheduled reports emailed to recipients (called <c>Reports</c> in 2.x).</summary>
    public ReportSchedulesResource ReportSchedules { get; }

    /// <summary>Shipment notification email templates, one per tracking event.</summary>
    public EmailTemplatesResource EmailTemplates { get; }

    /// <summary>
    /// Creates a new typed FlexOps client.
    /// </summary>
    /// <param name="baseUrl">The FlexOps Gateway API base URL.</param>
    /// <param name="apiKey">Optional workspace API key for X-Api-Key authentication.</param>
    /// <param name="accessToken">Optional JWT access token for Bearer authentication.</param>
    /// <param name="workspaceId">Optional default workspace ID.</param>
    /// <param name="httpClient">Optional pre-configured HttpClient.</param>
    public FlexOpsTypedClient(
        string baseUrl,
        string? apiKey = null,
        string? accessToken = null,
        string? workspaceId = null,
        HttpClient? httpClient = null)
    {
        _client = new FlexOpsClient(baseUrl, apiKey, accessToken, workspaceId, httpClient);

        Auth = new AuthResource(_client);
        Workspaces = new WorkspacesResource(_client);
        Shipping = new ShippingResource(_client);
        Carriers = new CarriersResource(_client);
        Webhooks = new WebhooksResource(_client);
        Wallet = new WalletResource(_client);
        Insurance = new InsuranceResource(_client);
        Returns = new ReturnsResource(_client);
        Analytics = new AnalyticsResource(_client);
        Orders = new OrdersResource(_client);
        Inventory = new InventoryResource(_client);
        Pickups = new PickupsResource(_client);
        ScanForms = new ScanFormsResource(_client);
        Rules = new RulesResource(_client);
        HsCodes = new HsCodesResource(_client);
        CarbonOffsets = new CarbonOffsetsResource(_client);
        RecurringShipments = new RecurringShipmentsResource(_client);
        ReportSchedules = new ReportSchedulesResource(_client);
        EmailTemplates = new EmailTemplatesResource(_client);
    }

    /// <summary>Sets the JWT access token for Bearer authentication.</summary>
    public void SetAccessToken(string token) => _client.SetAccessToken(token);

    /// <summary>Sets the API key for authentication.</summary>
    public void SetApiKey(string key) => _client.SetApiKey(key);

    /// <inheritdoc />
    public void Dispose() => _client.Dispose();
}

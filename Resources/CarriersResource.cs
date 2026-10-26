// ***********************************************************************
// Package          : FlexOps.Sdk
// Author           : FlexOps, LLC
// Created          : 2026-03-08
//
// Copyright (c) 2021-2026 by FlexOps, LLC. All rights reserved.
// ***********************************************************************

using FlexOps.Sdk.Resources.Carriers;

namespace FlexOps.Sdk.Resources;

/// <summary>
/// Carrier-specific operations via the legacy VisionSuite Core Services API proxy (USPS, UPS, FedEx, DHL).
/// Use the high-level <see cref="ShippingResource"/> for rates, labels, tracking and address validation.
/// </summary>
/// <remarks>
/// <para>
/// The GET lookups work with API keys that have the permission for the data they read: address, ZIP, location,
/// service point and reference lookups require <c>shipping:read</c>; tracking and proof of delivery require
/// <c>tracking:read</c>; delivery standards, rates and products require <c>rates:read</c>. A key without the
/// permission gets 403 <c>API_KEY_SCOPE_REQUIRED</c>; keys created with the default permissions have all three.
/// </para>
/// <para>
/// The write methods (POST, PUT, DELETE through <c>api/ApiProxy</c>) are
/// <see cref="ObsoleteAttribute">obsolete</see>: they need a user token from <see cref="AuthResource.LoginAsync"/>
/// (API keys get 403 <c>API_KEY_SCOPE_REQUIRED</c>) and stop working when the proxy is retired on 2027-06-30.
/// Native replacements: <see cref="ShippingResource"/> (rates, labels, cancellation, tracking, address validation),
/// <see cref="PickupsResource"/> (pickups) and <see cref="ScanFormsResource"/> (scan forms).
/// </para>
/// <para>
/// The proxy's label-generation routes answer 410 Gone, so the SDK has no proxy label methods.
/// </para>
/// </remarks>
public sealed class CarriersResource
{
    /// <summary>Message of the <see cref="ObsoleteAttribute"/> on every legacy proxy write.</summary>
    internal const string LegacyProxyWriteMessage =
        "Legacy carrier proxy (sunset 2027-06-30). Needs a user token: API keys get 403 API_KEY_SCOPE_REQUIRED. " +
        "Use client.Shipping / client.Pickups / client.ScanForms where they cover this.";

    /// <summary>USPS lookups (V3 endpoints), domestic labels, and deprecated proxy writes.</summary>
    public UspsResource Usps { get; }

    /// <summary>UPS tracking (V2 endpoints) and deprecated proxy writes.</summary>
    public UpsResource Ups { get; }

    /// <summary>FedEx deprecated proxy writes (V3 endpoints); all need a user token.</summary>
    public FedExResource FedEx { get; }

    /// <summary>DHL lookups (V2 endpoints) and deprecated proxy writes.</summary>
    public DhlResource Dhl { get; }

    /// <summary>Initializes a new instance of <see cref="CarriersResource"/>.</summary>
    public CarriersResource(FlexOpsClient client)
    {
        Usps = new UspsResource(client);
        Ups = new UpsResource(client);
        FedEx = new FedExResource(client);
        Dhl = new DhlResource(client);
    }

    /// <summary>Builds <c>api/ApiProxy/{path}</c> with <paramref name="parameters"/> as the query string.</summary>
    internal static string ProxyPath(string path, IReadOnlyDictionary<string, string>? parameters)
        => FlexOpsClient.WithQuery($"api/ApiProxy/{path}", parameters?.Select(p => new KeyValuePair<string, string?>(p.Key, p.Value)));
}

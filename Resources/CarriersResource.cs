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
/// Direct carrier-specific operations via the VisionSuite Core Services API proxy.
/// Provides pass-through access to USPS, UPS, FedEx, and DHL endpoints.
/// Use the high-level <see cref="ShippingResource"/> for normalized operations,
/// or these carrier-specific resources when you need full control over the payload.
/// </summary>
public sealed class CarriersResource
{
    /// <summary>USPS carrier operations (V3 endpoints).</summary>
    public UspsResource Usps { get; }

    /// <summary>UPS carrier operations (V2 endpoints).</summary>
    public UpsResource Ups { get; }

    /// <summary>FedEx carrier operations (V3 endpoints).</summary>
    public FedExResource FedEx { get; }

    /// <summary>DHL carrier operations (V2 endpoints).</summary>
    public DhlResource Dhl { get; }

    /// <summary>Initializes a new instance of <see cref="CarriersResource"/>.</summary>
    public CarriersResource(FlexOpsClient client)
    {
        Usps = new UspsResource(client);
        Ups = new UpsResource(client);
        FedEx = new FedExResource(client);
        Dhl = new DhlResource(client);
    }
}

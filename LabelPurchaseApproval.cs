// Copyright (c) FlexOps, LLC. All rights reserved.
using System.Text.Json;

namespace FlexOps.Sdk;

/// <summary>Immutable request and key paired with the server preview. Contains private shipment data; store securely if persisted.</summary>
public sealed record LabelPurchaseApproval(
    string IdempotencyKey,
    string RequestJson,
    JsonElement Preview,
    Label? SandboxLabel);

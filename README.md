# FlexOps .NET SDK

Official .NET client for the [FlexOps](https://flexops.io) multi-carrier shipping platform — typed, resource-based, async/await throughout.

## Installation

```bash
dotnet add package FlexOps.Sdk
```

## Quick start

The `FlexOpsTypedClient` is the primary entry point. It exposes 20 resource properties — one per Gateway domain — each with strongly-typed methods, XML doc comments, and `CancellationToken` support.

```csharp
using FlexOps.Sdk;

using var client = new FlexOpsTypedClient(
    baseUrl: "https://gateway.flexops.io",
    apiKey: "fxk_test_...",      // sandbox key — see below
    workspaceId: "ws_abc123"
);

// Rate shop across all connected carriers
var rateRequest = new RateShoppingRequest
{
    Origin = new() { AddressLine1 = "123 Main St", City = "New York", StateProvince = "NY", PostalCode = "10001" },
    Destination = new() { AddressLine1 = "456 Oak Ave", City = "Los Angeles", StateProvince = "CA", PostalCode = "90210" },
    Package = new() { Weight = 16, WeightUnit = "oz" }
};
var rates = await client.Shipping.GetRatesAsync(rateRequest);

// Cheapest rate only
var cheapest = await client.Shipping.GetCheapestRateAsync(rateRequest);

// Prepare a bounded purchase. Retain this operation securely for all retries.
var operation = await client.Shipping.PrepareLabelAsync(new
{
    origin = rateRequest.Origin, destination = rateRequest.Destination,
    package = rateRequest.Package, carrierCode = "USPS", serviceCode = "PRIORITY"
}, maximumPostageAmount: 10m, idempotencyKey: Guid.NewGuid().ToString());

Label? label = operation.SandboxLabel;
if (label is null)
{
    Console.WriteLine($"Postage: {operation.Preview.GetProperty("quotedPostageAmount")} USD; maximum: {operation.Preview.GetProperty("maximumPostageAmount")} USD; expires: {operation.Preview.GetProperty("expiresAt")}");
    Console.WriteLine("Type approve to buy at the displayed maximum (later adjustments/fees excluded):");
    if (Console.ReadLine() != "approve") return;
    label = await client.Shipping.PurchaseLabelAsync(operation);
}

// Track
var tracking = await client.Shipping.TrackAsync("9400111899223456789012");

// Create a return
var rma = await client.Returns.CreateRmaAsync(new { originalShipmentId = label!.LabelId, reason = "wrong_size" });
```

## Resources

The typed client exposes:

| Property | Domain |
| --- | --- |
| `client.Auth` | Login, register, password management |
| `client.Workspaces` | Workspace CRUD, members, invitations |
| `client.Shipping` | Rates, labels, tracking, batch, address validation |
| `client.Carriers` | Direct carrier-specific ops (USPS, UPS, FedEx, DHL) |
| `client.Webhooks` | Subscriptions + signature verification |
| `client.Wallet` | Balance, top-up, transactions |
| `client.Insurance` | Quotes, purchases, claims |
| `client.Returns` | RMAs, return labels, batch disposition |
| `client.ApiKeys` | Create, revoke, rotate |
| `client.Analytics` | Shipments, carriers, orders, returns |
| `client.Orders` | Order CRUD via VisionSuite proxy |
| `client.Inventory` | Inventory CRUD via VisionSuite proxy |
| `client.Pickups` | Carrier pickup scheduling |
| `client.ScanForms` | USPS scan form / manifest management |
| `client.Rules` | Shipping automation rules |
| `client.Offsets` | Carbon offset operations |
| `client.HsCodes` | HS code search, lookup, landed cost |
| `client.RecurringShipments` | Schedule management |
| `client.EmailTemplates` | Branded email templates |
| `client.Reports` | Scheduled reports |

## Authentication

### Workspace API key (recommended)

```csharp
using var client = new FlexOpsTypedClient(
    "https://gateway.flexops.io",
    apiKey: "fxk_live_...",
    workspaceId: "ws_abc123");
```

### JWT bearer token

```csharp
using var client = new FlexOpsTypedClient("https://gateway.flexops.io");
client.SetAccessToken("eyJhbGciOi...");
client.WorkspaceId = "ws_abc123";
```

### Custom HttpClient

Useful for `IHttpClientFactory`, custom handlers (telemetry, retries), or DI:

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("https://gateway.flexops.io/") };
using var client = new FlexOpsTypedClient(
    "https://gateway.flexops.io",
    apiKey: "fxk_live_...",
    httpClient: httpClient);
```

## Sandbox

Use an `fxk_test_...` API key instead of `fxk_live_...` to hit the sandbox. Mock carriers respond, no real charges, no real labels — useful for CI, integration tests, and demos. Tracking numbers are prefixed `SBOX...` so you can tell them apart from production.

## Advanced: lower-level HTTP client

`FlexOpsClient` is the underlying HTTP client used by every resource. Use it directly when you need an endpoint that hasn't been wrapped yet, or when you want to call a future endpoint without waiting for a SDK release.

```csharp
using var client = new FlexOpsClient(
    "https://gateway.flexops.io",
    apiKey: "fxk_live_...",
    workspaceId: "ws_abc123");

// Path helpers: WsPath() prefixes with /api/workspaces/{workspaceId}/
var custom = await client.PostAsync<ApiResponse<MyType>>(
    client.WsPath("some/new/endpoint"),
    new { /* payload */ });
```

`FlexOpsTypedClient` wraps `FlexOpsClient` and forwards auth + workspace context, so you can mix and match if needed.

## Curl quickstart

If you'd rather verify the API directly before wiring the SDK:

```bash
# Rate shop
curl -X POST https://gateway.flexops.io/api/shipping/rates \
  -H "X-API-Key: fxk_test_..." \
  -H "Content-Type: application/json" \
  -d '{
    "origin": {"addressLine1": "123 Main St", "city": "New York", "stateProvince": "NY", "postalCode": "10001", "countryCode": "US"},
    "destination": {"addressLine1": "456 Oak Ave", "city": "Los Angeles", "stateProvince": "CA", "postalCode": "90210", "countryCode": "US"},
    "package": {"weight": 16, "weightUnit": "oz"}
  }'

# Buy a label
curl -X POST https://gateway.flexops.io/api/workspaces/ws_abc123/shipping/labels \
  -H "X-API-Key: fxk_test_..." \
  -H "Content-Type: application/json" \
  -d '{
    "carrier":  "USPS",
    "service":  "PRIORITY_MAIL",
    "fromAddress": {"name": "Warehouse", "street1": "123 Main St", "city": "New York",   "state": "NY", "zip": "10001", "country": "US"},
    "toAddress":   {"name": "Customer",  "street1": "456 Oak Ave", "city": "Los Angeles", "state": "CA", "zip": "90210", "country": "US"},
    "parcel":   {"weight": 16, "weightUnit": "oz"}
  }'

# Track
curl https://gateway.flexops.io/api/workspaces/ws_abc123/shipping/track/9400111899223456789012 \
  -H "X-API-Key: fxk_test_..."
```

## Requirements

- .NET 10.0 or higher

## Links

- [FlexOps Platform](https://flexops.io)
- [API Reference](https://flexops.io/docs/api/reference) — interactive Scalar reference
- [Quickstart guide](https://flexops.io/docs/quickstart) — first label in 5 minutes
- [Source on GitHub](https://github.com/BillEisenman/flexops-sdk-dotnet)

## License

MIT

## Guarded label purchase contract

These methods require a Gateway deployment with the bounded approval contract. Coordinate Gateway/client activation; an older Gateway does not provide the new preview guarantee. Prepare never automatically approves. Your application must display the quote and obtain approval before calling PurchaseLabelAsync. The maximum covers pre-dispatch postage, not later carrier adjustments or separate fees.

Retain the operation's request JSON and idempotency key securely, with the original Gateway and credential identity. On a timeout or OutcomeUnknown, retry that operation only or reconcile with an operator. Do not regenerate keys. Completed purchases can replay after token expiry. Sandbox preparation returns SandboxLabel and does not create real postage. Legacy CreateLabelAsync and the USPS wrapper reject preview responses rather than misreporting a label; use the shared Shipping prepare/purchase methods for live purchases.

Run `dotnet run --project tests/FlexOps.Sdk.ContractCheck --configuration Release` for the repository-owned rate and approval/replay checks.

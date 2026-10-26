# FlexOps .NET SDK

Official .NET client for the [FlexOps](https://flexops.io) multi-carrier shipping platform — typed, resource-based, async/await throughout.

## Installation

```bash
dotnet add package FlexOps.Sdk
```

## Quick start

The `FlexOpsTypedClient` is the primary entry point. It exposes 19 resource properties — one per Gateway domain — each with strongly-typed methods, XML doc comments, and `CancellationToken` support.

```csharp
using FlexOps.Sdk;

using var client = new FlexOpsTypedClient(
    baseUrl: "https://gateway.flexops.io",
    apiKey: "test_...",          // sandbox key — see below
    workspaceId: "your-workspace-id"
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
    package = rateRequest.Package, carrierCode = "USPS", serviceCode = "PRIORITY",
    // Required without an orderId. The SDK never fills it in; None (0) = not hazardous.
    hazmatClassification = HazmatClassification.None
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

// Create a return authorization for an order
var rma = await client.Returns.CreateAsync(new CreateRmaRequest
{
    OrderId = 1042,
    Reason = "Wrong size",
    Items = [new RmaItem { Sku = "TS-1", Quantity = 1 }],
});

// Buy the return label. Send the package weight and size: anything left out comes from the
// workspace's default shipping preset, and with no preset the API answers 400 PackageMeasurementsRequired.
var returnLabel = await client.Returns.GenerateLabelAsync("RMA-1001",
    new ReturnLabelRequest { WeightOz = 16, LengthIn = 10, WidthIn = 8, HeightIn = 4 });

// Read the RMA back by its number. Returns.GetAsync reads a *received* return by its numeric return ID.
var current = await client.Returns.GetRmaAsync("RMA-1001");

// One order in several boxes: one label per box, same carrier and service. Multi-package labels
// cannot carry HAZMAT, so HazmatClassification is None or null. MaximumPostageAmount is required: the
// most the whole shipment may cost in USD. The Gateway will not buy a box that takes the total above it.
try
{
    var shipment = await client.Shipping.CreateMultiPackageAsync(new MultiPackageShipmentRequest
    {
        OrderId = 1042,
        CarrierCode = "USPS",
        ServiceCode = "PRIORITY",
        HazmatClassification = HazmatClassification.None,
        MaximumPostageAmount = 25m,
        Packages =
        [
            new MultiPackageDetail { WeightInOunces = 16, LengthInInches = 10, WidthInInches = 8, HeightInInches = 4 },
            new MultiPackageDetail { WeightInOunces = 8, LengthInInches = 6, WidthInInches = 6, HeightInInches = 6 },
        ],
    }, idempotencyKey: "order-1042-boxes");
    // At least one label was bought; check each box (shipment.Data.Labels) and FailedPackages.
}
catch (FlexOpsException ex) when (ex.ErrorCode is "AllPackagesFailed" or "OutcomeUnknown")
{
    // No label was bought. ex.ResponseData holds the per-box results; after OutcomeUnknown a label
    // may exist, so check the order before buying again.
}
```

## Resources

The typed client exposes:

| Property | Domain |
| --- | --- |
| `client.Auth` | Login, token refresh, logout (for operations that need a signed-in user) |
| `client.Workspaces` | Your workspaces, members, invitations |
| `client.Shipping` | Rates, labels, multi-package labels, tracking, batch, address validation |
| `client.Carriers` | Carrier lookups (USPS, UPS, DHL); deprecated proxy writes (USPS, UPS, FedEx, DHL) |
| `client.Webhooks` | Subscriptions, test deliveries, signature verification (subscriptions need a signed-in user) |
| `client.Wallet` | Workspace wallet balance |
| `client.Insurance` | Quotes, policies, claims |
| `client.Returns` | RMAs, return labels, receiving |
| `client.Analytics` | Shipments, carriers, orders, returns |
| `client.Orders` | Read-only order lookups via the VisionSuite proxy (API keys need `orders:read`) |
| `client.Inventory` | Read-only inventory lookups via the VisionSuite proxy (API keys need `inventory:read`) |
| `client.Pickups` | Carrier pickup scheduling |
| `client.ScanForms` | USPS scan form / manifest management |
| `client.Rules` | Shipping automation rules, including dry-run tests |
| `client.HsCodes` | HS code search, lookup, landed cost |
| `client.CarbonOffsets` | Carbon offset quotes, purchases, history and settings |
| `client.RecurringShipments` | Recurring shipment schedules (pause, resume, run now) |
| `client.ReportSchedules` | Scheduled reports emailed to recipients (`client.Reports` in 2.x) |
| `client.EmailTemplates` | Shipment notification email templates, one per tracking event |

The SDK covers the documented public Gateway API. Account, API-key and billing management live in the FlexOps
portal. Orders and inventory are read-only lookups through the legacy VisionSuite proxy, which refuses writes from
API keys.

Reads through the proxy need the API key permission for the data they read: `orders:read` for `client.Orders`,
`inventory:read` for `client.Inventory`, and for `client.Analytics` the permission named on each method
(`shipping:read`, `inventory:read`, `orders:read` or `returns:read`). A key without the permission gets 403
`API_KEY_SCOPE_REQUIRED` with `requiredScopes`. Keys created with the default permissions have all of these.

`RecurringShipments`, `ReportSchedules` and `EmailTemplates` reads work with any API key or a user token. Their
writes (create, update, delete, pause, resume, trigger, toggle, upsert, and email-template preview) need a user token
from `client.Auth.LoginAsync`, or an API key granted the resource's opt-in scope: `recurring-shipments:write`,
`report-schedules:write` or `email-templates:write`. Older keys don't have these scopes; an API key without one gets
403 `API_KEY_SCOPE_REQUIRED` with `requiredScopes`.

```csharp
// Wallet balance of the configured workspace (sent with X-Workspace-Id)
var balance = await client.Wallet.GetBalanceAsync();
Console.WriteLine($"{balance!.Balance:C} ({balance.BalanceCents} cents)");

// Address validation
var check = await client.Shipping.ValidateAddressAsync(new AddressValidationRequest
{
    Line1 = "1 Main St", City = "Albany", State = "NY", PostalCode = "12207",
});
if (check!.ValidationUnavailable) { /* no carrier answered; this is not a rejection */ }

// Invite a member; roles go out as the Gateway's WorkspaceRole numbers
await client.Workspaces.InviteMemberAsync("ops@example.com", WorkspaceRole.Admin);
```

## Webhook subscriptions

Managing webhooks needs a signed-in user's access token. The Gateway refuses API keys on every webhook route,
reads included (`ListAsync` and `ListDeliveryLogsAsync` as well as `CreateAsync`, `UpdateAsync`, `DeleteAsync`,
`RotateSecretAsync` and `TestAsync`), with 403 `SIGNED_IN_USER_REQUIRED`, whatever the key's scopes; the SDK throws
`FlexOpsAuthException` with that `ErrorCode`. Reads are refused too because webhook delivery logs carry each
delivery's payload (order and customer data). A client constructed with an API key sends only the key, so use a
client constructed without `apiKey`. Verifying deliveries (below) makes no request and needs no credentials.

```csharp
// Sign in as a user (or pass accessToken:); an API key gets 403 SIGNED_IN_USER_REQUIRED here.
using var client = new FlexOpsTypedClient(baseUrl: "https://gateway.flexops.io", workspaceId: "your-workspace-id");
await client.Auth.LoginAsync(new { email = "user@company.com", password = "your-password" });

// Leave Events null to receive every event.
var webhook = await client.Webhooks.CreateAsync(new CreateWebhookRequest
{
    Url = "https://your-app.com/webhooks/flexops",
    Events = [WebhookEventType.LabelCreated, WebhookEventType.LabelCanceled, WebhookEventType.BatchCompleted],
});
var signingSecret = webhook!.Data!.Secret; // returned only on create; store it

// Change the events or pause the subscription; unset properties are left as they are.
await client.Webhooks.UpdateAsync(webhook.Data.Id, new UpdateWebhookRequest
{
    Events = [WebhookEventType.LabelCreated],
    Status = "Paused",
});
```

The SDK sends `Events` as the Gateway's comma-separated `eventTypes` (`"LabelCreated,LabelCanceled,BatchCompleted"`).
A subscription's `EventTypes` comes back in the same form, or `*` for every event. FlexOps sends these events,
named exactly as below. Names are PascalCase and matched exactly, so a name like `label.created` would never
match; `CreateAsync` and `UpdateAsync` throw `ArgumentException` for an empty list or an undefined value
without calling the API.

| Event | Sent when |
| --- | --- |
| `LabelCreated` | A live label is purchased (sandbox labels send nothing) |
| `LabelCanceled` | A live label is voided |
| `BatchCompleted` | A batch label job finishes |
| `TrackingUpdated` | A USPS tracking scan arrives (USPS push-tracking pilot workspaces only) |
| `TrackingDelivered` | A USPS package is delivered (USPS push-tracking pilot workspaces only) |

The `CreateAsync(object)` and `UpdateAsync(string, object)` overloads send their payload as given. Use
`eventTypes` (one comma-separated string, no spaces) there; the Gateway ignores an `events` field and
subscribes to every event.

## Webhook verification

Each delivery is a `POST` whose body is the event data itself (there is no `{ "type", "data" }`
envelope). The headers carry the rest:

| Header | Value |
| --- | --- |
| `X-FlexOps-Event` | Event name, e.g. `LabelCreated`; `test` for a test delivery |
| `X-FlexOps-Event-Id` | Event ID; use it to ignore retried duplicates |
| `X-FlexOps-Timestamp` | Unix seconds when the delivery was signed |
| `X-FlexOps-Signature` | `sha256=` + hex HMAC-SHA256 of `{timestamp}.{raw body}`, keyed with your `whsec_...` secret |
| `X-FlexOps-Delivery` | Delivery attempt ID |

Verify against the raw body; re-serialized JSON does not reproduce what was signed.

```csharp
using System.Text;
using FlexOps.Sdk.Resources;

app.MapPost("/webhooks/flexops", async (HttpRequest request, IConfiguration config) =>
{
    using var reader = new StreamReader(request.Body, Encoding.UTF8);
    var body = await reader.ReadToEndAsync();

    var isValid = WebhooksResource.VerifySignature(
        body,
        request.Headers["X-FlexOps-Signature"],
        config["FlexOps:WebhookSecret"]!,
        request.Headers["X-FlexOps-Timestamp"]); // rejected if more than 5 minutes from now
    if (!isValid)
    {
        return Results.Unauthorized();
    }

    var eventType = request.Headers["X-FlexOps-Event"].ToString(); // "LabelCreated", "TrackingUpdated", "test", ...
    using var data = System.Text.Json.JsonDocument.Parse(body);      // the event data; no envelope
    // Process the event...
    return Results.Ok();
});
```

Pass `tolerance:` to change the five-minute timestamp window.

## Authentication

### Workspace API key (recommended)

```csharp
using var client = new FlexOpsTypedClient(
    "https://gateway.flexops.io",
    apiKey: "live_...",
    workspaceId: "your-workspace-id");
```

### JWT bearer token

```csharp
using var client = new FlexOpsTypedClient("https://gateway.flexops.io");
client.SetAccessToken("eyJhbGciOi...");
client.WorkspaceId = "your-workspace-id";
```

### Custom HttpClient

Useful for `IHttpClientFactory`, custom handlers (telemetry, retries), or DI:

```csharp
var httpClient = new HttpClient { BaseAddress = new Uri("https://gateway.flexops.io/") };
using var client = new FlexOpsTypedClient(
    "https://gateway.flexops.io",
    apiKey: "live_...",
    httpClient: httpClient);
```

## Errors, retries and idempotency

A non-success response throws `FlexOpsException` with the HTTP `StatusCode` and the Gateway `ErrorCode`
(for example `API_KEY_SCOPE_REQUIRED`, `PackageMeasurementsRequired`, `IDEMPOTENCY_CONFLICT`); 401 and 403 throw
`FlexOpsAuthException` and 429 throws `FlexOpsRateLimitException`. A success with no body (204) returns `null`.

The SDK does not retry. Every POST and PUT carries an `Idempotency-Key` header: the key you pass (label
purchases, `Insurance.PurchaseAsync`, `CarbonOffsets.PurchaseAsync`) or a new GUID for that call. A resilience
handler on your `HttpClient` resends the same request, and so the same key, and the Gateway replays a write it
already finished instead of charging again. Retry safely:

| Request | Retry on |
| --- | --- |
| GET | network error, timeout, 429, 500, 502, 503, 504 |
| POST, PUT | network error, timeout, 429, 502, 503, 504, and 409 `IDEMPOTENCY_CONFLICT`; **never 500**, which the Gateway does not keep for replay |
| DELETE | 429 only (DELETE is not deduplicated) |

Reusing a key with a different request gets 422 `IDEMPOTENCY_KEY_REUSED`: it surfaces as `FlexOpsException` with
that `ErrorCode` and must not be retried. Only 409 `IDEMPOTENCY_CONFLICT`, matched exactly, means the first request
with that key is still running. The label-purchase and managed-return `IdempotencyConflict` 409 (an altered request)
is not retryable either.

## Carrier-specific operations

`client.Carriers` reaches USPS, UPS, FedEx and DHL through the legacy VisionSuite proxy (`api/ApiProxy/...`).

- **Lookups (GET)** work with API keys: USPS address, ZIP and city lookups, tracking, delivery standards and
  locations; UPS tracking; DHL address validation, rates, products, tracking, proof of delivery, reference data and
  service points. Pass parameters as an `IReadOnlyDictionary<string, string>`; they go in the query string.
  The key needs the permission for the data: `shipping:read` for address, ZIP, city, location, service point and
  reference lookups; `tracking:read` for tracking and proof of delivery; `rates:read` for delivery standards, rates
  and products. Without it the Gateway returns 403 `API_KEY_SCOPE_REQUIRED`.
- **Writes (POST, PUT, DELETE)** are `[Obsolete]`. They need a user token (API keys get 403 `API_KEY_SCOPE_REQUIRED`)
  and stop working when the proxy is retired on 2027-06-30. The proxy's label routes already answer 410 Gone, so the
  SDK has no proxy label methods. Native replacements:

| Instead of | Use |
| --- | --- |
| Carrier rate searches and quotes | `client.Shipping.GetRatesAsync`, `GetCheapestRateAsync`, `GetFastestRateAsync` |
| Carrier labels and cancellations | `client.Shipping.PrepareLabelAsync`, `PurchaseLabelAsync`, `CancelLabelAsync` |
| Carrier address validation | `client.Shipping.ValidateAddressAsync` |
| Carrier pickups | `client.Pickups` |
| USPS scan forms | `client.ScanForms` |
| Landed cost | `client.HsCodes.EstimateLandedCostAsync` |

## Sandbox

Use a `test_...` API key instead of `live_...` to hit the sandbox. Mock carriers respond, no real charges, no real labels — useful for CI, integration tests, and demos. Tracking numbers are prefixed `SBOX...` so you can tell them apart from production.

## Advanced: lower-level HTTP client

`FlexOpsClient` is the underlying HTTP client used by every resource. Use it directly when you need an endpoint that hasn't been wrapped yet, or when you want to call a future endpoint without waiting for a SDK release.

```csharp
using var client = new FlexOpsClient(
    "https://gateway.flexops.io",
    apiKey: "live_...",
    workspaceId: "your-workspace-id");

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
  -H "X-API-Key: test_..." \
  -H "Content-Type: application/json" \
  -d '{
    "origin": {"addressLine1": "123 Main St", "city": "New York", "stateProvince": "NY", "postalCode": "10001", "countryCode": "US"},
    "destination": {"addressLine1": "456 Oak Ave", "city": "Los Angeles", "stateProvince": "CA", "postalCode": "90210", "countryCode": "US"},
    "package": {"weight": 16, "weightUnit": "oz"}
  }'

# Buy a label (hazmatClassification 0 = not hazardous; required when there is no orderId)
curl -X POST https://gateway.flexops.io/api/shipping/labels \
  -H "X-API-Key: test_..." \
  -H "Content-Type: application/json" \
  -d '{
    "carrierCode": "USPS",
    "serviceCode": "PRIORITY",
    "origin":      {"name": "Warehouse", "addressLine1": "123 Main St", "city": "New York",    "stateProvince": "NY", "postalCode": "10001", "countryCode": "US"},
    "destination": {"name": "Customer",  "addressLine1": "456 Oak Ave", "city": "Los Angeles", "stateProvince": "CA", "postalCode": "90210", "countryCode": "US"},
    "package":     {"weight": 16, "weightUnit": "oz"},
    "hazmatClassification": 0
  }'

# Track
curl https://gateway.flexops.io/api/shipping/track/9400111899223456789012 \
  -H "X-API-Key: test_..."
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

Run `dotnet run --project tests/FlexOps.Sdk.ContractCheck --configuration Release` for the repository-owned checks: rates, label approval and replay, HTTP policy, routes and bodies, and the contract test that every SDK method calls a documented Gateway operation.

## Guarded label release compatibility

This release requires a Gateway deployment with the bounded label preview/approval contract (Gateway PR #509 or later). Do not use its live label preparation against an older Gateway: older servers may purchase immediately. Upgrade the Gateway and affected callers together during a purchase maintenance window.

Live label creation now requires a USD maximum, preview, explicit approval, and replay of the saved request with its original idempotency key. Never retry an uncertain purchase with a new key. Sandbox results do not certify production postage.

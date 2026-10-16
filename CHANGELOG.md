# Changelog

All notable changes to the FlexOps .NET SDK are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed (breaking)
- `WebhookSubscription` now describes the Gateway response: `EventTypes` (comma-separated string, or `*` for every event), `Status` (`Active`, `Paused` or `Disabled`), `Description`, `ConsecutiveFailures`, `CreatedAt`, `UpdatedAt`, and `Secret`, set only on create. The `Events` and `IsActive` properties are removed; the Gateway never sent them, so they were always empty and `false`. Code that reads them no longer compiles (flexops-sdk-node#43).
- `WebhooksResource.VerifySignature(payload, signature, secret)` is now `VerifySignature(payload, signature, secret, timestamp, tolerance = null, now = null)`. Pass the `X-FlexOps-Timestamp` header as the new fourth argument. The three-argument overload is removed, so existing calls fail to compile (and code compiled against 2.0.0 gets `MissingMethodException`) instead of silently rejecting every delivery. The old helper never matched a real delivery: it signed the body alone and compared against bare hex, while the Gateway signs `{timestamp}.{raw body}` and sends `sha256=<hex>` (flexops-sdk-node#41).

### Added
- `WebhookEventType` with the events FlexOps sends and matches subscriptions on: `LabelCreated`, `LabelCanceled`, `BatchCompleted`, and, for USPS push-tracking pilot workspaces, `TrackingUpdated` and `TrackingDelivered`.
- `Webhooks.CreateAsync(CreateWebhookRequest)` and `Webhooks.UpdateAsync(webhookId, UpdateWebhookRequest)` send `Events` as the Gateway's comma-separated `eventTypes` (no spaces, duplicates dropped); leave `Events` null on create to receive every event. Both throw `ArgumentException`, without calling the API, for an empty list or an undefined `WebhookEventType` value. The `object` overloads remain and still send their payload as given, so existing calls compile and behave as before; their docs now say to use `eventTypes`, because the Gateway ignores an `events` field and subscribes to every event (flexops-sdk-node#43).
- README: a webhook subscription sample and the table of events FlexOps sends.
- `HazmatClassification` and an optional `HazmatClassification` on `LabelRequest` and `RateShoppingRequest`. It is sent only when set; the SDK never defaults it. Labels without an `OrderId` must declare it (`HazmatClassification.None` for an ordinary parcel).
- `Returns.GenerateLabelAsync(returnId, hazmatClassification)` sends a declared HAZMAT class with a return label.
- `Returns.GenerateLabelAsync(returnId, ReturnLabelRequest)` sends the package weight and size (`WeightOz`, `LengthIn`, `WidthIn`, `HeightIn`) with a return label, plus `CarrierCode`, `ServiceType`, `PaymentConfirmed` and `HazmatClassification`. The Gateway needs the package: a workspace with no default shipping preset gets a 400 (`PackageMeasurementsRequired`) naming the missing fields when the other overloads are used (Gateway #672).

### Fixed
- `Webhooks.CreateAsync` returned `Data == null` against the real Gateway, losing the signing secret: the create route answers `201` with the bare subscription, not the `ApiResponse` envelope. Both overloads now read the bare subscription (and still accept an envelope).
- `WebhooksResource.VerifySignature` verifies the Gateway signature: HMAC-SHA256 over `{X-FlexOps-Timestamp}.{raw body}` keyed with the `whsec_...` secret, accepting `sha256=<hex>` or bare hex, compared in constant time. It returns `false` for missing or non-numeric timestamps and timestamps more than `tolerance` (default `DefaultSignatureTolerance`, five minutes) from now; `now` overrides the clock for tests. An empty secret throws `ArgumentException` (flexops-sdk-node#41).
- README: added a webhook verification sample that reads the event name from `X-FlexOps-Event`; the body is the event data with no envelope (flexops-sdk-node#41).
- `Returns.GenerateLabelAsync(returnId)` sends `{}` instead of a JSON `null` body, which the Gateway return-label route (`[FromBody] GenerateReturnLabelRequest`) refused with 400.
- `Shipping.ValidateAddressAsync(ShippingAddress)` sends the Gateway `{ "address": { ... } }` request. The untyped overload still sends its payload as given.
- README curl quickstart: the label example uses the Gateway `LabelRequest` shape (`carrierCode`, `serviceCode`, `origin`, `destination`, `package`) and the `api/shipping/labels` and `api/shipping/track` routes.

### Changed
- README: added a curl quickstart section so developers can verify the FlexOps API before committing to the SDK.
- Retargeted `RepositoryUrl` in the csproj to `github.com/BillEisenman/flexops-sdk-dotnet` (organization migration from `FlexOps/`).

## [1.0.0] - 2026-03-31

### Added
- Initial public release.
- `FlexOpsClient` thin HTTP wrapper with `GetAsync<T>` / `PostAsync<T>` / `PutAsync<T>` / `DeleteAsync` and workspace-path helpers.
- `FlexOpsTypedClient` convenience layer for strongly-typed common operations.
- API key and JWT authentication.
- Custom `HttpClient` injection for testing.
- Typed response models for `ApiResponse<T>`, `PaginatedResponse<T>`, `ShippingRate`, `Label`, `TrackingInfo`, `TrackingEvent`, `Workspace`, `WalletBalance`, and `WebhookSubscription`.
- Targets .NET 10.0.

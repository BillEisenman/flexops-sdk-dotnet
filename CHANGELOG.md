# Changelog

All notable changes to the FlexOps .NET SDK are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [3.0.0] - 2026-10-19

### Removed (breaking)
The SDK now covers only the documented public Gateway API. Portal features and calls with no Gateway route are gone; code that uses them no longer compiles.
- `client.ApiKeys` (`ApiKeysResource`): API keys are managed in the portal by a signed-in user.
- `client.Offsets` (`OffsetResource`): its per-label routes never existed. Use the new `client.CarbonOffsets`.
- `Wallet.AddFundsAsync`, `Wallet.ListTransactionsAsync` and `Wallet.ConfigureAutoReloadAsync`.
- `Auth.RegisterAsync`, `GetProfileAsync`, `UpdateProfileAsync`, `ChangePasswordAsync`, `ForgotPasswordAsync`, `ResetPasswordAsync` and `VerifyEmailAsync`. `LoginAsync`, `RefreshTokenAsync` and `LogoutAsync` remain for operations that need a signed-in user.
- `Workspaces.CreateAsync` and `Workspaces.UpdateAsync`.
- `Returns.ApproveAsync`, `RejectAsync`, `CancelAsync` and `ProcessRefundAsync`.
- `Webhooks.GetAsync`.
- `Orders.CreateAsync` and `Orders.CancelAsync`, and `Inventory.PostAsnReceiptAsync` and `Inventory.UpdateInventoryAsync`. They wrote through the legacy `api/ApiProxy` route, which the Gateway refuses for API keys. The read methods remain.
- `Shipping.ValidateAddressAsync(object)`. It sent a `{ "address": ... }` payload to an undocumented route; use the `AddressValidationRequest` or `ShippingAddress` overload.
- Carrier proxy label methods, whose routes the Gateway closes with 410 Gone: USPS `CreateReturnLabelAsync` and `CreateInternationalLabelAsync`, UPS `CreateLabelAsync`, FedEx `CreateShipmentAsync` and `CreateReturnShipmentAsync`, and DHL `CreateShipmentAsync`. Buy labels with `client.Shipping` (`PrepareLabelAsync` and `PurchaseLabelAsync`).
- DHL `UpdatePickupAsync`: it sent a PUT to the PATCH-only VisionSuite route `patchDhlUpdatePickup`, and the Gateway proxy has no PATCH route.
- `WalletBalance.Currency` and `WalletBalance.AutoReloadEnabled`, and `Workspace.IsActive`: the Gateway never sent them.

### Changed (breaking)
- Every POST and PUT now sends an `Idempotency-Key` header: the caller's key where a method takes one, otherwise a new GUID for that call. A retrying `HttpClient` handler (Polly, `Microsoft.Extensions.Http.Resilience`) resends the same request and key, so the Gateway replays a write it already finished instead of charging twice. The SDK still has no retry loop; the README lists which statuses are safe to retry for each verb (never a write after a 500, and a DELETE only after a 429). A key reused with a different request gets 422 `IDEMPOTENCY_KEY_REUSED` (Gateway #736), which surfaces as `FlexOpsException` with that `ErrorCode` and must not be retried; only 409 `IDEMPOTENCY_CONFLICT` (exact, uppercase) means the first request is still running.
- Errors are consistent for every verb: a non-success response throws `FlexOpsException` with `StatusCode`, the Gateway `ErrorCode` (`errorCode` or `code`), `Message` and `Errors`; 401 and 403 throw `FlexOpsAuthException` and 429 throws `FlexOpsRateLimitException` (with `RetryAfter`). GET, PUT, DELETE and POST without a key used to throw `HttpRequestException` and lose the error code.
- A success with no body (204 No Content, or an empty body) now returns `null` instead of throwing `JsonException`. This fixes every DELETE that answers 204 (rules, members, webhooks).
- `Wallet.GetBalanceAsync` calls `GET api/wallet/my-balance` with `X-Workspace-Id` and returns the bare `WalletBalance` (`HasWallet`, `BalanceCents`, `Balance`), not `ApiResponse<WalletBalance>`. The old workspace route did not exist.
- `HsCodes` calls `api/shipping/hs-codes/search`, `api/shipping/hs-codes/{code}` and `api/shipping/landed-cost` without the workspace in the path, and returns the bare `List<HsCodeResult>`, `HsCodeResult` and `LandedCostEstimate`. `EstimateLandedCostAsync` takes a `LandedCostRequest`.
- `Shipping.ValidateAddressAsync` calls the documented `POST api/addresses/validate` with the flat Gateway address (`name`, `line1`, `line2`, `city`, `state`, `postalCode`, `country`) and returns `AddressValidationResult`. The `ShippingAddress` overload maps to that body.
- `Insurance.GetQuoteAsync(InsuranceQuoteRequest)`, `PurchaseAsync(PurchaseInsuranceRequest, idempotencyKey)` (now `POST insurance/policies`) and `FileClaimAsync(FileInsuranceClaimRequest)` (now `POST insurance/claims` with `policyId` in the body) take typed requests.
- `Returns.CreateAsync(CreateRmaRequest)` posts to `returns/rma`. `Returns.MarkReceivedAsync(long rmaId, ReceiveRmaRequest)` sends `rmaNumber`, `warehouseId` and `inspectedItems` with a numeric `ReturnDisposition`. `Returns.ListAsync` filters by `startDate`, `endDate`, `page` and `pageSize`.
- `Rules.CreateAsync(CreateShippingRuleRequest)`, `UpdateAsync(ruleId, UpdateShippingRuleRequest)` and `ReorderAsync(IEnumerable<RulePriority>)`, which now sends `POST shipping-rules/reorder` with `{ rules: [{ ruleId, priority }] }` instead of `PUT { ruleIds }`.
- `Pickups.ScheduleAsync(SchedulePickupRequest)`. `Pickups.ListAsync` calls `GET pickups/scheduled`; `GetAsync` and `CancelAsync` take an optional `carrierCode` (default `usps`).
- `Workspaces.InviteMemberAsync(email, WorkspaceRole)` and `UpdateMemberRoleAsync(userId, WorkspaceRole)` send the role as the Gateway number (`{ email, role }`, `{ newRole }`); they took a role name, which the Gateway did not accept. `ListMembersAsync` returns `ApiResponse<WorkspaceMemberList>` (members, pending invitations, limits) instead of an array that failed to deserialize. `GetAsync` returns `ApiResponse<WorkspaceDetail>`; `Workspace` now has `MyRole`, `MemberCount`, `IsCreator` and `ProductType`.
- `Auth.LoginAsync` and `Auth.RefreshTokenAsync` return `ApiResponse<LoginResponse>` and make the client send the returned `token` as its Bearer token. `RefreshTokenAsync(refreshToken, accessToken = null)` sends the `{ token, refreshToken }` body the Gateway requires; the access token defaults to the client's current one, and the call throws `InvalidOperationException` without sending when there is none.
- `client.RecurringShipments`, `client.ReportSchedules` and `client.EmailTemplates` call the real Gateway routes (`recurring-shipments`, `report-schedules`, `email-templates`); the 2.x paths and shapes never worked. `client.Reports` is renamed `client.ReportSchedules` (`ReportSchedulesResource`). Email templates are keyed by tracking event: `UpsertAsync` (PUT) replaces the old create and update, and `PreviewAsync` renders sample data. Requests and responses are typed (`RecurringShipment`, `ReportSchedule`, `EmailTemplate`) with numeric `RecurrenceInterval`, `ReportFormat`, `ReportFrequency` and `TrackingEmailEvent`. Recurring shipments and email templates answer with bare JSON; report schedules use the envelope. Writes need a user token or an API key with the `recurring-shipments:write`, `report-schedules:write` or `email-templates:write` scope (otherwise 403 `API_KEY_SCOPE_REQUIRED`). `RecurringShipments.TriggerAsync` takes an optional idempotency key.
- The carrier writes through the legacy `api/ApiProxy` route (USPS, UPS, FedEx and DHL POST, PUT and DELETE methods) keep their 2.x names and signatures but are `[Obsolete]`: they need a user token (API keys get 403 `API_KEY_SCOPE_REQUIRED`) and stop working when the proxy is retired on 2027-06-30. They now send an `Idempotency-Key` like every POST and PUT. Use `client.Shipping`, `client.Pickups` and `client.ScanForms` where they cover the operation.
- The remaining carrier lookups are GET requests with their parameters in the query string, taking `IReadOnlyDictionary<string, string>` instead of an `object` body: USPS `ValidateAddressAsync`, `ZipCodeLookupAsync`, `TrackSummaryAsync`, `TrackDetailAsync`, `GetDeliveryStandardsAsync`, `FindDropOffLocationsAsync`, `FindPostOfficesAsync` and `CityStateLookupAsync(string zipCode)`; UPS `TrackAsync`; DHL `ValidateAddressAsync`, `GetRatesAsync`, `GetProductsAsync`, `TrackAsync`, `TrackMultipleAsync`, `GetProofOfDeliveryAsync`, `GetReferenceDataAsync` and `FindServicePointsAsync`.
- `WebhookSubscription` now describes the Gateway response: `EventTypes` (comma-separated string, or `*` for every event), `Status` (`Active`, `Paused` or `Disabled`), `Description`, `ConsecutiveFailures`, `CreatedAt`, `UpdatedAt`, and `Secret`, set only on create. The `Events` and `IsActive` properties are removed; the Gateway never sent them, so they were always empty and `false`. Code that reads them no longer compiles (flexops-sdk-node#43).
- `WebhooksResource.VerifySignature(payload, signature, secret)` is now `VerifySignature(payload, signature, secret, timestamp, tolerance = null, now = null)`. Pass the `X-FlexOps-Timestamp` header as the new fourth argument. The three-argument overload is removed, so existing calls fail to compile (and code compiled against 2.0.0 gets `MissingMethodException`) instead of silently rejecting every delivery. The old helper never matched a real delivery: it signed the body alone and compared against bare hex, while the Gateway signs `{timestamp}.{raw body}` and sends `sha256=<hex>` (flexops-sdk-node#41).

### Added
- `client.CarbonOffsets` (`CarbonOffsetsResource`): `QuoteAsync`, `PurchaseAsync` (with an optional idempotency key), `ListAsync(page, pageSize)`, `GetAsync`, `GetSummaryAsync`, `GetSettingsAsync` and `UpdateSettingsAsync`.
- `Rules.TestAsync(TestShippingRuleRequest)` dry-runs the workspace's rules against a hypothetical shipment.
- `Webhooks.TestAsync(webhookId)` sends a test delivery.
- `ReturnLabelRequest.RateIndicator` for USPS flat-rate returns, and `CustomsItem.SkuOrProductId`.
- `FlexOpsClient.PutAsync(path, body, ct, idempotencyKey)` and `FlexOpsClient.GetAsync(path, headers, ct)`.
- A contract check (`tests/FlexOps.Sdk.ContractCheck`) that calls every public resource method and fails when one calls an operation missing from the documented Gateway OpenAPI, writes through `api/ApiProxy` from a method that is not `[Obsolete]`, or calls a closed proxy label route.
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

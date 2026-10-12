# Changelog

All notable changes to the FlexOps .NET SDK are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- `HazmatClassification` and an optional `HazmatClassification` on `LabelRequest` and `RateShoppingRequest`. It is sent only when set; the SDK never defaults it. Labels without an `OrderId` must declare it (`HazmatClassification.None` for an ordinary parcel).
- `Returns.GenerateLabelAsync(returnId, hazmatClassification)` sends a declared HAZMAT class with a return label.

### Fixed
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

# Changelog

All notable changes to the FlexOps .NET SDK are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

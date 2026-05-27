# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

`FlexOps.Sdk` is the **official hand-crafted .NET SDK** for the FlexOps Platform. It targets the FlexOps **Gateway BFF** — the public ingress for shipping, label, tracking, address validation, and account operations. Published to NuGet as `FlexOps.Sdk`; root namespace `FlexOps.Sdk`; target framework .NET 10.

> **Gateway-targeted, not VSCS-targeted.** All hand-crafted SDKs in this family (.NET, Node, Python, Go, PHP, Ruby) hit Gateway. The Java SDK is the lone exception — it was auto-generated against VisionSuiteCoreServices and is archived as of 2026-03-08.

## Build & Run Commands

```powershell
dotnet restore                                                          # Restore packages
dotnet build                                                            # Build
dotnet build --configuration Release -p:TreatWarningsAsErrors=true      # CI build
dotnet test                                                             # Run tests
dotnet pack --configuration Release                                     # Produce NuGet package
```

## Architecture

```text
Customer .NET app  →  FlexOps.Sdk (this repo)  →  Gateway BFF (gateway.flexops.io)
                                                  ↓
                                                  VSCS / Integrations / etc.
```

## Key files

| Path | Purpose |
|------|---------|
| `FlexOpsClient.cs` | Primary client (raw HTTP surface) |
| `FlexOpsTypedClient.cs` | Strongly-typed convenience surface |
| `Models.cs` | Request/response DTOs |
| `Errors.cs` | Exception hierarchy + error envelope mapping |
| `Resources/` | Static resources |
| `scripts/` | Build/publish helpers |
| `CHANGELOG.md` | Per-release notes |

## Conventions

- **.NET 10** / nullable enabled / XML doc comments on public members (XML docs ship with the NuGet package — `GenerateDocumentationFile=true`).
- Strongly-typed errors via `Errors.cs` — never throw raw `HttpRequestException` to the caller; wrap in a FlexOps-typed exception that carries the Gateway error envelope.
- The two-client design (raw + typed) is intentional — the raw `FlexOpsClient` is the escape hatch for endpoints the typed surface hasn't covered yet.

## Publish

GitHub Actions handles NuGet publishing. Bump `Version` in the `.csproj`, tag, push — the workflow does `dotnet pack` and `dotnet nuget push`.

## Related Repositories

| Repository | Purpose |
|---|---|
| **This repo** | `FlexOps.Sdk` on NuGet |
| FlexOps Gateway | The HTTP API this SDK calls — `BillEisenman/FlexOpsGateway` |
| Sibling SDKs | `@flexops/sdk` (Node), `flexops` (Python/Ruby), `flexops/sdk` (PHP), `flexops-sdk-go` (Go), `@flexops/elements` (React widgets), `@flexops/cli` (terminal) |
| FlexOps Developer Docs | Hosts the SDK page — `BillEisenman/FlexOpsDeveloperDocs` |

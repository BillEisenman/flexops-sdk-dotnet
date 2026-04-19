#!/usr/bin/env bash
# Regenerate the C# client from the unified OpenAPI spec.
# Run from the FlexOps.Sdk project directory.
set -euo pipefail

echo "Building project to trigger NSwag code generation..."
dotnet build

echo "Done. Generated client is at Generated/FlexOpsApiClient.g.cs"

Repository: CLWebStore.Catalog

Purpose
-------
This repository contains the Catalog solution for CLWebStore. Projects included: API, Application, Domain, Infrastructure, OutboxProcessor, ReadModelProjector, and unit tests. Use these instructions to build, run, test, and contribute.

Projects
--------
- src/CLWebStore.Catalog.API — web API (primary entrypoint)
- src/CLWebStore.Catalog.Application — application layer / use cases
- src/CLWebStore.Catalog.Domain — domain models and logic
- src/CLWebStore.Catalog.Infrastructure — external integrations and persistence
- src/CLWebStore.Catalog.OutboxProcessor — background/outbox processor
- src/CLWebStore.Catalog.ReadModelProjector — read model projection function app
- tests/CLWebStore.Catalog.UnitTests — unit tests for the entire solution

Prerequisites
-------------
- .NET SDKs as required by the projects (solution contains projects targeting .NET 10 and .NET 8). Install matching SDKs from https://dotnet.microsoft.com.
- Visual Studio 2026 or newer recommended for IDE work; PowerShell is the preferred terminal.

Common commands
---------------
Run from repository root.

- Restore and build solution:
  dotnet restore
  dotnet build CLWebStore.Catalog.slnx --configuration Release

- Run the API locally (development):
  dotnet run --project src/CLWebStore.Catalog.API --configuration Debug

- Run the OutboxProcessor locally:
  dotnet run --project src/CLWebStore.Catalog.OutboxProcessor --configuration Debug

- Run unit tests:
  dotnet test tests/CLWebStore.Catalog.UnitTests --no-build --verbosity normal

- Format code:
  dotnet tool run dotnet-format --folder

Configuration & environment
---------------------------
- Check appsettings*.json files in the API and Infrastructure projects for configuration keys. Local secrets (connection strings, keys) should not be committed. Use user secrets, environment variables, or a local development secrets store.
- When running locally, ensure any dependent services (databases, message brokers) referenced in configuration are available or mocked.

Development workflow
--------------------
- Create feature branches from main (or repo default): feature/<ticket>-short-description
- Keep commits small and focused; use descriptive commit messages.
- Run tests and format locally before opening a pull request.

Unit Testing Standards & Code Generation
----------------------------------------
- Testing Frameworks: xUnit and Moq.
- Target Test Project: `tests/CLWebStore.Catalog.UnitTests`.
- Mirror Structure: Always mirror the target project and feature folder path under `CLWebStore.Catalog.UnitTests` (e.g., `CLWebStore.Catalog.Application/Commands/V1/CreateProduct/` maps to `CLWebStore.Catalog.UnitTests/Application/Commands/V1/CreateProduct/`).
- Namespace Convention: Match namespaces directly to the folder hierarchy (e.g., `CLWebStore.Catalog.UnitTests.Application.Commands.V1.CreateProduct`).
- Test Naming: Use standard AAA format and descriptive method names: `[MethodName]_[Scenario]_[ExpectedResult]`.
- Test Data Builders: Check `CLWebStore.Catalog.UnitTests.Common.Builders` for existing builder classes (e.g., `ProductBuilder`) before instantiating raw domain aggregates or entities in test setups.
- Mocking Rules:
  - Mock external interface abstractions (`IProductRepository`, `IProductQueryService`, etc.) using Moq.
  - Test both success (happy path) and failure (validation errors, domain exceptions, missing entities) scenarios.

Notes for Copilot / automation
-----------------------------
- Keep code changes within the existing layering (API -> Application -> Domain -> Infrastructure).
- Prefer minimal and conservative changes when fixing failing builds or tests.
- If adding new projects or changing TFMs, update the solution file and CI configuration.

Contact / further details
-------------------------
For repository-specific runtime details (ports, external dependencies, startup args) inspect the API's appsettings.Development.json and the Infrastructure project. Refer to project README files (if present) for deeper operational docs.
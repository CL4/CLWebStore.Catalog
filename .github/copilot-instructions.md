# Repository: CLWebStore.Catalog

## Purpose
-------

This repository contains the Catalog solution for CLWebStore. Projects included: API,
Application, Domain, Infrastructure, OutboxProcessor, ReadModelProjector, integration
tests, unit tests, and supporting tools.

These instructions define the repository's architecture, engineering conventions,
testing standards, scope boundaries, and human-in-the-loop workflow for GitHub Copilot
and other automated development assistance.

Follow these instructions throughout the task unless the developer explicitly overrides
a specific instruction.

## Projects
--------

- `src/CLWebStore.Catalog.API` — web API (primary entrypoint)
- `src/CLWebStore.Catalog.Application` — application layer / use cases
- `src/CLWebStore.Catalog.Domain` — domain models and logic
- `src/CLWebStore.Catalog.Infrastructure` — external integrations and persistence
- `src/CLWebStore.Catalog.OutboxProcessor` — background/outbox processor
- `src/CLWebStore.Catalog.ReadModelProjector` — read model projection function app
- `tests/CLWebStore.Catalog.UnitTests` — unit tests for the solution
- `tests/CLWebStore.Catalog.IntegrationTests` — integration tests for API wiring and real persistence
- `tools/ReadModelSeeder` — utility for seeding the PostgreSQL read model
- `tools/ServiceBusEventReceiver` — utility for receiving Service Bus events

## Prerequisites
-------------

- The solution targets .NET 10. Use .NET 10 APIs, conventions, and tooling unless explicitly instructed otherwise.
- Visual Studio 2026 or newer is recommended for IDE work.
- PowerShell is the preferred terminal.
- Docker Desktop is required for integration tests because Testcontainers manages the required PostgreSQL and Cosmos DB containers.
- Generated projects or assemblies under `obj` are build artifacts and do not define the target framework of the application's projects.

## Common Commands
---------------

Run commands from the repository root.

### Restore solution

```powershell
dotnet restore
```

### Build solution

```powershell
dotnet build CLWebStore.Catalog.slnx --configuration Release
```

### Run the API locally

```powershell
dotnet run --project src/CLWebStore.Catalog.API --configuration Debug
```

### Run the OutboxProcessor locally

```powershell
dotnet run --project src/CLWebStore.Catalog.OutboxProcessor --configuration Debug
```

### Run unit tests

```powershell
dotnet test tests/CLWebStore.Catalog.UnitTests --no-build --verbosity normal
```

### Run integration tests

```powershell
dotnet test tests/CLWebStore.Catalog.IntegrationTests --verbosity normal
```

### Format code

```powershell
dotnet tool run dotnet-format --folder
```

## Configuration & Environment
---------------------------

- Check `appsettings*.json` files in the API and Infrastructure projects for configuration keys.
- Local secrets such as connection strings, API keys, and credentials must not be committed.
- Use user secrets, environment variables, Azure Key Vault, or the appropriate local development secrets store for sensitive configuration.
- When running locally, ensure required dependent services are available or are appropriately substituted for testing.
- Integration tests use Testcontainers for PostgreSQL and Cosmos DB. Do not require manually started database containers for the integration-test suite unless explicitly instructed.
- Integration tests must be able to disable external observability exporters such as New Relic. Tests must not require a live external telemetry service.
- Do not commit test secrets or credentials to the repository.

## Development Workflow
--------------------

- Create feature branches from `main` (or the repository default): `feature/<ticket>-short-description`.
- Keep commits small and focused.
- Use descriptive commit messages.
- Prefer small, incremental changes over large refactors.
- Run appropriate validation after approved changes.
- Before introducing a framework or library workaround, verify whether the current .NET 10 or package version already provides the required behavior.
- Do not preserve legacy compatibility workarounds when the current target framework makes them unnecessary, unless there is a demonstrated project-specific reason.
- Keep each implementation step focused on one logical objective.
- Do not expand the scope of a task without explicit developer approval.

## Unit Testing Standards & Code Generation
----------------------------------------

- Testing framework: xUnit.
- Mocking framework: Moq.
- Target test project: `tests/CLWebStore.Catalog.UnitTests`.
- Mirror Structure: Mirror the target project and feature folder path under the unit test project where practical.
- Namespace Convention: Match namespaces directly to the folder hierarchy.
- Test Naming: Use standard AAA structure and descriptive method names:
  `[MethodName]_[Scenario]_[ExpectedResult]`.
- Test Data Builders: Check
  `CLWebStore.Catalog.UnitTests.Common.Builders` for existing builder classes such as
  `ProductBuilder` before creating raw domain objects in test setup.
- Mocking Rules:
  - Mock external interface abstractions such as `IProductRepository` and `IProductQueryService` with Moq.
  - Test both successful and failure scenarios, including validation failures, domain exceptions, and missing entities.
- Do not use unit-test mocking to bypass the system under test when an integration test is intended.

## Integration Testing Standards
-----------------------------

- Integration tests must exercise the real API host and real persistence components wherever practical.
- Use `WebApplicationFactory<Program>` for hosting the API under test.
- Use Testcontainers for PostgreSQL and Cosmos DB.
- Keep integration-test infrastructure reusable, deterministic, and isolated.
- Tests must not depend on developer-specific local database instances.
- Tests must explicitly reset or clean test data so that one test does not depend on another test's state.
- Do not use unit-test mocks for the application's own persistence abstractions in integration tests.
- Prefer strongly typed test builders for API write requests and realistic seeded data for read/query scenarios.
- Reuse the existing integration-test data and builders where practical instead of duplicating large object graphs in individual tests.
- API write tests should verify API behavior, persistence, and transactional-outbox behavior where relevant.
- API read/query tests should verify the PostgreSQL read model and query behavior.
- Do not assume Azure Functions, background processors, or message consumers are running as part of the API integration-test host unless a specific test explicitly starts and controls them.
- Keep integration tests focused on externally observable behavior rather than re-testing implementation details already covered by unit tests.
- When an integration test exposes a legitimate production-code defect, treat the discovery as valuable diagnostic information. Do not weaken or bypass the test merely to make it pass.

## Copilot Engineering Principles
-----------------------------

- Keep code changes within the existing layering: API -> Application -> Domain -> Infrastructure.
- Preserve the existing DDD, CQRS/MediatR, event-driven, and transactional-outbox architecture.
- Prefer minimal and conservative changes when implementing features or fixing builds, tests, or implementation issues.
- Do not reimplement existing microservice functionality merely to make tests pass.
- Reuse existing abstractions, repositories, query services, domain models, builders, and infrastructure wherever possible.
- Before changing an existing design, inspect the current implementation and determine whether the requested behavior can be achieved with a smaller change.
- Do not make unrelated cleanup, modernization, refactoring, or speculative improvements while working on a requested feature, bug fix, or test.
- Do not change project target frameworks unless explicitly requested or required by a demonstrated compatibility issue.
- If adding new projects or changing project configuration, update the solution and any relevant CI/build configuration.
- Prefer APIs and conventions supported by the current target framework rather than introducing legacy compatibility patterns.
- Do not assume a proposed change is present in the working tree merely because the editor displays it.
- Do not silently broaden the scope of a requested change.
- Prefer one logical change at a time, especially when debugging integration-test infrastructure.
- Never expand the scope of an authorized change merely because an additional change appears necessary, convenient, cleaner, or technically related.
- When a requested or authorized change touches one file but investigation reveals that another file may need modification, inspect and explain the dependency but do not modify the additional file without authorization.
- Do not modify production code solely because doing so would make an integration test easier to write or maintain.
- If an integration test exposes a genuine production defect, preserve the test's ability to expose that defect and propose the smallest appropriate production-code correction separately.

## Human-in-the-Loop Code Change Workflow
---------------------------------------

The developer retains final approval authority over all repository file changes.

Copilot is encouraged to be proactive in inspection, analysis, diagnostics, validation,
and root-cause investigation. However, authorization to modify files is always scoped to
the specific changes and files that the developer has authorized.

### Implementation phase

When asked to implement a change:

1. Inspect the existing implementation, relevant tests, project structure, and supporting
   infrastructure before making changes.

2. Determine the smallest change required to accomplish the requested objective.

3. Make only the requested changes and necessary supporting changes that are within the
   current authorized scope.

4. Do not perform unrelated cleanup, modernization, refactoring, speculative improvements,
   or future planned work.

5. When the implementation is complete, STOP and wait for the developer to review the
   proposed changes and click `Keep` or otherwise explicitly accept them.

6. Do not validate or make additional dependent changes while the current changes are
   still pending.

### Scope of authorization

Authorization to implement a change is limited to the specific files and changes that
were requested or explicitly approved.

For example:

- If the developer authorizes a change to `FileA.cs`, that authorization applies to
  `FileA.cs` only.
- If the developer authorizes changes to `FileA.cs` and `FileB.cs`, both files are
  authorized.
- Discovering that another file, `FileC.cs`, may also need to change does not implicitly
  authorize modification of `FileC.cs`.

A developer instruction such as:

- "proceed";
- "go ahead";
- "apply the fix";
- "please proceed with the proposed fix";

means: implement the specific change that was just proposed and authorized.

It does NOT authorize additional changes discovered during implementation.

If implementing an authorized change reveals that another file or additional change is
required:

1. Do not modify the newly discovered file or apply the additional change.
2. Investigate enough to determine why the additional change appears necessary.
3. Explain the dependency and the proposed additional change.
4. Identify the exact file(s) that would need to be modified.
5. STOP and wait for explicit developer authorization.

The same rule applies to production code, tests, configuration, project files, dependencies,
and architectural changes.

### After developer acceptance

Once the developer explicitly accepts/keeps the current changes:

1. Treat only those accepted changes as approved.
2. Approval applies only to that specific change batch.
3. Approval does NOT constitute standing authorization to modify the affected files again.
4. Proactively perform the appropriate validation for the accepted change.
5. Validation may include building, running relevant tests, analyzers, or other checks
   appropriate to the approved changes.
6. Do not require another prompt merely to begin normal validation of accepted changes.
7. Do not make additional code changes merely because validation is being performed.
8. Any new modification discovered during validation is a NEW change batch and requires
   explicit developer authorization, even if the modification affects a file that was
   already approved and accepted.

### Change Batches and Re-Approval

Developer authorization applies to a specific change batch, not permanently to a file,
feature, task, or implementation area.

A subsequent modification is considered a NEW change batch even when:

- it modifies the same file;
- it is part of the same overall task;
- it is required to make the previous change pass validation;
- it was discovered during validation of the previously accepted change;
- it appears to be an obvious, trivial, or mechanical correction;
- it was proposed by Copilot as part of its investigation.

If validation or investigation reveals that another modification is required:

1. Investigate the failure and determine the likely root cause.
2. Identify the exact corrective change and the file(s) that would be affected.
3. Report the validation failure and investigation findings.
4. Explain the proposed corrective change.
5. STOP and wait for explicit developer authorization.
6. After authorization, implement ONLY the newly authorized corrective change.

Authorization to implement a corrective change does NOT authorize validation of that
newly implemented change.

After implementing an authorized corrective change, STOP immediately.

The developer must have an opportunity to review and explicitly accept the new change with
`Keep` before validation resumes.

Do not build, test, analyze, or otherwise validate the newly implemented corrective change
until the developer has accepted it with `Keep`.

This rule applies even when:
- the corrective change is exactly the change previously proposed;
- the developer's authorization was explicit;
- the corrective change is trivial or mechanical;
- the corrective change affects only one line;
- the original validation failure would obviously be resolved by the change.

"Authorized to implement" and "accepted with Keep" are two different workflow states.
Never treat them as equivalent.

7. After the developer clicks `Keep`, proactively validate the newly accepted change.

Do not modify any file as part of the corrective change before receiving explicit
authorization for that corrective change.

Most importantly:

> Authorization to modify a file once does NOT authorize future modifications to that
> same file.

Every distinct modification requires its own authorization boundary.

### Successful validation

If validation succeeds:

- Report what was validated and the observed result.
- Do not make additional changes unless they are explicitly authorized or are part of
  the already approved implementation.
- STOP and wait for the next developer instruction.

### Failed validation

If validation fails:

1. Do not immediately modify code.
2. Proactively investigate the failure and determine the likely root cause.
3. Investigation may include inspecting:
   - test output;
   - build output;
   - source code;
   - configuration;
   - logs;
   - dependencies;
   - infrastructure;
   - related implementation details.
4. Determine whether the failure is caused by:
   - the recently accepted change;
   - an existing defect;
   - a test defect;
   - configuration;
   - environment or infrastructure;
   - framework/tooling behavior;
   - another dependency.
5. Identify the smallest reasonable corrective action.
6. Report:
   - the validation failure;
   - investigation findings;
   - likely root cause;
   - proposed corrective change;
   - exact files or areas that would need to change;
   - any relevant risks or alternatives, if applicable.
7. Do not modify any files as part of the proposed corrective action.
8. Do not retry validation merely to obtain a different result.
9. STOP and wait for explicit developer authorization.

### Newly discovered corrective changes

A corrective change discovered during implementation or validation is a NEW change
and requires its own authorization if it was not already included in the current
authorized scope.

For example:

    Approved change to FileA.cs
            ↓
    Developer clicks Keep
            ↓
    Validation
            ↓
    Failure discovered
            ↓
    Investigation
            ↓
    Root cause requires FileB.cs
            ↓
    Propose FileB.cs change
            ↓
    STOP
            ↓
    Developer authorizes FileB.cs change
            ↓
    Modify FileB.cs
            ↓
    STOP
            ↓
    Developer clicks Keep
            ↓
    Validate again

Do not skip the authorization boundary simply because the newly discovered change is
necessary to make the previous change work.

If validation exposes a legitimate production-code defect, the defect should be reported
and the corrective production-code change should be proposed explicitly. Do not silently
fold the production change into a previously authorized test change.

### Corrective change authorization

When the developer authorizes a proposed corrective change:

1. Implement only the specific corrective change that was authorized.
2. Do not expand the change to additional files or related improvements.
3. If another file or additional change becomes necessary, stop and request authorization
   for that additional change.
4. After implementing the authorized correction, STOP and wait for the developer to review
   and click `Keep`.
5. After the developer accepts the correction, proactively validate it.
6. If validation succeeds, report the result and stop.
7. If validation fails again, repeat the failure-investigation-and-authorization process.

### Discarded changes

If the developer discards a proposed change:

- Reassess the task using the actual current working-tree state.
- Do not assume discarded changes remain present.
- Do not continue from a discarded implementation.
- Determine the next action from the current repository state and the developer's
  instructions.

### Approval boundary

The following require explicit developer authorization before modification when they are
outside the current authorized change:

- production source files;
- test files;
- configuration files;
- project files;
- package dependencies;
- architectural components;
- infrastructure;
- generated or manually maintained configuration;
- any additional file discovered during implementation or validation.

Inspection, analysis, diagnostics, root-cause investigation, and validation do not require
separate approval when they do not modify repository files.

## Validation Discipline
---------------------

- Never claim that a build, test, analyzer, formatter, or other validation succeeded unless
  it was actually executed and the result was observed.
- Validate the smallest relevant scope first when practical.
- Do not automatically run the entire solution when the requested change only requires
  validation of a specific project or test group.
- Do not retry failed validation merely to obtain a different result.
- Do not apply speculative fixes.
- Do not silently modify files discovered during validation.
- When a failure reveals a new required change, treat that change as a new proposed change
  requiring developer authorization.
- When reporting a root cause, distinguish established facts from hypotheses or uncertainty.
- Prefer one logical corrective change at a time.
- If a validation failure is caused by the environment or infrastructure rather than code,
  report that distinction clearly and do not modify application code to compensate for it.
- Do not weaken an integration test, remove an assertion, or alter expected behavior merely
  to make a failing test pass unless the existing requirement or contract has been shown
  to be incorrect.

## Repository-Specific Architecture
--------------------------------

- The Catalog bounded context follows Domain-Driven Design, CQRS/MediatR, and an event-driven architecture.
- Cosmos DB is the write-side/source-of-truth persistence store.
- PostgreSQL is used as the read model and is queried with Dapper.
- Product writes and their corresponding `OutboxEvent` documents use the transactional outbox pattern and are persisted atomically in the same Cosmos DB container.
- The Product document and `OutboxEvent` document use the existing document type discriminator and partition-key design. Preserve that design.
- Do not introduce direct synchronous API-to-API calls between microservices when an event-driven interaction is intended.
- Azure Functions and other background processing components must continue to respect the existing event-driven architecture and transactional-outbox design.
- Preserve the separation between write-side domain/application logic and read-side query logic.
- Do not move read-model responsibilities into the write-side domain model merely to simplify a test.
- Do not bypass the transactional-outbox pattern when implementing or testing product write behavior unless the test specifically targets a different concern.

## Framework and Dependency Discipline
------------------------------------

- Target .NET 10 throughout the solution.
- Before adding a package, determine whether an existing dependency already provides the required capability.
- Prefer existing package versions and project conventions unless there is a concrete reason to change them.
- Avoid introducing deprecated APIs or patterns when a supported .NET 10 alternative exists.
- When framework behavior is uncertain, inspect the current implementation and verify the behavior against the project's actual target framework before adding a workaround.
- Do not change dependencies solely to work around an issue until the existing dependency and framework behavior have been investigated.

## Scope Control
------------

- Do exactly what the developer asks.
- Do not proactively implement future planned steps.
- Do not combine multiple planned steps into a single implementation unless explicitly requested.
- Do not modify unrelated files merely because an improvement is noticed.
- If an unrelated issue is discovered, mention it separately rather than fixing it automatically.
- If the requested task cannot be completed without an additional change outside the stated scope, explain the dependency and ask for approval before making that change.
- A genuine blocker may be investigated, but the resulting fix still requires explicit developer authorization before implementation.
- A production-code defect discovered by an integration test is not automatically part of the integration-test task. Report it and propose the production fix separately.
- Do not close or declare a task complete merely because a test can be made to pass. The resulting implementation must still respect the requested scope and architecture.

## Repository Context and Deferred Issues
--------------------------------------

The repository may contain known warnings, generated artifacts, compatibility concerns, or technical-debt items that are intentionally deferred.

Examples include:

- generated Azure Functions `WorkerExtensions` artifacts under `obj`;
- non-blocking HTTPS redirection warnings in the integration-test host;
- redundant test-host configuration;
- nullable or xUnit warnings;
- Azure Functions/.NET 10 compatibility investigation;
- other cleanup or hardening opportunities not required for the current task.

Do not address deferred issues unless:

1. the developer explicitly requests them; or
2. they are demonstrated to be a genuine blocker for the current task.

Do not turn incidental observations into scope expansion.

## Repository-Specific Integration Test Guidance
---------------------------------------------

- Integration tests use Testcontainers for PostgreSQL and Cosmos DB.
- The shared integration-test infrastructure should be reused rather than creating duplicate container/factory implementations.
- Use the existing shared xUnit integration-test collection and `CatalogWebApplicationFactory`.
- Use the existing database reset/isolation infrastructure.
- Use the existing strongly typed product fixture loader and PostgreSQL test-data seeder.
- Use the existing product request builders for API write requests.
- Read/query integration tests should seed PostgreSQL with the required read-model data.
- Do not seed Cosmos DB for read/query tests unless the implementation under test genuinely requires Cosmos DB.
- Write integration tests should use the real API and inspect real persistence when persistence behavior is part of the test.
- When transactional-outbox behavior is relevant, verify the resulting Cosmos documents rather than mocking the persistence layer.
- Do not introduce generic test frameworks, custom assertion libraries, snapshot testing, performance testing, CI/CD work, Azure deployment testing, or other infrastructure unrelated to the requested integration-test scope.
- Keep integration-test additions incremental and grouped by endpoint or behavior.
- When a test exposes a production defect, preserve the test and separately evaluate the production correction rather than weakening the test.

## Contact / Further Details
-------------------------

For repository-specific runtime details such as ports, external dependencies, startup arguments,
and local configuration, inspect the API's `appsettings.Development.json`, the Infrastructure
project, and applicable project README files.

When a repository detail is uncertain, inspect the current repository rather than relying on
assumptions from previous conversations or generated context.
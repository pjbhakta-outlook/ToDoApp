# Copilot Instructions for ToDo

## Build, run, test

All commands from the repo root (where `ToDo.sln` lives).

```bash
# Build
dotnet build ToDo.sln

# Run the full app (Redis + SQL Server + API + Web + Aspire dashboard)
dotnet run --project ToDo.AppHost

# Run all tests
dotnet test ToDo.Tests/ToDo.Tests.csproj

# Run a single test class or method (substring match with ~)
dotnet test ToDo.Tests/ToDo.Tests.csproj --filter "FullyQualifiedName~WebTests"
```

Do not run `ToDo.ApiService` or `ToDo.Web` directly — always go through the AppHost so service discovery, Redis, and SQL Server are wired up.

## Architecture

This is a **.NET 10 Aspire** distributed application with five projects:

- **`ToDo.AppHost`** — Aspire orchestrator. Declares infrastructure (Redis `"cache"`, SQL Server `"sqlserver"` → database `"tododb"`) and service projects. Resource names here are the service-discovery identifiers used elsewhere.
- **`ToDo.ApiService`** — Minimal API backend using **vertical slice architecture**. Uses EF Core with SQL Server (`TodoDbContext`) registered via `builder.AddSqlServerDbContext<TodoDbContext>("tododb")`. Each endpoint is a self-contained feature slice in its own file under `Features/`. The DB is auto-created on startup with seed data via `EnsureCreated()` + `HasData()`.
- **`ToDo.Web`** — Blazor Server frontend (interactive server render mode). Communicates with the API through a typed `HttpClient` (`TodoApiClient`) whose base address is `https+http://apiservice` (Aspire service discovery, not DNS).
- **`ToDo.ServiceDefaults`** — Shared setup referenced by all services. Configures OpenTelemetry, standard HTTP resilience, service discovery on all `HttpClient`s, and `/health` + `/alive` endpoints.
- **`ToDo.Tests`** — Integration tests using `Aspire.Hosting.Testing` + xUnit v3. Boots the full topology via `DistributedApplicationTestingBuilder.CreateAsync<Projects.ToDo_AppHost>`.

## Key conventions

**Vertical slice architecture (API)** — the `ToDo.ApiService` project is organized by feature, not by technical layer. Follow these practices:

- **Folder structure**: `Data/` for shared entities and DbContext; `Features/<Domain>/` for endpoint slices (e.g., `Features/Todos/`).
- **One file per endpoint**: Each operation (Create, Get, Delete, Toggle, etc.) lives in its own file containing:
  - A `static class` named after the operation (e.g., `CreateTodo`)
  - A `public static void MapEndpoint(WebApplication app)` method that registers the route
  - Any request/response records specific to that endpoint (co-located, not in a shared `Models/` folder)
- **Program.cs is a thin composition root**: It only handles DI registration, middleware, and calling each feature's `MapEndpoint`. No business logic or inline endpoint lambdas belong here.
- **Adding a new endpoint**: Create a new file under the appropriate `Features/<Domain>/` folder, define the class + `MapEndpoint`, then call it from `Program.cs`. Do not add logic to `Program.cs`.
- **Keep slices independent**: Each slice should be self-contained. Avoid cross-slice dependencies. Shared concerns (entities, DbContext, common utilities) live in `Data/` or a `Shared/` folder.
- **Route ordering**: Register literal-segment endpoints (e.g., `/count`) before `{id}` parameterized routes to avoid ambiguity.
- **No shared DTOs folder**: Request/response records belong in the feature file that uses them. Only promote to a shared location if genuinely reused across multiple slices.
- **OpenAPI metadata**: Every endpoint must include `.WithName()`, `.WithDescription()`, and `.Produces<T>()` / `.ProducesValidationProblem()` annotations so the generated OpenAPI spec is complete and self-documenting.

**API routes** follow the pattern `/api/todos`, `/api/todos/{id}`, `/api/todos/{id}/toggle`, `/api/todos/count`. Place literal-segment endpoints (like `/count`) before `{id}` parameterized routes to avoid ambiguity.

**Inter-service HTTP** — always use `https+http://<resourceName>` as the base address. Resilience and discovery are automatic from `ServiceDefaults`.

**Adding a new Aspire resource**: register it in `AppHost.cs`, wire consumers with `WithReference()`/`WaitFor()`. For a new service project, also add `builder.AddServiceDefaults()` + `app.MapDefaultEndpoints()` and reference `ToDo.ServiceDefaults`.

**EF Core / database** — the `TodoDbContext` uses `HasData()` for seed data. Schema is applied with `EnsureCreated()` (not migrations). If migrations are added later, remove the `EnsureCreated()` call.

**Tests** use xUnit v3 (`xunit.v3`), not v2. Cancellation tokens come from `TestContext.Current.CancellationToken`. Integration tests resolve HTTP clients with `app.CreateHttpClient("<resourceName>")` and gate on `WaitForResourceHealthyAsync`.

**Every API endpoint must have tests.** When adding a new endpoint, also add corresponding integration tests in `ToDo.Tests/ApiTests.cs` covering:
- Happy-path (correct status code + response body)
- Edge cases (e.g., non-existent ID returns 404)
- No endpoint should be merged without test coverage.

**AAA pattern** — every test method must be structured with `// Arrange`, `// Act`, and `// Assert` comments clearly separating each phase.

**C# style**: top-level `Program.cs` (no `Main` method), file-scoped namespaces, nullable enabled, primary constructors, collection expressions (`[]`), `required` properties where appropriate. Target framework is `net10.0` everywhere.

## Tooling

- `.vscode/mcp.json` registers the **Aspire MCP server** (`aspire agent mcp`) for AppHost and resource operations.
- `.agents/skills/` has project-scoped skills: `aspire`, `aspireify`, `dotnet-inspect`, `playwright-cli`.

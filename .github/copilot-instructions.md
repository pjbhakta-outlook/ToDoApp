# Copilot Instructions for ToDo

## Agents

- **`api-creator`** — Use the `@api-creator` agent for any API-related work: creating endpoints, modifying endpoint logic, adding new entities/domains to the API, or writing API integration tests. The agent uses the `api-creator` skill which encodes all vertical slice conventions, testing patterns, and route ordering rules.
- **`frontend-dev`** — Use the `@frontend-dev` agent for any frontend work: creating pages or components, styling, layout changes, responsive design, accessibility fixes, Tailwind CSS updates, or anything visual in the `ToDo.Web` project. The agent encodes Blazor Server patterns, Tailwind CSS 4.3+ conventions, responsive/mobile-first design, and accessibility best practices.

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
- **`ToDo.ApiService`** — Minimal API backend using **vertical slice architecture**. See the `api-creator` skill for full conventions. Uses EF Core with SQL Server (`TodoDbContext`). Each endpoint is a self-contained feature slice in its own file under `Features/`.
- **`ToDo.Web`** — Blazor Server frontend (interactive server render mode). See the `frontend-dev` skill for full conventions. Uses Tailwind CSS 4.3+ with utility-first styling and the project's custom design tokens.
- **`ToDo.ServiceDefaults`** — Shared setup referenced by all services. Configures OpenTelemetry, standard HTTP resilience, service discovery on all `HttpClient`s, and `/health` + `/alive` endpoints.
- **`ToDo.Tests`** — Integration tests using `Aspire.Hosting.Testing` + xUnit v3. Boots the full topology via `DistributedApplicationTestingBuilder.CreateAsync<Projects.ToDo_AppHost>`.

## Key conventions

> **For API endpoint conventions** (vertical slices, route patterns, OpenAPI metadata, testing) → delegate to `@api-creator` agent or refer to the `api-creator` skill.

> **For frontend/UI conventions** (Blazor components, Tailwind CSS, responsive design, accessibility) → delegate to `@frontend-dev` agent or refer to the `frontend-dev` skill.

**Inter-service HTTP** — always use `https+http://<resourceName>` as the base address. Resilience and discovery are automatic from `ServiceDefaults`.

**Adding a new Aspire resource**: register it in `AppHost.cs`, wire consumers with `WithReference()`/`WaitFor()`. For a new service project, also add `builder.AddServiceDefaults()` + `app.MapDefaultEndpoints()` and reference `ToDo.ServiceDefaults`.

**EF Core / database** — the `TodoDbContext` uses `HasData()` for seed data. Schema is applied with `EnsureCreated()` (not migrations). If migrations are added later, remove the `EnsureCreated()` call.

**C# style**: top-level `Program.cs` (no `Main` method), file-scoped namespaces, nullable enabled, primary constructors, collection expressions (`[]`), `required` properties where appropriate. Target framework is `net10.0` everywhere.

## Tooling

- `.vscode/mcp.json` registers the **Aspire MCP server** (`aspire agent mcp`) for AppHost and resource operations.
- `.agents/skills/` has project-scoped skills: `aspire`, `aspireify`, `dotnet-inspect`, `playwright-cli`, `api-creator`, `frontend-dev`, `api-versioning`.
- `.github/agents/api-creator.md` — custom agent for all API development tasks.
- `.github/agents/frontend-dev.md` — custom agent for all frontend/UI development tasks.

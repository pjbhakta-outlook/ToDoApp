---
name: api-creator
description: "Use this skill when the user wants to create, modify, or delete an API endpoint in the ToDo.ApiService project. This includes adding new endpoints, updating existing endpoint logic, adding new entity types, or writing integration tests for API endpoints. Trigger on any mention of 'endpoint', 'API', 'route', 'controller', 'feature slice', or requests to expose new backend functionality. Do not use for frontend/Blazor changes, Aspire infrastructure, or non-API concerns."
---

# API Creator Skill

Use this skill to build, modify, or extend API endpoints in the `ToDo.ApiService` project following vertical slice architecture conventions.

## When to use

- Creating a new API endpoint
- Modifying an existing endpoint's behavior
- Adding a new entity/domain to the API
- Writing or updating integration tests for API endpoints
- Adding OpenAPI metadata to endpoints

## Architecture: Vertical Slice

The API uses **vertical slice architecture** — organized by feature, not by technical layer.

### Folder structure

```
ToDo.ApiService/
├── Data/                          # Shared: entities, DbContext
│   ├── TodoDbContext.cs
│   └── TodoItem.cs
├── Features/
│   └── <Domain>/                  # One folder per domain
│       ├── CreateTodo.cs          # One file per endpoint
│       ├── GetAllTodos.cs
│       ├── GetTodoCount.cs
│       ├── ToggleTodo.cs
│       └── DeleteTodo.cs
└── Program.cs                     # Thin composition root
```

### Endpoint file template

Every endpoint lives in its own file with this structure:

```csharp
using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.<Domain>;

public static class <OperationName>
{
    // Co-locate request/response records here (not in a shared folder)
    public record <Name>Request(...);
    public record <Name>Response(...);

    public static void MapEndpoint(WebApplication app)
    {
        app.Map<Method>("/api/<resource>", async (TodoDbContext db) =>
        {
            // Implementation
        })
        .WithName("<OperationName>")
        .WithDescription("<What this endpoint does>")
        .Produces<ResponseType>(StatusCodes.Status200OK);
    }
}
```

### Rules

1. **One file per endpoint** — each operation (Create, Get, Delete, etc.) is a separate file.
2. **Static class** named after the operation (e.g., `CreateTodo`, `GetAllTodos`).
3. **`MapEndpoint(WebApplication app)`** — the single public method that registers the route.
4. **Co-located DTOs** — request/response records live in the feature file, not shared.
5. **Program.cs is thin** — only calls `MapEndpoint`. No inline lambdas or business logic.
6. **Route ordering** — register literal-segment endpoints (e.g., `/count`) BEFORE `{id}` parameterized routes.
7. **OpenAPI metadata required** — every endpoint must have `.WithName()`, `.WithDescription()`, and `.Produces<T>()`.
8. **Keep slices independent** — no cross-slice dependencies. Shared concerns go in `Data/` or `Shared/`.

## Route conventions

- Base pattern: `/api/<resource>` (e.g., `/api/todos`)
- Item: `/api/<resource>/{id}`
- Actions: `/api/<resource>/{id}/<action>` (e.g., `/api/todos/{id}/toggle`)
- Aggregates: `/api/<resource>/<aggregate>` (e.g., `/api/todos/count`)

## Database (EF Core)

- `TodoDbContext` is registered via `builder.AddSqlServerDbContext<TodoDbContext>("tododb")`
- Seed data uses `HasData()` in model configuration
- Schema applied with `EnsureCreated()` (no migrations currently)
- New entities: add to `Data/`, configure in `TodoDbContext.OnModelCreating`

## Testing requirements

**Every endpoint MUST have integration tests** in `ToDo.Tests/ApiTests.cs`.

### Test structure (AAA pattern)

```csharp
[Fact]
public async Task <EndpointName>_<Scenario>_<ExpectedResult>()
{
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;
    var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
    await using var __ = app;

    // Act
    var response = await client.<Method>Async("/api/<route>", cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.<Expected>, response.StatusCode);
}
```

### Test coverage requirements

- **Happy-path**: correct status code + response body
- **Edge cases**: non-existent ID returns 404, validation errors return 400
- **No endpoint ships without tests**

### Test conventions

- xUnit v3 (`xunit.v3`), NOT v2
- Cancellation tokens from `TestContext.Current.CancellationToken`
- HTTP clients via `app.CreateHttpClient("apiservice")`
- Gate on `WaitForResourceHealthyAsync` before requests
- DTOs for deserialization are `private record` types at the bottom of the test class

## C# style

- Top-level `Program.cs` (no `Main` method)
- File-scoped namespaces
- Nullable enabled
- Primary constructors
- Collection expressions (`[]`)
- `required` properties where appropriate
- Target framework: `net10.0`

## Build & verify commands

```bash
# Build the API project
dotnet build ToDo.ApiService/ToDo.ApiService.csproj

# Build the full solution
dotnet build ToDo.sln

# Run all tests
dotnet test ToDo.Tests/ToDo.Tests.csproj

# Run specific test
dotnet test ToDo.Tests/ToDo.Tests.csproj --filter "FullyQualifiedName~<TestName>"
```

## Step-by-step: Adding a new endpoint

1. Create `ToDo.ApiService/Features/<Domain>/<Operation>.cs` with the static class, DTOs, and `MapEndpoint`
2. In `Program.cs`, add `<Operation>.MapEndpoint(app);` — literal routes before parameterized ones
3. Add integration tests in `ToDo.Tests/ApiTests.cs` (happy-path + edge cases)
4. Build and run tests to verify

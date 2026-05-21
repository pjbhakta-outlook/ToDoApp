---
name: api-versioning
description: "Use this skill when the user wants to add API versioning to endpoints, create a new API version, deprecate an old version, or configure versioning middleware. Trigger on any mention of 'version', 'v1', 'v2', 'deprecate endpoint', 'breaking change', 'backward compatibility', or 'API evolution'. Do not use for non-versioning endpoint work (use api-creator instead) or frontend/Blazor changes."
---

# API Versioning Skill

Use this skill to introduce, manage, and evolve API versions in the `ToDo.ApiService` project using the official `Asp.Versioning.Http` package.

## When to use

- Adding versioning to an API that currently has none
- Creating a new version of an existing endpoint (e.g., v2 with a different contract)
- Deprecating an old API version
- Configuring versioning strategy (URL segment, header, query string)
- Ensuring backward compatibility when making breaking changes

## When NOT to use

- Creating a brand-new endpoint that doesn't involve versioning (use `api-creator`)
- Frontend/Blazor changes
- Aspire infrastructure work

## Versioning Strategy

**Default: URL segment versioning** — the most explicit and discoverable approach.

```
/api/v1/todos
/api/v1/todos/{id}
/api/v2/todos          ← new version with different contract
```

### Why URL segment

- Visible in browser, logs, and documentation
- Easy to test (just change the URL)
- Works naturally with OpenAPI/Swagger (separate docs per version)
- No magic headers or query params for clients to remember

### Alternative strategies (use only when explicitly requested)

| Strategy | Format | Package Config |
|----------|--------|----------------|
| URL segment | `/api/v1/resource` | `ApiVersionReader.UrlSegment` (default) |
| Header | `X-Api-Version: 1.0` | `ApiVersionReader.Header("X-Api-Version")` |
| Query string | `?api-version=1.0` | `ApiVersionReader.QueryString("api-version")` |

## Package Setup

### 1. Add the NuGet package

```bash
dotnet add ToDo.ApiService/ToDo.ApiService.csproj package Asp.Versioning.Http
```

### 2. Configure services in `Program.cs`

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});
```

### 3. Create versioned route groups

```csharp
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

var v1 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(new ApiVersion(1, 0));

var v2 = app.MapGroup("/api/v{version:apiVersion}")
    .WithApiVersionSet(versionSet)
    .MapToApiVersion(new ApiVersion(2, 0));
```

## Endpoint Registration Pattern

### Versioned endpoint file

Feature files gain a version-aware `MapEndpoint` overload:

```csharp
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Todos;

public static class GetAllTodosV2
{
    public record TodoResponseV2(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt, DateTime? CompletedAt);

    public static void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/todos", async (TodoDbContext db) =>
        {
            var todos = await db.Todos.ToListAsync();
            return Results.Ok(todos.Select(t => new TodoResponseV2(
                t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt, t.CompletedAt)));
        })
        .WithName("GetAllTodosV2")
        .WithDescription("Get all todos with extended metadata (v2)")
        .Produces<List<TodoResponseV2>>(StatusCodes.Status200OK);
    }
}
```

### Registration in Program.cs

```csharp
// v1 endpoints (existing behavior)
GetAllTodos.MapEndpoint(v1);
CreateTodo.MapEndpoint(v1);
GetTodoCount.MapEndpoint(v1);
ToggleTodo.MapEndpoint(v1);
DeleteTodo.MapEndpoint(v1);

// v2 endpoints (new/changed behavior)
GetAllTodosV2.MapEndpoint(v2);
CreateTodo.MapEndpoint(v2);   // unchanged endpoints can be shared
```

## Migrating Existing Unversioned Endpoints

When versioning an API that currently uses `/api/todos`:

1. **Add the versioning package and configuration** (see Package Setup above)
2. **Keep unversioned routes working** — set `AssumeDefaultVersionWhenUnspecified = true`
3. **Create v1 group** matching existing behavior (no breaking change)
4. **Create v2 group** for new behavior
5. **Update `MapEndpoint` signatures** to accept `RouteGroupBuilder` instead of `WebApplication`
6. **Optionally deprecate unversioned routes** after clients migrate

### Refactoring MapEndpoint for groups

Change from:
```csharp
public static void MapEndpoint(WebApplication app)
{
    app.MapGet("/api/todos", ...);
}
```

To:
```csharp
public static void MapEndpoint(RouteGroupBuilder group)
{
    group.MapGet("/todos", ...);  // prefix comes from the group
}
```

**Important**: Keep an overload accepting `WebApplication` during transition if unversioned routes must continue working.

## Deprecation

Mark a version as deprecated in the version set:

```csharp
var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasDeprecatedApiVersion(new ApiVersion(1, 0))  // mark v1 deprecated
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();
```

This adds `api-deprecated-versions: 1.0` to response headers, signaling clients to migrate.

## Testing Versioned Endpoints

```csharp
[Fact]
public async Task GetAllTodos_V1_ReturnsOk()
{
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;
    var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
    await using var __ = app;

    // Act
    var response = await client.GetAsync("/api/v1/todos", cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

[Fact]
public async Task GetAllTodos_V2_ReturnsExtendedFields()
{
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;
    var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
    await using var __ = app;

    // Act
    var response = await client.GetAsync("/api/v2/todos", cancellationToken);
    var todos = await response.Content.ReadFromJsonAsync<List<TodoV2Dto>>(cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.All(todos!, t => Assert.NotNull(t.CreatedAt));
}

[Fact]
public async Task GetAllTodos_UnversionedRoute_DefaultsToV1()
{
    // Arrange
    var cancellationToken = TestContext.Current.CancellationToken;
    var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
    await using var __ = app;

    // Act
    var response = await client.GetAsync("/api/todos", cancellationToken);

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
```

## Naming Conventions

| Item | Convention | Example |
|------|-----------|---------|
| V2+ feature file | `<Operation>V<N>.cs` | `GetAllTodosV2.cs` |
| V2+ static class | `<Operation>V<N>` | `GetAllTodosV2` |
| V2+ DTOs | `<Name>V<N>` | `TodoResponseV2` |
| WithName | `"<Operation>V<N>"` | `"GetAllTodosV2"` |
| Folder | Same domain folder | `Features/Todos/` |

V1 files keep their original names (no `V1` suffix) for backward compatibility.

## OpenAPI / Swagger

When versioning is active, configure separate OpenAPI documents per version:

```csharp
builder.Services.AddOpenApi("v1", options => 
    options.AddDocumentTransformer((doc, ctx, ct) => { doc.Info.Version = "1.0"; return Task.CompletedTask; }));
builder.Services.AddOpenApi("v2", options => 
    options.AddDocumentTransformer((doc, ctx, ct) => { doc.Info.Version = "2.0"; return Task.CompletedTask; }));
```

Map both:
```csharp
app.MapOpenApi("/openapi/{documentName}.json");
```

## Checklist

When adding a new API version:

- [ ] Package `Asp.Versioning.Http` is referenced
- [ ] Versioning services configured in `Program.cs`
- [ ] Version set includes the new version number
- [ ] Route group created for the new version
- [ ] Feature file created with versioned class name and DTOs
- [ ] Endpoint registered on the correct route group
- [ ] OpenAPI metadata (`.WithName()`, `.WithDescription()`, `.Produces<T>()`) present
- [ ] Integration tests cover the new versioned route
- [ ] Old version still works (backward compatibility verified)
- [ ] Deprecated versions marked with `HasDeprecatedApiVersion` if applicable

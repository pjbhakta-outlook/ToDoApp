using Microsoft.EntityFrameworkCore;
using ToDo.ApiService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.AddSqlServerDbContext<TodoDbContext>("tododb");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "Todo API service is running.");

app.MapGet("/api/todos", async (TodoDbContext db) =>
    await db.Todos.OrderByDescending(t => t.CreatedAt).ToListAsync());

app.MapPost("/api/todos", async (CreateTodoRequest request, TodoDbContext db) =>
{
    var item = new TodoItem
    {
        Title = request.Title,
        Description = request.Description ?? "",
        CreatedAt = DateTime.UtcNow
    };
    db.Todos.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/api/todos/{item.Id}", item);
});

app.MapPatch("/api/todos/{id}/toggle", async (int id, TodoDbContext db) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.IsCompleted = !item.IsCompleted;
    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/api/todos/{id}", async (int id, TodoDbContext db) =>
{
    var item = await db.Todos.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Todos.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDefaultEndpoints();

app.Run();

public record CreateTodoRequest(string Title, string? Description);

using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class GetTodosByList
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/lists/{listId}/todos", async (int listId, TodoDbContext db) =>
        {
            var listExists = await db.Lists.AnyAsync(l => l.Id == listId);
            if (!listExists) return Results.NotFound();

            var items = await db.Todos
                .Where(t => t.ListId == listId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Results.Ok(items);
        })
        .WithName("GetTodosByList")
        .WithDescription("Retrieves all todo items for a specific list.")
        .Produces<List<TodoItem>>()
        .Produces(StatusCodes.Status404NotFound);
    }
}

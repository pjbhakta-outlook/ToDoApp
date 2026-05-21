using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class ToggleTodoInList
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/lists/{listId}/todos/{todoId}/toggle", async (int listId, int todoId, TodoDbContext db) =>
        {
            var item = await db.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.ListId == listId);
            if (item is null) return Results.NotFound();

            item.IsCompleted = !item.IsCompleted;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        })
        .WithName("ToggleTodoInList")
        .WithDescription("Toggles the completion status of a todo item within a list.")
        .Produces<TodoItem>()
        .Produces(StatusCodes.Status404NotFound);
    }
}

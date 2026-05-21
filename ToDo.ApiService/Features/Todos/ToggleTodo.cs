using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Todos;

public static class ToggleTodo
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapPatch("/api/todos/{id}/toggle", async (int id, TodoDbContext db) =>
        {
            var item = await db.Todos.FindAsync(id);
            if (item is null) return Results.NotFound();

            item.IsCompleted = !item.IsCompleted;
            await db.SaveChangesAsync();
            return Results.Ok(item);
        })
        .WithName("ToggleTodo")
        .WithDescription("Toggles the completion status of a todo item.")
        .Produces<TodoItem>()
        .Produces(StatusCodes.Status404NotFound);
    }
}

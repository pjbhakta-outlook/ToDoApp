using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Todos;

public static class DeleteTodo
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/todos/{id}", async (int id, TodoDbContext db) =>
        {
            var item = await db.Todos.FindAsync(id);
            if (item is null) return Results.NotFound();

            db.Todos.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteTodo")
        .WithDescription("Deletes a todo item by ID.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

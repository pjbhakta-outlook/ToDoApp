using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class DeleteTodoInList
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/lists/{listId}/todos/{todoId}", async (int listId, int todoId, TodoDbContext db) =>
        {
            var item = await db.Todos.FirstOrDefaultAsync(t => t.Id == todoId && t.ListId == listId);
            if (item is null) return Results.NotFound();

            db.Todos.Remove(item);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteTodoInList")
        .WithDescription("Deletes a todo item from a specific list.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}

using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class DeleteList
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapDelete("/api/lists/{listId}", async (int listId, TodoDbContext db) =>
        {
            var list = await db.Lists.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == listId);
            if (list is null) return Results.NotFound();
            if (list.Items.Count > 0) return Results.Conflict("Cannot delete a list that still has items.");

            db.Lists.Remove(list);
            await db.SaveChangesAsync();
            return Results.NoContent();
        })
        .WithName("DeleteList")
        .WithDescription("Deletes an empty todo list.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status409Conflict);
    }
}

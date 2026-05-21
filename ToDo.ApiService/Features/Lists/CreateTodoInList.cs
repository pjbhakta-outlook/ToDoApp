using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class CreateTodoInList
{
    public record CreateTodoRequest(string Title, string? Description);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/lists/{listId}/todos", async (int listId, CreateTodoRequest request, TodoDbContext db) =>
        {
            var listExists = await db.Lists.AnyAsync(l => l.Id == listId);
            if (!listExists) return Results.NotFound();

            var item = new TodoItem
            {
                Title = request.Title,
                Description = request.Description ?? "",
                ListId = listId,
                CreatedAt = DateTime.UtcNow
            };
            db.Todos.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/lists/{listId}/todos/{item.Id}", item);
        })
        .WithName("CreateTodoInList")
        .WithDescription("Creates a new todo item in a specific list.")
        .Produces<TodoItem>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status404NotFound)
        .ProducesValidationProblem();
    }
}

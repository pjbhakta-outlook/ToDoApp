using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Todos;

public static class GetAllTodos
{
    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/todos", async (TodoDbContext db) =>
            await db.Todos.OrderByDescending(t => t.CreatedAt).ToListAsync())
            .WithName("GetAllTodos")
            .WithDescription("Retrieves all todo items ordered by creation date descending.")
            .Produces<List<TodoItem>>();
    }
}

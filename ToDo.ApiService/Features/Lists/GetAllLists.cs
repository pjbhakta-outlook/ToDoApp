using Microsoft.EntityFrameworkCore;
using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class GetAllLists
{
    public record TodoListDto(int Id, string Name, DateTime CreatedAt, int ActiveCount, int CompletedCount);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/lists", async (TodoDbContext db) =>
            await db.Lists
                .OrderBy(l => l.CreatedAt)
                .Select(l => new TodoListDto(
                    l.Id,
                    l.Name,
                    l.CreatedAt,
                    l.Items.Count(i => !i.IsCompleted),
                    l.Items.Count(i => i.IsCompleted)))
                .ToListAsync())
            .WithName("GetAllLists")
            .WithDescription("Retrieves all todo lists with item counts.")
            .Produces<List<TodoListDto>>();
    }
}

using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Lists;

public static class CreateList
{
    public record CreateListRequest(string Name);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/lists", async (CreateListRequest request, TodoDbContext db) =>
        {
            var list = new TodoList
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };
            db.Lists.Add(list);
            await db.SaveChangesAsync();
            return Results.Created($"/api/lists/{list.Id}", new { list.Id, list.Name, list.CreatedAt });
        })
        .WithName("CreateList")
        .WithDescription("Creates a new todo list.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}

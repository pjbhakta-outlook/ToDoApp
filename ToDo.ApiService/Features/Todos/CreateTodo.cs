using ToDo.ApiService.Data;

namespace ToDo.ApiService.Features.Todos;

public static class CreateTodo
{
    public record CreateTodoRequest(string Title, string? Description);

    public static void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/todos", async (CreateTodoRequest request, TodoDbContext db) =>
        {
            var item = new TodoItem
            {
                Title = request.Title,
                Description = request.Description ?? "",
                CreatedAt = DateTime.UtcNow
            };
            db.Todos.Add(item);
            await db.SaveChangesAsync();
            return Results.Created($"/api/todos/{item.Id}", item);
        })
        .WithName("CreateTodo")
        .WithDescription("Creates a new todo item.")
        .Produces<TodoItem>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}

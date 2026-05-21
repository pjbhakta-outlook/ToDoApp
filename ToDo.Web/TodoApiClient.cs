namespace ToDo.Web;

public class TodoApiClient(HttpClient httpClient)
{
    // List operations
    public async Task<TodoListDto[]> GetListsAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoListDto[]>("/api/lists", cancellationToken) ?? [];
    }

    public async Task<TodoListDto?> CreateListAsync(string name, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/lists", new { Name = name }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoListDto>(cancellationToken);
    }

    public async Task DeleteListAsync(int listId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/lists/{listId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    // Todo operations (list-scoped)
    public async Task<TodoItemDto[]> GetTodosByListAsync(int listId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoItemDto[]>($"/api/lists/{listId}/todos", cancellationToken) ?? [];
    }

    public async Task<TodoItemDto?> CreateTodoInListAsync(int listId, string title, string description, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/lists/{listId}/todos", new { Title = title, Description = description }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItemDto>(cancellationToken);
    }

    public async Task ToggleTodoInListAsync(int listId, int todoId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsync($"/api/lists/{listId}/todos/{todoId}/toggle", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTodoInListAsync(int listId, int todoId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/lists/{listId}/todos/{todoId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    // Legacy endpoints (keep for backward compat)
    public async Task<TodoItemDto[]> GetTodosAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoItemDto[]>("/api/todos", cancellationToken) ?? [];
    }

    public async Task<TodoItemDto?> CreateTodoAsync(string title, string description, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/todos", new { Title = title, Description = description }, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TodoItemDto>(cancellationToken);
    }

    public async Task ToggleTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsync($"/api/todos/{id}/toggle", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/todos/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public record TodoItemDto(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt, int ListId = 0);
public record TodoListDto(int Id, string Name, DateTime CreatedAt, int ActiveCount, int CompletedCount);

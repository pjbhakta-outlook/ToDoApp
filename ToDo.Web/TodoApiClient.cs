namespace ToDo.Web;

public class TodoApiClient(HttpClient httpClient)
{
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

public record TodoItemDto(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt);

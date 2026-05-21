namespace ToDo.ApiService.Data;

public class TodoList
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<TodoItem> Items { get; set; } = [];
}

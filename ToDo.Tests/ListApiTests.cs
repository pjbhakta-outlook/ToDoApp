using System.Net.Http.Json;
using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace ToDo.Tests;

public class ListApiTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(120);

    private async Task<(IDistributedApplicationTestingBuilder AppHost, DistributedApplication App, HttpClient Client)> CreateAppAndClientAsync(CancellationToken cancellationToken)
    {
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.ToDo_AppHost>(cancellationToken);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

        var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        var httpClient = app.CreateHttpClient("apiservice");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

        return (appHost, app, httpClient);
    }

    [Fact]
    public async Task GetAllLists_ReturnsOkWithSeededDefaultList()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.GetAsync("/api/lists", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var lists = await response.Content.ReadFromJsonAsync<List<TodoListDto>>(cancellationToken);
        Assert.NotNull(lists);
        Assert.Contains(lists, list => list.Name == "Default");
    }

    [Fact]
    public async Task CreateList_ReturnsCreatedWithNewList()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var request = new { Name = "Work" };
        var response = await client.PostAsJsonAsync("/api/lists", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<CreatedListDto>(cancellationToken);
        Assert.NotNull(list);
        Assert.True(list.Id > 0);
        Assert.Equal("Work", list.Name);
    }

    [Fact]
    public async Task DeleteList_EmptyList_ReturnsNoContent()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;
        var createResponse = await client.PostAsJsonAsync("/api/lists", new { Name = "Empty List" }, cancellationToken);
        var createdList = await createResponse.Content.ReadFromJsonAsync<CreatedListDto>(cancellationToken);

        // Act
        var response = await client.DeleteAsync($"/api/lists/{createdList!.Id}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteList_NonEmptyList_ReturnsConflict()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.DeleteAsync("/api/lists/1", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetTodosByList_ReturnsOkWithItems()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.GetAsync("/api/lists/1/todos", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoDto>>(cancellationToken);
        Assert.NotNull(todos);
        Assert.True(todos.Count >= 5, "Should contain at least the 5 seeded items");
        Assert.Contains(todos, todo => todo.ListId == 1 && todo.Title == "Team Meeting");
    }

    [Fact]
    public async Task CreateTodoInList_ReturnsCreatedWithItem()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var request = new { Title = "List Test Todo", Description = "Created in list" };
        var response = await client.PostAsJsonAsync("/api/lists/1/todos", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);
        Assert.NotNull(todo);
        Assert.Equal("List Test Todo", todo.Title);
        Assert.Equal("Created in list", todo.Description);
        Assert.Equal(1, todo.ListId);
        Assert.False(todo.IsCompleted);
    }

    [Fact]
    public async Task ToggleTodoInList_ReturnsOkWithToggledState()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.PatchAsync("/api/lists/1/todos/1/toggle", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);
        Assert.NotNull(todo);
        Assert.Equal(1, todo.ListId);
        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task DeleteTodoInList_ReturnsNoContent()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;
        var createResponse = await client.PostAsJsonAsync("/api/lists/1/todos", new { Title = "Delete Me", Description = "" }, cancellationToken);
        var createdTodo = await createResponse.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);

        // Act
        var response = await client.DeleteAsync($"/api/lists/1/todos/{createdTodo!.Id}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private record TodoListDto(int Id, string Name, DateTime CreatedAt, int ActiveCount, int CompletedCount);

    private record CreatedListDto(int Id, string Name, DateTime CreatedAt);

    private record TodoDto(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt, int ListId);
}

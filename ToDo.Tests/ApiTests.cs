using System.Net.Http.Json;
using Aspire.Hosting;
using Microsoft.Extensions.Logging;

namespace ToDo.Tests;

public class ApiTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

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
    public async Task GetAllTodos_ReturnsOkWithSeededData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.GetAsync("/api/todos", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoDto>>(cancellationToken);
        Assert.NotNull(todos);
        Assert.True(todos.Count >= 5, "Should contain at least the 5 seeded items");
    }

    [Fact]
    public async Task CreateTodo_ReturnsCreatedWithNewItem()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var request = new { Title = "Test Todo", Description = "Created by integration test" };
        var response = await client.PostAsJsonAsync("/api/todos", request, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);
        Assert.NotNull(todo);
        Assert.Equal("Test Todo", todo.Title);
        Assert.Equal("Created by integration test", todo.Description);
        Assert.False(todo.IsCompleted);
    }    

    [Fact]
    public async Task ToggleTodo_ReturnsOkWithToggledState()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.PatchAsync("/api/todos/1/toggle", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);
        Assert.NotNull(todo);
        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task ToggleTodo_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.PatchAsync("/api/todos/99999/toggle", null, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_ReturnsNoContent()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;
        var createRequest = new { Title = "To Delete", Description = "" };
        var createResponse = await client.PostAsJsonAsync("/api/todos", createRequest, cancellationToken);
        var created = await createResponse.Content.ReadFromJsonAsync<TodoDto>(cancellationToken);

        // Act
        var response = await client.DeleteAsync($"/api/todos/{created!.Id}", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteTodo_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var (_, app, client) = await CreateAppAndClientAsync(cancellationToken);
        await using var __ = app;

        // Act
        var response = await client.DeleteAsync("/api/todos/99999", cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private record TodoDto(int Id, string Title, string Description, bool IsCompleted, DateTime CreatedAt);
}

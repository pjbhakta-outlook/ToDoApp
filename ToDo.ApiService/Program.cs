using ToDo.ApiService.Data;
using ToDo.ApiService.Features.Lists;
using ToDo.ApiService.Features.Todos;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddSqlServerDbContext<TodoDbContext>("tododb");

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => Results.Redirect("/swagger"));

// Map feature endpoints (literal segments before parameterized routes)
GetAllLists.MapEndpoint(app);
CreateList.MapEndpoint(app);
DeleteList.MapEndpoint(app);
GetTodosByList.MapEndpoint(app);
CreateTodoInList.MapEndpoint(app);
ToggleTodoInList.MapEndpoint(app);
DeleteTodoInList.MapEndpoint(app);

GetAllTodos.MapEndpoint(app);
CreateTodo.MapEndpoint(app);
ToggleTodo.MapEndpoint(app);
DeleteTodo.MapEndpoint(app);

app.MapDefaultEndpoints();

app.Run();

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var sql = builder.AddSqlServer("sqlserver");
var tododb = sql.AddDatabase("tododb");

var apiService = builder.AddProject<Projects.ToDo_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(tododb)
    .WaitFor(tododb);

builder.AddProject<Projects.ToDo_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();

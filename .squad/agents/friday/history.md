# Friday — History

## Project Context

- **Project:** ToDo — .NET 10 Aspire distributed todo application
- **Stack:** .NET Aspire, Docker, Azure Container Apps, azd, Redis, SQL Server
- **User:** PJ Bhakta
- **Key files:** `ToDo.AppHost/AppHost.cs`, `ToDo.ServiceDefaults/`
- **Current config:** SQL Server with `WithLifetime(ContainerLifetime.Persistent)` + `WithDataVolume()`, Redis cache

## Learnings


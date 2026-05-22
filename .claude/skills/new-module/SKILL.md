---
name: new-module
description: Scaffold a new module for the modular monolith. Creates folder structure, DbContext, Contracts project, and module registration.
---

# New Module Scaffold

Create a new module for the AgriMarket modular monolith.

Usage: `/new-module <ModuleName>`

The module name is: $ARGUMENTS

## What to Create

### 1. Contracts Project

`Modules/<ModuleName>/AgriMarket.Modules.<ModuleName>.Contracts/`

```
AgriMarket.Modules.<ModuleName>.Contracts.csproj
I<ModuleName>Module.cs          # Public facade interface
Dtos/                           # Public DTOs
Events/                         # Integration events (INotification)
```

The `.csproj` should reference only `AgriMarket.Shared` and `MediatR.Contracts`.

All types in this project are `public`.

### 2. Core Module Project

`Modules/<ModuleName>/AgriMarket.Modules.<ModuleName>/`

```
AgriMarket.Modules.<ModuleName>.csproj
<ModuleName>Module.cs           # IModule implementation
Domain/                         # Entities
Application/                    # Services, MediatR handlers
Infrastructure/
  <ModuleName>DbContext.cs      # Own DbContext with HasDefaultSchema
  Repositories/                 # EF repositories
Api/
  Controllers/                  # API controllers (internal)
```

The `.csproj` should reference:
- Own `.Contracts` project
- `AgriMarket.Shared`
- EF Core, MediatR

All implementation classes are `internal`. Use `[assembly: InternalsVisibleTo("AgriMarket.Tests")]`.

### 3. DbContext Template

```csharp
internal sealed class <ModuleName>DbContext : DbContext
{
    public <ModuleName>DbContext(DbContextOptions<<ModuleName>DbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("<module_name_lowercase>");
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
```

### 4. Module Registration Template

```csharp
internal sealed class <ModuleName>Module : IModule
{
    public string Name => "<ModuleName>";

    public IServiceCollection RegisterServices(
        IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<<ModuleName>DbContext>(opts =>
            opts.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                o => o.MigrationsHistoryTable(
                    "__EFMigrationsHistory", "<module_name_lowercase>")));

        // Register repositories and services here

        return services;
    }

    public WebApplication MapEndpoints(WebApplication app)
    {
        // Map API endpoints here
        return app;
    }
}
```

### 5. Register in Bootstrapper

Add the module to `Program.cs` in the Bootstrapper project:
- Call `RegisterServices` in the service registration section
- Call `MapEndpoints` in the endpoint mapping section
- Register the module's assembly with MediatR

### 6. Add to Solution

```bash
dotnet sln add Modules/<ModuleName>/AgriMarket.Modules.<ModuleName>/AgriMarket.Modules.<ModuleName>.csproj
dotnet sln add Modules/<ModuleName>/AgriMarket.Modules.<ModuleName>.Contracts/AgriMarket.Modules.<ModuleName>.Contracts.csproj
```

After scaffolding, run `dotnet build` to verify everything compiles.

---
paths:
  - "Modules/**"
  - "Shared/**"
  - "**/*Module*.cs"
---
# Modular Monolith Rules

## Module Structure

Every module consists of two projects:

- `AgriMarket.Modules.<Name>/` — internal implementation (entities, services, repos, DbContext, controllers)
- `AgriMarket.Modules.<Name>.Contracts/` — public API (interfaces, DTOs, integration events)

## Access Modifiers

- All classes inside module core are `internal` by default
- Only Contracts project contains `public` types
- Use `[assembly: InternalsVisibleTo("...Tests")]` for test access only

## Dependency Rules

Allowed references:
- Module → own `.Contracts`
- Module → another module's `.Contracts` (never the core project)
- Module → `AgriMarket.Shared`
- Bootstrapper → all modules

Forbidden:
- Module.A → Module.B (direct core-to-core reference)
- Any module → Bootstrapper

## Inter-Module Communication

- **Synchronous queries**: call another module's Contracts interface (e.g. `IUsersModule.GetProfileAsync`)
- **Async events**: publish `INotification` via MediatR — handlers in other modules react
- **Never**: import another module's entities, never share DbContext

## Data Ownership

- Each module owns its DB schema (`users`, `marketplace`, `bookings`, `messaging`)
- No cross-schema foreign keys — use IDs and resolve via Contracts
- Each DbContext declares `HasDefaultSchema("name")` in `OnModelCreating`

## Module Registration

Each module implements `IModule` with:
- `RegisterServices(IServiceCollection, IConfiguration)` — DI registrations
- `MapEndpoints(WebApplication)` — route mappings

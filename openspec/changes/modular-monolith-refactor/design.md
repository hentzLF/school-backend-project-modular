## Context

AgriMarket today is a five-project layered monolith. `AgriMarket.Domain` holds 19 EF entities; `AgriMarket.DAL` holds a single `AppDbContext` (with all relationships and seeding configured in one `OnModelCreating`), 9 EF repositories, `EfUnitOfWork`, and `BCryptPasswordHasher`; `AgriMarket.BLL` holds 14 services and 46 DTOs; `AgriMarket.Api` holds 13 API controllers, the SignalR `MessageHub`, and the `Program.cs` composition root; `AgriMarket.Web` holds the MVC Admin and Client areas. `AgriMarket.Tests` references BLL/DAL/Web/Api directly; `AgriMarket.E2E` references Web and relies on `InternalsVisibleTo`.

`CLAUDE.md` already specifies the target modular-monolith architecture. This change implements it. The hard part is not moving files — it is the single `AppDbContext`, whose `OnModelCreating` wires foreign keys that cross domain boundaries: `Booking → ServiceListing/Availability` (Marketplace) and `Booking → UserProfile` (Users); `Review → Booking` and `Review → UserProfile`; `Conversation → Booking` (nullable); `Message/ConversationParticipant/MessageRead → UserProfile`; `Equipment → UserProfile`.

## Goals / Non-Goals

**Goals:**
- Organize the codebase by business domain with four bounded modules.
- One `.csproj`, one `DbContext`, one DB schema per module.
- Implementation classes `internal`; only `.Contracts` projects `public`.
- Cross-module communication only via `.Contracts` interfaces (sync queries) and MediatR `INotification` events (async side effects).
- All 84 existing tests pass unchanged in behavior.
- Each numbered task block builds green and is committed.

**Non-Goals:**
- Changing observable API/MVC behavior, request/response shapes, or auth semantics.
- Changing the database table/column layout — only the schema namespace changes.
- Splitting the deployment — it stays a single process (modular *monolith*).
- Rewriting business logic beyond what boundary enforcement requires.

## Decisions

### 1. Solution layout

```
src/
  Shared/AgriMarket.Shared/
  Modules/
    Users/AgriMarket.Modules.Users.Contracts/        + AgriMarket.Modules.Users/
    Marketplace/AgriMarket.Modules.Marketplace.Contracts/ + AgriMarket.Modules.Marketplace/
    Bookings/AgriMarket.Modules.Bookings.Contracts/   + AgriMarket.Modules.Bookings/
    Messaging/AgriMarket.Modules.Messaging.Contracts/ + AgriMarket.Modules.Messaging/
  Bootstrapper/AgriMarket.Api/
  AgriMarket.Web/
```
Project files physically move under `src/`. The bootstrapper and Web reference module core + `.Contracts` projects; module cores reference only `AgriMarket.Shared` and the `.Contracts` projects they consume — never another module's core.

### 2. Module boundary enforcement

A module core project references: `AgriMarket.Shared`, its own `.Contracts`, and the `.Contracts` of modules it queries. It must NOT reference another module's core project. All non-`.Contracts` types are declared `internal`. `.Contracts` projects contain only `public` interfaces, DTO `record`s, and integration-event `record`s — no EF, no implementation.

### 3. Cross-module references become loose IDs

EF navigation properties that cross a module boundary are replaced by plain `Guid` foreign-key properties with no navigation property and no EF relationship configuration. Example: `Booking.ServiceListingId` stays as a `Guid`; `Booking.ServiceListing` navigation is removed. Within a module, navigation properties are kept. Where a service previously used `.Include()` across a boundary, it now calls a `.Contracts` interface (e.g. `ICatalogModule.GetListingSummaryAsync`) and composes the DTO in memory. Each module's `DbContext` only registers its own entities; cross-module `Guid` columns are mapped as plain properties.

### 4. One DbContext + one schema per module

`UsersDbContext`, `MarketplaceDbContext`, `BookingsDbContext`, `MessagingDbContext`, each calling `modelBuilder.HasDefaultSchema("<module>")`. Relationship and index configuration from the old `AppDbContext.OnModelCreating` is partitioned: each `DbContext` keeps only the configuration for its own entities. County/Municipality `HasData` seeding moves to `MarketplaceDbContext`.

### 5. EF migrations split per module

Each module gets a fresh initial migration generated against its own `DbContext` (`dotnet ef migrations add InitialCreate --context <ModuleDbContext>`). Because each module owns a distinct schema, the four migration sets are independent. The bootstrapper applies all four on startup. Old `AgriMarket.DAL/Migrations` are removed once the per-module migrations exist. This is acceptable because the deployment target is rebuilt from compose; table shapes are preserved so a data-bearing environment could be migrated with a hand-written schema-move script if ever needed (out of scope here).

### 6. IModule composition

`AgriMarket.Shared` defines:
```csharp
public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
```
Each module provides one `internal sealed` `IModule` implementation (exposed via a single `public` static entry point or assembly marker) that registers its `DbContext`, repositories, services, and MediatR handlers, and maps its controllers/hub. `Program.cs` holds an explicit ordered list of the four modules plus shared infrastructure, calling `RegisterServices` then `MapEndpoints` on each.

### 7. Inter-module side effects via MediatR integration events

Synchronous cross-module *queries* use `.Contracts` interfaces (`IUsersModule`, `ICatalogModule`, `IBookingsModule`, `IMessagingModule`). Asynchronous cross-module *side effects* use MediatR `INotification` integration events defined in the publishing module's `.Contracts` project. The known case: when a booking is confirmed, Messaging creates a conversation. Bookings publishes `BookingConfirmedEvent`; a handler in Messaging consumes it. MediatR is registered once in the bootstrapper across all module assemblies.

### 8. Controllers and SignalR hub placement

API controllers move into their owning module core project (controllers are `internal`-friendly via ASP.NET application parts registered by the module's `IModule.RegisterServices`). The SignalR `MessageHub` and `SignalRMessageNotifier` move into the Messaging module. The bootstrapper registers application parts for each module assembly so controllers are discovered. MVC `AgriMarket.Web` keeps its areas but its controllers call module `.Contracts` interfaces instead of BLL services directly.

### 9. Test project rewiring

`AgriMarket.Tests` updates its `ProjectReference`s from BLL/DAL to the module core projects, and unit tests that mock cross-module dependencies switch to mocking `.Contracts` interfaces. `AgriMarket.E2E` keeps referencing `AgriMarket.Web`; `InternalsVisibleTo` is added to each module core for the test assemblies that need internal access. Test behavior and assertions are unchanged.

## Risks / Trade-offs

- **Large surface area** → Mitigated by phasing: shared infra first, then one module at a time, each phase building green and committed independently. A module can be left dual-referenced transitionally until Phase 6 wires composition.
- **Cross-module `.Include()` removal changes query shape** → Services that joined across boundaries now do N+1-style `.Contracts` lookups; mitigated by batch lookup methods on `.Contracts` interfaces where a list is involved.
- **EF migration reset** → Per-module migrations are regenerated fresh; existing dev databases are recreated from compose. Documented as acceptable for this project stage.
- **Internal controllers** → ASP.NET controller discovery needs application parts per module assembly; if discovery proves fragile, controllers may stay `public` inside the module (the `internal`-only rule applies to services/repositories/entities first).
- **Test `InternalsVisibleTo` churn** → Each module core declares `InternalsVisibleTo` for `AgriMarket.Tests`; E2E continues through `AgriMarket.Web`'s public surface.

## Open Questions

- Whether API controllers can remain `internal` with application parts, or must stay `public` — resolved pragmatically during Phase 6 (Decision 8 fallback).
- Whether a transitional period with both old and new projects in the solution is needed — yes; old projects are removed only in Phase 7 after all modules and tests are green.

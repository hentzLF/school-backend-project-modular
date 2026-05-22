## Why

AgriMarket is currently a layered monolith: `AgriMarket.Domain` (entities), `AgriMarket.DAL` (one `AppDbContext`, repositories), `AgriMarket.BLL` (services, DTOs), `AgriMarket.Api` (controllers, SignalR), `AgriMarket.Web` (MVC). Every layer spans every business domain, so a change to bookings touches the same projects as a change to messaging. There is no enforced boundary between domains — any service can reach any entity, and the single `AppDbContext` couples all 19 entities together.

Reorganizing by **business domain** instead of **technical layer** gives each domain (Users, Marketplace, Bookings, Messaging) an enforced boundary: its own project, its own `DbContext`, its own database schema, and `internal` implementation classes. Cross-domain coupling becomes explicit and reviewable — it can only happen through published `.Contracts` interfaces or MediatR integration events. This is the modular-monolith architecture already documented as the target in `CLAUDE.md`.

## What Changes

- **Restructure the solution** into `src/Bootstrapper`, `src/Modules/{Users,Marketplace,Bookings,Messaging}`, `src/Shared`, and `src/AgriMarket.Web`.
- Introduce `AgriMarket.Shared` with the `IModule` interface, base entity/repository abstractions, and MediatR integration-event base types.
- Split the single `AppDbContext` into four module `DbContext`s, each with its own PostgreSQL schema (`users`, `marketplace`, `bookings`, `messaging`).
- Move the 19 entities, 14 services, 46 DTOs, 9 repositories, and 13 API controllers into their owning modules.
- Each module exposes a `.Contracts` project (public interfaces + DTOs); all implementation classes become `internal`.
- **BREAKING (internal architecture)**: cross-module entity navigation properties (e.g. `Booking.ServiceListing`, `Conversation.Booking`, `Message.SenderProfile`) become loose `Guid` ID references; cross-module data is resolved through `.Contracts` interfaces.
- Inter-module side effects move to MediatR `INotification` integration events (e.g. `BookingConfirmedEvent` → Messaging creates a conversation).
- The bootstrapper (`AgriMarket.Api`) composes modules via `IModule.RegisterServices()` / `IModule.MapEndpoints()`.
- Remove the obsolete `AgriMarket.Domain`, `AgriMarket.DAL`, and `AgriMarket.BLL` projects.
- Update `AgriMarket.slnx`, `Directory.Build.props`, `docker-compose.yml`, and `.gitlab-ci.yml`.

## Capabilities

### New Capabilities

- `modular-monolith`: the project is organized as a modular monolith — four domain modules with enforced boundaries, communicating only through `.Contracts` interfaces and MediatR integration events.

### Modified Capabilities

_None — this change is a structural refactoring. All existing capabilities (auth, listings, bookings, payments, reviews, messaging, admin) retain identical observable behavior; their code simply moves into the owning module._

## Impact

- **Projects**: `AgriMarket.Domain`/`AgriMarket.DAL`/`AgriMarket.BLL` removed; 9 new projects added (`AgriMarket.Shared` + 4 module cores + 4 `.Contracts`); `AgriMarket.Api` becomes the bootstrapper.
- **Database**: four `DbContext`s replace one; each owns a schema. EF migrations split per module. Table/column shapes are preserved so existing data is compatible.
- **Tests**: `AgriMarket.Tests` (28 unit/integration) and `AgriMarket.E2E` (56 E2E) must continue to pass; their project references and `InternalsVisibleTo` targets are updated to the new module projects.
- **Cross-module references**: replaced with `Guid` IDs + `.Contracts` lookups and MediatR events.
- **Composition root**: `Program.cs` rewritten to discover and register modules through `IModule`.
- **No git push** is performed by this change — commits stay local.

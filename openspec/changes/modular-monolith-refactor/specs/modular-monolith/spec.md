## ADDED Requirements

### Requirement: Solution is organized as domain modules
The solution SHALL be organized by business domain under `src/`, with four domain modules — Users, Marketplace, Bookings, Messaging — plus a shared kernel (`AgriMarket.Shared`), a bootstrapper (`AgriMarket.Api`), and the MVC web project (`AgriMarket.Web`). The legacy layered projects `AgriMarket.Domain`, `AgriMarket.DAL`, and `AgriMarket.BLL` SHALL NOT exist after this change.

#### Scenario: Module owns its full vertical slice
- **WHEN** a module is inspected
- **THEN** it contains its own entities, `DbContext`, repositories, services, and (where applicable) controllers, and exposes a separate `.Contracts` project for public interfaces and DTOs

#### Scenario: Solution builds after restructuring
- **WHEN** `dotnet build` is run on the restructured solution
- **THEN** the build succeeds with zero errors

### Requirement: Modules do not reference each other directly
A module core project SHALL reference only `AgriMarket.Shared` and `.Contracts` projects. A module core project SHALL NOT have a project reference to another module's core project.

#### Scenario: Cross-module dependency goes through Contracts
- **WHEN** module A needs data owned by module B
- **THEN** module A depends on `B.Contracts` and calls a published interface — it does not reference B's core project or B's `DbContext`

#### Scenario: Implementation classes are internal
- **WHEN** a class in a module core project is not part of a `.Contracts` project
- **THEN** it is declared `internal` (entities, repositories, services, `DbContext`)

### Requirement: Each module owns a DbContext and database schema
Each module SHALL define exactly one EF Core `DbContext` bound to its own PostgreSQL schema via `HasDefaultSchema` — `users`, `marketplace`, `bookings`, `messaging`. A module's `DbContext` SHALL register only entities owned by that module.

#### Scenario: Module DbContext is schema-scoped
- **WHEN** a module's `DbContext` `OnModelCreating` runs
- **THEN** it sets its default schema and configures only its own entities' relationships and indexes

#### Scenario: Cross-module foreign keys are loose IDs
- **WHEN** an entity references an entity owned by another module
- **THEN** the reference is stored as a plain `Guid` property with no navigation property and no EF relationship configuration

### Requirement: Cross-module communication uses Contracts and integration events
Synchronous cross-module queries SHALL go through `.Contracts` interfaces. Asynchronous cross-module side effects SHALL be delivered through MediatR `INotification` integration events declared in the publishing module's `.Contracts` project.

#### Scenario: Booking confirmation notifies Messaging
- **WHEN** a booking is confirmed
- **THEN** the Bookings module publishes a `BookingConfirmedEvent` and a handler in the Messaging module creates the associated conversation

### Requirement: The bootstrapper composes modules via IModule
`AgriMarket.Shared` SHALL define an `IModule` interface with `RegisterServices` and `MapEndpoints`. The bootstrapper `Program.cs` SHALL register and map every module through `IModule`.

#### Scenario: Module is wired through IModule
- **WHEN** the application starts
- **THEN** each module's `RegisterServices` registers its `DbContext`, repositories, services, and handlers, and each module's `MapEndpoints` maps its controllers/hub

### Requirement: Existing behavior and tests are preserved
The refactoring SHALL NOT change observable API or MVC behavior. All 28 unit/integration tests in `AgriMarket.Tests` and all 56 E2E tests in `AgriMarket.E2E` SHALL continue to pass.

#### Scenario: Full test suite passes after refactoring
- **WHEN** `dotnet test` is run for the unit/integration and E2E suites
- **THEN** all 84 tests pass

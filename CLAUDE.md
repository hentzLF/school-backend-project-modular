# AgriMarket — Modular Monolith

Agricultural service marketplace built with ASP.NET Core (.NET 10), PostgreSQL, MediatR.

## Quick Reference

```bash
dotnet build                                    # Build all projects
dotnet test                                     # Run unit + integration tests
dotnet test AgriMarket.Tests/                   # Unit/integration only
dotnet test AgriMarket.E2E/                     # E2E (requires Docker for Testcontainers)
dotnet format                                   # Auto-format
dotnet ef migrations add <Name> --context <ModuleDbContext> --project <ModulePath>
```

## Architecture: Modular Monolith

Organized by **business domain**, not by technical layer. Each module owns its entities, services, repositories, controllers, and DbContext.

```
src/
  Bootstrapper/AgriMarket.Api/        # Composition root, Swagger, JWT, SignalR
  Modules/
    Users/          (.Contracts + core)  # Auth, profiles, roles
    Marketplace/    (.Contracts + core)  # Listings, categories, equipment, locations
    Bookings/       (.Contracts + core)  # Bookings, payments, reviews
    Messaging/      (.Contracts + core)  # Conversations, messages, SignalR hub
  Shared/AgriMarket.Shared/            # IModule, base classes, integration events
  AgriMarket.Web/                      # MVC UI (Admin + Client areas)
```

### Module Rules

- Each module = own `.csproj` + own `DbContext` + own DB schema
- Implementation classes are `internal` — only Contracts (interfaces + DTOs) are `public`
- Modules NEVER reference each other directly — only through `.Contracts` projects
- Inter-module communication via **MediatR** (`INotification` for events, `ICatalogModule` interfaces for queries)
- Module registration via `IModule.RegisterServices()` + `IModule.MapEndpoints()` in Program.cs

### Module Communication Patterns

| Pattern | When | Example |
|---------|------|---------|
| **Contracts interface** | Synchronous query across modules | `IUsersModule.GetUserProfileAsync(userId)` |
| **MediatR INotification** | Async event (fire-and-forget) | `BookingConfirmedEvent` -> Messaging creates conversation |
| **Shared DTOs** | Return types in Contracts | `UserSummaryDto`, `ListingSummaryDto` |

## Tech Stack

- .NET 10, ASP.NET Core, EF Core 10 + Npgsql (PostgreSQL)
- MediatR for CQRS + inter-module events
- JWT Bearer (API) + Cookie auth (MVC)
- SignalR for real-time messaging
- Swashbuckle (Swagger) + Asp.Versioning (API v1 via URL segment)
- xUnit + FluentAssertions + Moq (unit/integration)
- Playwright + Testcontainers (E2E)
- Docker Compose + GitLab CI/CD

## Database

- Single PostgreSQL instance, **one schema per module**: `users.*`, `marketplace.*`, `bookings.*`, `messaging.*`
- Each module has its own `DbContext` with `HasDefaultSchema("module_name")`
- Migrations per module: always specify `--context` flag
- Seeded data: Estonian counties/municipalities (EHAK codes), service categories

## Conventions

### Code Style

- One function = one job. Extract, don't nest.
- Functions < 50 lines, files < 800 lines
- `record` for immutable DTOs, `class` for entities with lifecycle
- All implementation classes `internal` unless in a Contracts project
- `sealed` on non-inherited classes
- `CancellationToken` on all public async APIs
- No `ViewBag`/`ViewData` in MVC — use ViewModels exclusively
- Run `dotnet format` before committing

### Git

- Conventional commits: `feat:`, `fix:`, `docs:`, `refactor:`, `test:`, `chore:`
- Subject lines < 72 chars, English, imperative mood
- Run `dotnet build` before committing
- One logical change per commit

### Testing

- Minimum 80% coverage
- AAA pattern (Arrange-Act-Assert)
- Naming: `MethodName_Scenario_ExpectedResult`
- Unit tests mock cross-module dependencies via Contracts interfaces
- Integration tests use `WebApplicationFactory`
- E2E tests use Playwright + Testcontainers (real PostgreSQL)

## Key Decisions

- MVC Web shares BLL services with API — no direct DbContext access from controllers
- IDOR protection: all API controllers verify resource ownership via JWT claims (`sub`, `profileId`, `role`)
- i18n: UI via `.resx` files (en, et)
- Admin area: full CRUD, protected by `AdminOnly` policy
- Real-time: SignalR MessageHub with JWT auth via query string

## What NOT to Do

- Do NOT create cross-module entity references (use DTOs from Contracts)
- Do NOT add `public` to implementation classes inside modules
- Do NOT query another module's DbContext directly
- Do NOT use `ViewBag`/`ViewData` — use ViewModels
- Do NOT skip `dotnet build` verification before commits

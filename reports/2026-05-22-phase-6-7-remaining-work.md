# Phase 6 & 7 — Remaining Work Plan

**Date:** 2026-05-22
**Change:** `modular-monolith-refactor`
**Status of this document:** handoff plan for the un-started phases.

## Where the refactoring stands

Phases 1–5 are complete and committed (25 commits this session). The four
domain modules — Users, Marketplace, Bookings, Messaging — plus the
`AgriMarket.Shared` kernel exist as **additive** projects under `src/`:

```
src/Shared/AgriMarket.Shared
src/Modules/Users/{AgriMarket.Modules.Users, .Contracts}
src/Modules/Marketplace/{AgriMarket.Modules.Marketplace, .Contracts}
src/Modules/Bookings/{AgriMarket.Modules.Bookings, .Contracts}
src/Modules/Messaging/{AgriMarket.Modules.Messaging, .Contracts}
```

Each module has its entities (cross-module navigations stripped to loose
`Guid` FKs), its schema-scoped `DbContext`, its repositories, its
cleanly-extractable services, an `IModule` registration class, and a
`.Contracts` project. `dotnet build AgriMarket.slnx` succeeds with 0 errors;
all 238 unit/integration tests pass. The legacy `AgriMarket.Domain` / `DAL` /
`BLL` / `Api` / `Web` projects are **untouched and fully functional** — the
application still runs entirely on the legacy code. Nothing is wired to the
new modules yet.

Phases 6 and 7 are the **destructive switchover**: they replace the legacy
code paths with the modules and then delete the legacy projects. They were not
started because a partial switchover leaves the build and the test suite
broken — strictly worse than the current green, additive state.

## Deferred service extractions (must precede the switchover)

Nine services could not be extracted during Phases 2–5 because they cross
module boundaries. Each must be extracted into its owning module with the
cross-module access rewritten to go through `.Contracts` interfaces or MediatR
events. All of this is still **additive** (legacy untouched) and can be done
keeping the build green.

| Service | Owning module | Cross-module dependency | Extraction approach |
|---|---|---|---|
| `UserService` | Users | `Booking`, `Review`, `Message`, `ServiceListing` repos; `IReviewService` | `DeleteUserAsync` cascade → publish a new `UserDeletedEvent` (Users.Contracts) handled by Marketplace/Bookings/Messaging; profile rating enrichment → a new review-stats method on `IBookingsModule` |
| `ListingService` | Marketplace | `IRepository<UserProfile>`, `IRepository<Booking>`, `IReviewService`, `BookingStatus` | provider name → `IUsersModule.GetProfilesAsync`; active-booking guard → new `IBookingsModule.HasActiveBookingsAsync(listingId)`; ratings → `IBookingsModule` review-stats |
| `BookingService` | Bookings | `ServiceListing`, `Availability`, `UserProfile` | listing price/availability → `ICatalogModule`; client/provider names → `IUsersModule`; publish `BookingConfirmedEvent` on confirmation |
| `ClientPaymentService` | Bookings | listing/profile for receipts | `ICatalogModule` + `IUsersModule` |
| `ReviewService` | Bookings | reviewer/reviewed `UserProfile` for `ReviewDto` names | `IUsersModule.GetProfilesAsync`; rating-stats methods are already in-module (Review→Booking→ServiceListingId) |
| `DashboardService` | Bookings | user counts, listing counts | admin dashboard — aggregate via `IUsersModule` + `ICatalogModule` count methods (add them to the contracts) |
| `ProviderDashboardService` | Bookings | listings + profiles | `ICatalogModule` + `IUsersModule` |
| `MessagingService` | Messaging | participant/sender names | `EfConversationRepository` returns DTOs with empty names; `MessagingService` batch-resolves via `IUsersModule.GetProfilesAsync` |
| `EfConversationRepository` | Messaging | `UserProfile` name projections | drop the `.FirstName + .LastName` projections; return `SenderProfileId`/`ProfileId` only — names filled by the service |

New `.Contracts` surface needed: `UserDeletedEvent`; `IBookingsModule` —
`HasActiveBookingsAsync`, review-stats (`GetListingRatingAsync`,
`GetProfileRatingAsync`), count methods; `ICatalogModule` — count methods,
`GetEquipmentAsync`; `IMessagingModule` adapter implementation;
`IMessageNotifier` (move to Messaging.Contracts). Module cores gain
`.Contracts` references for the modules they query (e.g. Bookings core →
Users.Contracts + Marketplace.Contracts).

## Phase 6 — composition and switchover (blocks 19–22)

**19. Bootstrapper composition.** Move `AgriMarket.Api` to
`src/Bootstrapper/AgriMarket.Api`; reference the 4 module cores + 4
`.Contracts` + `AgriMarket.Shared`. Rewrite `Program.cs`: build the ordered
module list (`new UsersModule()`, `MarketplaceModule`, `BookingsModule`,
`MessagingModule`), call `RegisterServices` then `MapEndpoints` on each;
register MediatR across all module assemblies
(`AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(...))`); preserve JWT,
SignalR, Swagger, API versioning, CORS, `ProblemDetails`. Move the 13 API
controllers + the SignalR hub into their owning modules and register each
module assembly as an MVC application part
(`AddControllers().AddApplicationPart(typeof(UsersModule).Assembly)` …).
Apply all 4 module migrations on startup; run each module's seeder.

**20. Web switchover.** Move `AgriMarket.Web` to `src/AgriMarket.Web`;
replace its `BLL`/`DAL` references with module core + `.Contracts`
references. Update the 16 MVC controllers, ViewComponents and Web services to
consume module `.Contracts` interfaces (or the module services where the Web
shares the BLL — note CLAUDE.md "MVC Web shares BLL services with API").

**21. Migrations per module.** `dotnet ef migrations add InitialCreate
--context UsersDbContext --project src/Modules/Users/AgriMarket.Modules.Users`
(repeat for Marketplace/Bookings/Messaging). Remove `AgriMarket.DAL/Migrations`.
Each module already sets `MigrationsHistoryTable("__EFMigrationsHistory",
"<schema>")` in its `IModule.RegisterServices`, so the four migration sets are
schema-isolated. Verify against a fresh database.

**22. Test rewiring.** Update `AgriMarket.Tests` `ProjectReference`s from
`BLL`/`DAL` to the module cores; switch cross-module mocks to `.Contracts`
interfaces. Each module core needs
`[assembly: InternalsVisibleTo("AgriMarket.Tests")]` (deferred in tasks 4.4 /
8.5 / 12.4 / 16.4). Update `AgriMarket.E2E` `InternalsVisibleTo` targets.
Run `dotnet test` — all 238 unit/integration + 56 E2E tests must pass.

## Phase 7 — cleanup (blocks 23–25)

Delete `AgriMarket.Domain` / `AgriMarket.DAL` / `AgriMarket.BLL` and remove
them from `AgriMarket.slnx`. Update `Directory.Build.props`,
`docker-compose.yml` (build context/paths) and `.gitlab-ci.yml`. Final
verification: `dotnet build`, `dotnet test` (both suites), `dotnet format`.

## Risk notes

- **Do the deferred service extractions additively first** (build stays
  green), and only then perform the blocks 19–22 switchover as one coherent
  unit — the switchover itself cannot be partially committed without breaking
  the build.
- Controller discovery for `internal` controllers needs MVC application parts;
  if discovery proves fragile, keep controllers `public` inside the module
  (design.md decision 8 fallback).
- The four per-module migrations are generated fresh; a data-bearing database
  would need a hand-written schema-move script (out of scope — dev databases
  are recreated from compose).

# Project Status

## Current Phase
Phases 1–5 complete. **STEP 1 of Phase 6 (deferred service extractions) is
complete.** The destructive switchover (Phase 6 blocks 19–22) and Phase 7 are
not started — see "Remaining Work".

## Session Outcome (2026-05-22, continued)
The nine deferred cross-module services have been **extracted into their owning
modules, additively** — the legacy `AgriMarket.Domain`/`DAL`/`BLL`/`Api`/`Web`
projects are untouched and still run the application. `dotnet build
AgriMarket.slnx` → **0 errors**; **238/238 unit/integration tests pass**.

Extractions completed this session (7 commits):
- **Contracts surface expanded** — `UserDeletedEvent`, `ListingsDeletedEvent`,
  `RatingStatsDto`; `IBookingsModule` gained `HasActiveBookingsAsync` +
  listing/profile rating queries; `ICatalogModule` gained `CountListingsAsync`,
  `GetListingsByProviderAsync`, `TryReserveAvailabilityAsync`; `IUsersModule`
  gained `CountUsersAsync`; `IMessageNotifier` + `MessageNotificationDto` added
  to Messaging.Contracts.
- **Marketplace** — `ListingService` extracted; provider names via
  `IUsersModule`, ratings + active-booking guard via `IBookingsModule`.
- **Bookings** — `BookingService`, `ClientPaymentService`, `ReviewService`,
  `DashboardService`, `ProviderDashboardService` extracted; listing/availability
  data via `ICatalogModule`, names/counts via `IUsersModule`. `BookingService`
  and `ClientPaymentService` publish `BookingConfirmedEvent`. `UserDeletedEvent`
  and `ListingsDeletedEvent` handlers cascade-delete Bookings-owned data.
- **Users** — `UserService` extracted; ratings via `IBookingsModule`;
  `DeleteUserAsync` publishes `UserDeletedEvent`.
- **Messaging** — `MessagingService` + `EfConversationRepository` extracted;
  participant names via `IUsersModule`; `IMessagingModule` adapter implemented;
  `UserDeletedEvent` handler + `BookingConfirmedEvent` handler (opens a
  client/provider conversation) added.

Cross-module access goes exclusively through `.Contracts` interfaces and MediatR
events. Module write methods now take `UserProfile` ids directly (AppUser→Profile
resolution moves to the controller layer in the switchover). Module cores
reference only other modules' `.Contracts` projects — no core-to-core references.

## Completed
- [x] Phases 1–5: Shared kernel + 4 module pairs (core + `.Contracts`)
- [x] Phase 6 STEP 1: all 9 deferred cross-module service extractions (additive)

## Remaining Work (Phase 6 switchover + Phase 7 — not started)
Full plan unchanged: `reports/2026-05-22-phase-6-7-remaining-work.md`.

The destructive switchover was **not started**: blocks 19–22 cannot be
partially committed without breaking the build and the 238-test suite, which
violates the "every commit stays green" constraint. It must be done as one
coherent unit:
- **Block 19** — move `AgriMarket.Api` to `src/Bootstrapper`, rewrite
  `Program.cs` for `IModule` composition + MediatR + module MVC application
  parts; move the 19 API controllers + SignalR hub into their modules; provide a
  SignalR-backed `IMessageNotifier`; apply per-module migrations on startup.
- **Block 20** — move `AgriMarket.Web` to `src/`, rewire its 16 MVC controllers
  / ViewComponents / mappers onto the module services + `.Contracts`.
- **Block 21** — generate 4 per-module EF migrations (needs an
  `IDesignTimeDbContextFactory` per module DbContext, since the cores are class
  libraries with `internal` contexts); drop `AgriMarket.DAL/Migrations`.
- **Block 22** — repoint `AgriMarket.Tests` / `AgriMarket.E2E` from `BLL`/`DAL`
  to the module cores; add `[assembly: InternalsVisibleTo]` to each module core;
  switch cross-module mocks to `.Contracts` interfaces.

Then Phase 7 (blocks 23–25): delete `Domain`/`DAL`/`BLL`, update
`AgriMarket.slnx`, `docker-compose.yml`, `.gitlab-ci.yml`; final verification.

## Notes for the switchover
- Module services/DTOs are `internal`; the API controllers move INTO the modules
  (same assembly — fine). `AgriMarket.Web` is a separate assembly: it must
  consume modules through `.Contracts`, or the module service interfaces it
  needs must be exposed (revisit CLAUDE.md "MVC Web shares BLL services").
- The extracted module DTOs are faithful ports of the legacy BLL DTO shapes, so
  controller response contracts are preserved — switchover wiring is mechanical
  for serialization but each controller still needs its service-call sites and
  AppUser→Profile resolution updated.

## Approach Note
Refactoring proceeds additively until the switchover. Every commit so far leaves
the solution green (0 build errors, 238/238 tests). E2E tests not re-run this
session (require Docker/Testcontainers).

## Last Updated
2026-05-22

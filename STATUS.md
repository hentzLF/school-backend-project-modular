# Project Status

## Current Phase
Phase 5 (starting): Messaging module.

## Completed
- [x] Phase 0: AI workflow setup (agents, rules, skills, settings, devcontainer)
- [x] Baseline verified: solution builds clean (0 errors)
- [x] Cleared root-owned bin/obj artifacts (fixed MSB3374 permission errors)
- [x] OpenSpec change `modular-monolith-refactor` created (25 task blocks, 7 phases)
- [x] Phase 1: Shared infrastructure — `AgriMarket.Shared` (IModule, EntityBase,
      IRepository/IUnitOfWork/IQueryMaterializer, IntegrationEvent, exceptions)
- [x] Phase 2: Users module — Contracts (IUsersModule), entities, UsersDbContext
      (schema `users`), repositories, BCrypt hasher, Token/Auth services,
      UsersModuleApi, `UsersModule : IModule`
- [x] Phase 3: Marketplace module — Contracts (ICatalogModule), 8 entities,
      MarketplaceDbContext (schema `marketplace`) + EHAK seeding, repositories,
      Category/Equipment/LocationLookup services, CatalogModuleApi,
      `MarketplaceModule : IModule`
- [x] Phase 4: Bookings module — Contracts (IBookingsModule, BookingConfirmedEvent),
      entities (Booking/Payment/Review), BookingsDbContext (schema `bookings`),
      repositories, PaymentService, BookingsModuleApi, `BookingsModule : IModule`

## In Progress
- [ ] Phase 5: Messaging module
- [ ] Phase 6: MediatR integration events + composition
- [ ] Phase 7: Cleanup old layered projects, update Docker/CI

## Deferred to Phase 6 (cross-module rework / composition)
- `UserService` (Users) — depends on Bookings/Marketplace/Messaging types and
  `IReviewService`; needs a `UserDeletedEvent` integration event + review-stats
  contract.
- `ListingService` (Marketplace) — depends on `IRepository<UserProfile>`,
  `IRepository<Booking>`, `IReviewService`, `BookingStatus`.
- `BookingService`, `ClientPaymentService`, `ReviewService`, `DashboardService`,
  `ProviderDashboardService` (Bookings) — depend on Users/Marketplace types and
  services; `BookingConfirmedEvent` is published once `BookingService` lands.
- Moving API/MVC controllers into modules — coupled to the bootstrapper's MVC
  application-part + API-versioning wiring.

## Blocked
- **orchestrator agent unspawnable** (2026-05-22): 5 consecutive `API Error: 529
  Overloaded` over ~35 min spawning the Opus-model orchestrator subagent; it
  never reached its first inference. The top-level agent assumes the
  orchestrator's coordinator role (plan + delegate, no code written by the
  coordinator); the Sonnet `coder` and the reviewer subagents spawn fine, so the
  structured workflow (OpenSpec change -> tasks -> build -> review -> fix ->
  commit -> report) is preserved.

## Approach Note
Refactoring proceeds **additively**: new module projects are built alongside the
legacy `AgriMarket.Domain`/`DAL`/`BLL`/`Api`/`Web` projects, which stay
untouched and keep building. Every commit leaves the solution green and the
legacy application fully working. The legacy → modular switchover happens in
Phase 6; legacy projects are removed only in Phase 7. All 84 tests continue to
pass throughout (they exercise the still-present legacy code).

## Last Updated
2026-05-22

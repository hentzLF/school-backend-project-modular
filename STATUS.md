# Project Status

## Current Phase
Phases 1–5 complete (additive modular skeleton). Phases 6–7 not started —
see "Remaining Work" below.

## Session Outcome (2026-05-22)
The modular-monolith **skeleton is complete and committed** (25 commits):
`AgriMarket.Shared` + four module pairs (core + `.Contracts`) for Users,
Marketplace, Bookings, Messaging. `dotnet build AgriMarket.slnx` → **0 errors**;
**238/238 unit/integration tests pass**. The work was done additively, so the
legacy `AgriMarket.Domain`/`DAL`/`BLL`/`Api`/`Web` projects are untouched and
the application still runs fully on the legacy code.

Phases 6 (cross-module service rewrite + legacy→modular switchover) and 7
(delete legacy projects, update Docker/CI) were **not started**: the switchover
is destructive and cannot be partially committed without breaking the build and
the test suite, which would violate the "everything keeps working" constraint.
The remaining work is fully specified in
`reports/2026-05-22-phase-6-7-remaining-work.md`.

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
- [x] Phase 5: Messaging module — Contracts (IMessagingModule), entities
      (Conversation/ConversationParticipant/Message/MessageRead), MessagingDbContext
      (schema `messaging`), persistence plumbing, MessagingModuleApi,
      `MessagingModule : IModule`

## Remaining Work (Phases 6–7 — not started)
Full plan: `reports/2026-05-22-phase-6-7-remaining-work.md`.

Nine services were deferred during Phases 2–5 because they cross module
boundaries; they must be extracted with cross-module access rewritten through
`.Contracts` interfaces / MediatR events before the switchover:
- `UserService` (Users) — needs a `UserDeletedEvent` integration event +
  review-stats contract.
- `ListingService` (Marketplace) — needs `IUsersModule` +
  `IBookingsModule.HasActiveBookingsAsync` + review-stats.
- `BookingService`, `ClientPaymentService`, `ReviewService`, `DashboardService`,
  `ProviderDashboardService` (Bookings) — need `ICatalogModule` + `IUsersModule`;
  `BookingService` publishes `BookingConfirmedEvent`.
- `MessagingService` + `EfConversationRepository` (Messaging) — need
  `IUsersModule` batch name resolution; a `BookingConfirmedEvent` handler
  creates the conversation.

Then the destructive switchover (blocks 19–22): rewrite `Program.cs` for
`IModule` composition + MediatR, move the 29 controllers + SignalR hub into
modules, rewire `AgriMarket.Web`, split EF migrations per module, rewire the
test projects. Phase 7 (blocks 23–25): delete the legacy projects, update
`AgriMarket.slnx` / `docker-compose.yml` / `.gitlab-ci.yml`, final verification.

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
Phase 6; legacy projects are removed only in Phase 7. The 238 unit/integration
tests pass throughout (they exercise the still-present legacy code); E2E tests
were not re-run this session (they require Docker/Testcontainers).

## Last Updated
2026-05-22


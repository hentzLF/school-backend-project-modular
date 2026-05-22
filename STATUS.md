# Project Status

## Current Phase
Phase 3 (starting): Marketplace module.

## Completed
- [x] Phase 0: AI workflow setup (agents, rules, skills, settings, devcontainer)
- [x] Baseline verified: solution builds clean (0 errors, 17 warnings)
- [x] Cleared root-owned bin/obj artifacts (fixed MSB3374 permission errors)
- [x] OpenSpec change `modular-monolith-refactor` created (25 task blocks, 7 phases)
- [x] Phase 1: Shared infrastructure — `AgriMarket.Shared` (IModule, EntityBase,
      IRepository/IUnitOfWork, IntegrationEvent, exceptions)
- [x] Phase 2: Users module — Contracts (IUsersModule, UserProfileDto), entities
      (AppUser, UserProfile, UserRole, RefreshToken), UsersDbContext (schema `users`),
      repositories, BCryptPasswordHasher, TokenService, AuthService, UsersModuleApi,
      `UsersModule : IModule`; code review cycle completed; build green

## In Progress
- [ ] Phase 3: Marketplace module
- [ ] Phase 4: Bookings module
- [ ] Phase 5: Messaging module
- [ ] Phase 6: MediatR integration events + composition
- [ ] Phase 7: Cleanup old layered projects, update Docker/CI

## Deferred to Phase 6 (cross-module rework / composition)
- `UserService` extraction — depends on Bookings/Marketplace/Messaging types and
  `IReviewService`; needs a `UserDeletedEvent` integration event + review-stats
  contract.
- Moving API controllers into modules — coupled to the bootstrapper's MVC
  application-part + API-versioning wiring.

## Blocked
- **orchestrator agent unspawnable** (2026-05-22): 5 consecutive `API Error: 529
  Overloaded` over ~35 min when spawning the Opus-model orchestrator subagent;
  it never reached its first inference (0 tokens, 0 tool uses each time). A
  deliberate 10-min wait did not clear it. The top-level agent's own inference
  works normally; the Sonnet `coder` and reviewer subagents spawn fine.
  **Mitigation:** the top-level agent assumes the orchestrator's coordinator
  role (plan + delegate, no code written by the coordinator). Code work is
  delegated to `coder` and reviewed by `code-reviewer`/`csharp-reviewer`. The
  structured workflow (OpenSpec change -> tasks -> build -> review -> fix ->
  commit -> report) is preserved.

## Approach Note
The refactoring proceeds **additively**: new module projects are built alongside
the legacy `AgriMarket.Domain`/`DAL`/`BLL`/`Api`/`Web` projects, which stay
untouched and keep building. Every commit leaves the solution green and the
legacy application fully working. The legacy → modular switchover happens in
Phase 6; legacy projects are removed only in Phase 7.

## Last Updated
2026-05-22 13:45 UTC

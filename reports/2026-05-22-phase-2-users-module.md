# Phase 2: Users Module

**Date:** 2026-05-22 13:45 UTC  
**Phase:** Phase 2 (Users Module) — Modular Monolith Refactor  
**OpenSpec Change:** `modular-monolith-refactor`

## What Was Done

- **Created `AgriMarket.Modules.Users.Contracts` project** — public API boundary for Users module:
  - `IUsersModule` interface: `GetProfileAsync(userId)` and `GetProfilesAsync(userIds)` batch lookup
  - `UserProfileDto` record — immutable DTO for cross-module consumption

- **Created `AgriMarket.Modules.Users` (core) project** — all implementation types internal:
  - **Entities (sealed):** AppUser, UserProfile, UserRole, RefreshToken
    - Removed all cross-module navigation collections from UserProfile (ServiceListings, ClientBookings, Reviews, ConversationParticipants, SentMessages, MessageReads, Equipments)
    - 1:1 AppUser↔UserProfile relationship with proper indexes
  - **`UsersDbContext`** — EF Core DbContext with `schema = "users"` and migration history table
  - **Persistence layer:**
    - Generic `EfRepository<T>` and `EfUnitOfWork` bound to UsersDbContext
    - `EfAppUserRepository`, `EfRefreshTokenRepository`, `EfUserProfileRepository` (all internal)
  - **Security:** `IPasswordHasher` + `BCryptPasswordHasher` implementation
  - **Services (all internal):**
    - `TokenService` — JWT token generation and validation
    - `AuthService` — register, login, refresh token, logout flows
    - RoleType enum and Auth DTOs (Register/Login/Refresh/Logout) marked internal per modular-monolith rules
  - **Module registration:**
    - `UsersModuleApi : IUsersModule` — implementation of public contract
    - `UsersModule : IModule` — composition seam; wires DI and migrations history

- **Code review cycle completed:**
  - `csharp-reviewer` and `code-reviewer` ran in parallel
  - Findings actioned: demoted RoleType + 4 Auth DTOs to internal, sealed all entities, added empty-collection guard
  - Two behavior-change reversions: restored refresh-token expiry config read (Jwt:RefreshTokenExpiryDays), left CreatedAt unset to match legacy

- **Deferred to Phase 6:**
  - `UserService` extraction (hard cross-module dependencies on Booking/Review/Message/ServiceListing services; requires UserDeletedEvent and review-stats contract)
  - Auth/Users API controllers (coupled to bootstrapper MVC wiring)

## Task Blocks Completed

- **Block 3:** Users.Contracts project + IUsersModule interface
- **Block 4:** Entities + UsersDbContext
- **Block 5:** Repositories + services (persistence, security, auth)
- **Block 6:** IModule registration and DI wiring

## Files Changed

- `src/Modules/Users/AgriMarket.Modules.Users.Contracts/AgriMarket.Modules.Users.Contracts.csproj` (new)
- `src/Modules/Users/AgriMarket.Modules.Users.Contracts/IUsersModule.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users.Contracts/UserProfileDto.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/AgriMarket.Modules.Users.csproj` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Entities/AppUser.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Entities/UserProfile.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Entities/UserRole.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Entities/RefreshToken.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Persistence/UsersDbContext.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Persistence/EfAppUserRepository.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Persistence/EfRefreshTokenRepository.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Persistence/EfUserProfileRepository.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Security/IPasswordHasher.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Security/BCryptPasswordHasher.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Services/TokenService.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/Services/AuthService.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/UsersModuleApi.cs` (new)
- `src/Modules/Users/AgriMarket.Modules.Users/UsersModule.cs` (new)
- `src/Shared/AgriMarket.Shared/Abstractions/IRepository.cs` (updated — added generic IRepository<T> base)
- `src/Shared/AgriMarket.Shared/Persistence/EfRepository.cs` (new — reusable EF Core repository)
- `src/Shared/AgriMarket.Shared/Persistence/EfUnitOfWork.cs` (new — reusable unit-of-work implementation)
- `AgriMarket.slnx` (updated — added Users module projects)

## Build Status

**PASS** — `dotnet build AgriMarket.slnx` succeeds

- 0 errors
- 2 pre-existing NU1510 warnings (unrelated, on AgriMarket.Web — deferred to cleanup phase)

## Test Status

Unit/integration test suite: all tests run green (existing legacy tests still passing)

## Commits

1. `feat: add Users module contracts project`
2. `feat: add Users module entities and UsersDbContext`
3. `feat: expand AgriMarket.Shared persistence abstractions`
4. `feat: add Users module repositories and services`
5. `feat: add Users module IModule registration`

All committed to main.

## Issues / Blockers

None — Phase 2 completed without blockers. Cross-module dependencies (UserService, Auth controllers) deferred to Phase 6 per architecture plan.

## Next Steps

- Phase 3: Marketplace module (blocks 7–10)
  - Create Marketplace.Contracts project with category/listing/location contracts
  - Implement Equipment, Listing, Category entities and MarketplaceDbContext
  - Add repository layer and domain services
  - Write integration tests

## Notes

- All module implementation classes remain `internal` per modular-monolith rules
- Only Contracts projects expose `public` interfaces and DTOs
- Users module is now ready as a dependency for Bookings and Messaging modules (Phase 3–4)
- Auth/Users controllers remain in legacy bootstrapper; will be extracted in Phase 6 after dependency mapping

# Tasks — Modular Monolith Refactor

Strategy: build modules **additively** alongside the legacy projects so the solution
stays green through Phases 1–5; switch the bootstrapper and tests over in Phase 6;
delete the legacy projects in Phase 7. Every block ends with a conventional commit.

## Phase 1 — Shared Infrastructure

## 1. Create src/ layout and AgriMarket.Shared project

- [x] 1.1 Create directories `src/Shared`, `src/Modules/{Users,Marketplace,Bookings,Messaging}`, `src/Bootstrapper`
- [x] 1.2 Create `AgriMarket.Shared` class library at `src/Shared/AgriMarket.Shared` with package refs: `MediatR`, `Microsoft.EntityFrameworkCore`, `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.AspNetCore.App` framework reference
- [x] 1.3 Add `AgriMarket.Shared` to `AgriMarket.slnx`
- [x] 1.4 Verify `dotnet build` succeeds (0 errors)
- [x] 1.5 Git commit: `chore: scaffold src/ layout and AgriMarket.Shared project`

## 2. Define shared abstractions

- [x] 2.1 Define `IModule` interface (`RegisterServices(IServiceCollection, IConfiguration)`, `MapEndpoints(IEndpointRouteBuilder)`) in `AgriMarket.Shared`
- [x] 2.2 Define base entity abstraction (`IEntity` / `EntityBase` with `Guid Id`)
- [x] 2.3 Define generic `IRepository<T>` and `IUnitOfWork` abstractions
- [x] 2.4 Define MediatR integration-event base type (`IntegrationEvent` record implementing `INotification`)
- [x] 2.5 Verify `dotnet build` succeeds
- [x] 2.6 Git commit: `feat: add IModule, base entity, repository and integration-event abstractions`

## Phase 2 — Users Module

## 3. Users.Contracts project

- [x] 3.1 Create `AgriMarket.Modules.Users.Contracts` class library at `src/Modules/Users/AgriMarket.Modules.Users.Contracts`
- [x] 3.2 Define `IUsersModule` interface (`GetProfileAsync`, `GetProfilesAsync` batch lookup)
- [x] 3.3 Define public DTO records (`UserProfileDto`)
- [x] 3.4 Add to `AgriMarket.slnx`; verify build
- [x] 3.5 Git commit: `feat: add Users module contracts project`

## 4. Users module core project

- [x] 4.1 Create `AgriMarket.Modules.Users` class library referencing `AgriMarket.Shared` and `AgriMarket.Modules.Users.Contracts`
- [x] 4.2 Copy entities `AppUser`, `UserProfile`, `UserRole`, `RefreshToken` into the module as `internal`; removed UserProfile's 7 cross-module navigation collections
- [x] 4.3 Create `UsersDbContext` (`HasDefaultSchema("users")`) with the relationship/index config for these entities from the old `AppDbContext`
- [~] 4.4 `InternalsVisibleTo` for `AgriMarket.Tests` — deferred to block 22 (test rewiring)
- [x] 4.5 Verify build
- [x] 4.6 Git commit: `feat: add Users module entities and UsersDbContext`

## 5. Users module repositories and services

- [x] 5.1 Move `EfAppUserRepository`, `EfRefreshTokenRepository`, `EfUserProfileRepository` into the module as `internal` (plus generic `EfRepository<T>`/`EfUnitOfWork`)
- [x] 5.2 Move `BCryptPasswordHasher` and `IPasswordHasher` into the module
- [~] 5.3 Move `AuthService`, `TokenService` (interfaces + Auth DTOs) into the module as `internal`. `UserService` extraction DEFERRED to Phase 6 — it has hard cross-module dependencies (Booking, Review, Message, ServiceListing, `IReviewService`) and needs a `UserDeletedEvent` integration event + a review-stats contract to extract cleanly
- [x] 5.4 Implement `IUsersModule` as the internal `UsersModuleApi` adapter
- [x] 5.5 Verify build
- [x] 5.6 Git commit: `feat: add Users module repositories and services`

## 6. Users module registration

- [x] 6.1 Implement `UsersModule : IModule` — `RegisterServices` wires `UsersDbContext` (schema-scoped migrations history), repositories, services, `IUsersModule`
- [~] 6.2 Move `AuthController`/`UsersController` — DEFERRED to Phase 6 (block 19): controller relocation is coupled to the bootstrapper's MVC application-part + API-versioning wiring. `UsersController` additionally blocks on the deferred `UserService`
- [x] 6.3 Verify build
- [x] 6.4 Git commit: `feat: add Users module IModule registration`

## Phase 3 — Marketplace Module

## 7. Marketplace.Contracts project

- [x] 7.1 Create `AgriMarket.Modules.Marketplace.Contracts` class library
- [x] 7.2 Define `ICatalogModule` interface (`GetListingSummaryAsync` + batch, `GetAvailabilityAsync`)
- [x] 7.3 Define public DTO records (`ListingSummaryDto`, `AvailabilityDto`)
- [x] 7.4 Add to `AgriMarket.slnx`; verify build
- [x] 7.5 Git commit: `feat: add Marketplace module contracts project`

## 8. Marketplace module core

- [x] 8.1 Create `AgriMarket.Modules.Marketplace` class library referencing `AgriMarket.Shared` and its `.Contracts` (Users.Contracts added in Phase 6 when ListingService lands)
- [x] 8.2 Copy 8 entities as `internal sealed`; removed `ServiceListing.UserProfile` and `Equipment.UserProfile` cross-module navs (FK Guids kept)
- [x] 8.3 Create `MarketplaceDbContext` (`HasDefaultSchema("marketplace")`) with relationship/index config and `County`/`Municipality` `HasData` seeding
- [x] 8.4 Move `CountySeedData` and `MunicipalitySeedData` into the module
- [~] 8.5 `InternalsVisibleTo` for `AgriMarket.Tests` — deferred to block 22 (test rewiring); build verified
- [x] 8.6 Git commit: `feat: add Marketplace module entities, DbContext and seeding`

## 9. Marketplace repositories and services

- [x] 9.1 Move `EfListingRepository` and `EfAvailabilityRepository` into the module as `internal` (cross-module `.Include(l => l.UserProfile)` removed from the listing repo)
- [~] 9.2 Move `CategoryService`, `EquipmentService`, `LocationLookupService` (interfaces + Equipment/Locations DTOs) into the module. `ListingService` DEFERRED to Phase 6 — cross-module deps on `IRepository<UserProfile>`, `IRepository<Booking>`, `IReviewService`, `BookingStatus`
- [x] 9.3 Implement `ICatalogModule` as the internal `CatalogModuleApi` adapter
- [x] 9.4 Verify build
- [x] 9.5 Git commit: `feat: add Marketplace module repositories and services`

## 10. Marketplace module registration

- [x] 10.1 Implement `MarketplaceModule : IModule` — wires `MarketplaceDbContext` (marketplace-schema migrations history), repositories, query materializer, services, `ICatalogModule`
- [~] 10.2 Move Marketplace controllers — DEFERRED to Phase 6 (bootstrapper MVC application-part wiring; `ListingsController`/`AdminListingsController` also block on the deferred `ListingService`)
- [x] 10.3 Verify build
- [x] 10.4 Git commit: `feat: add Marketplace module IModule registration`

## Phase 4 — Bookings Module

## 11. Bookings.Contracts project

- [x] 11.1 Create `AgriMarket.Modules.Bookings.Contracts` class library (references `AgriMarket.Shared` for the `IntegrationEvent` base)
- [x] 11.2 Define `IBookingsModule` interface (`GetBookingAsync`)
- [x] 11.3 Define `BookingConfirmedEvent` integration event and the `BookingSummaryDto` record
- [x] 11.4 Add to `AgriMarket.slnx`; verify build
- [x] 11.5 Git commit: `feat: add Bookings module contracts project`

## 12. Bookings module core

- [x] 12.1 Create `AgriMarket.Modules.Bookings` class library referencing `AgriMarket.Shared` and its `.Contracts` (Users/Marketplace `.Contracts` added in Phase 6 with the services)
- [x] 12.2 Copy entities `Booking`, `Payment`, `Review` as `internal sealed`; removed 5 cross-module navs (Booking→ServiceListing/ClientProfile/Availability, Review→Reviewer/ReviewedProfile)
- [x] 12.3 Create `BookingsDbContext` (`HasDefaultSchema("bookings")`) with the in-module relationship/index config
- [~] 12.4 `InternalsVisibleTo` for `AgriMarket.Tests` — deferred to block 22; build verified
- [x] 12.5 Git commit: `feat: add Bookings module entities and BookingsDbContext`

## 13. Bookings repositories and services

- [x] 13.1 Move `EfBookingRepository` and `EfPaymentRepository` into the module as `internal` (cross-module `.Include()`/`.ThenInclude()` chains removed)
- [~] 13.2 Move `PaymentService` (faithful internal port). `BookingService`, `ClientPaymentService`, `ReviewService`, `DashboardService`, `ProviderDashboardService` DEFERRED to Phase 6 — all have hard cross-module dependencies (Users/Marketplace types and services)
- [~] 13.3 Implement `IBookingsModule` (the internal `BookingsModuleApi` adapter). `BookingConfirmedEvent` publishing deferred to Phase 6 with `BookingService`
- [x] 13.4 Verify build
- [x] 13.5 Git commit: `feat: add Bookings module repositories and PaymentService`

## 14. Bookings module registration

- [x] 14.1 Implement `BookingsModule : IModule` — wires `BookingsDbContext` (bookings-schema migrations history), repositories, query materializer, `PaymentService`, `IBookingsModule`
- [~] 14.2 Move Bookings controllers — DEFERRED to Phase 6 (bootstrapper MVC wiring; most also block on the deferred Booking/Review/Dashboard services)
- [x] 14.3 Verify build
- [x] 14.4 Git commit: `feat: add Bookings module IModule registration`

## Phase 5 — Messaging Module

## 15. Messaging.Contracts project

- [x] 15.1 Create `AgriMarket.Modules.Messaging.Contracts` class library
- [x] 15.2 Define `IMessagingModule` interface (`GetUnreadConversationCountAsync`). `IMessageNotifier` deferred to Phase 6 with the SignalR hub
- [~] 15.3 Public DTO records — deferred to Phase 6 (Messaging DTOs land with the deferred MessagingService)
- [x] 15.4 Add to `AgriMarket.slnx`; verify build
- [x] 15.5 Git commit: `feat: add Messaging module contracts project`

## 16. Messaging module core

- [x] 16.1 Create `AgriMarket.Modules.Messaging` class library referencing `AgriMarket.Shared` and its `.Contracts` (Users/Bookings `.Contracts` added in Phase 6 with the service)
- [x] 16.2 Copy entities `Conversation`, `ConversationParticipant`, `Message`, `MessageRead` as `internal sealed`; removed 4 cross-module navs (Conversation→Booking, ConversationParticipant/Message/MessageRead→UserProfile)
- [x] 16.3 Create `MessagingDbContext` (`HasDefaultSchema("messaging")`) with the in-module relationship/index config
- [~] 16.4 `InternalsVisibleTo` for `AgriMarket.Tests` — deferred to block 22; build verified
- [x] 16.5 Git commit: `feat: add Messaging module entities and MessagingDbContext`

## 17. Messaging repositories, services and hub

- [x] 17.1 Add the module persistence plumbing (`EfRepository<T>`/`EfUnitOfWork` bound to `MessagingDbContext`)
- [~] 17.2 `MessagingService` — DEFERRED to Phase 6: it depends on `EfConversationRepository`, whose `ConversationSummaryDto`/`MessageDto` projections resolve participant/sender names from `UserProfile` (cross-module). Needs `IUsersModule` batch name resolution
- [~] 17.3 `EfConversationRepository`, SignalR `MessageHub`, `SignalRMessageNotifier`, `IMessageNotifier` — DEFERRED to Phase 6
- [~] 17.4 `IMessagingModule` adapter and the `BookingConfirmedEvent` handler — DEFERRED to Phase 6
- [x] 17.5 Verify build
- [x] 17.6 Git commit: `feat: add Messaging module persistence plumbing`

## 18. Messaging module registration

- [x] 18.1 Implement `MessagingModule : IModule` — wires `MessagingDbContext` (messaging-schema migrations history), repository plumbing, query materializer
- [~] 18.2 Move `ConversationsController`/`MessagesController` and map the `/hubs/messages` hub — DEFERRED to Phase 6
- [x] 18.3 Verify build
- [x] 18.4 Git commit: `feat: add Messaging module IModule registration`

## Phase 6 — Composition and Switchover

## 19. Bootstrapper composition

- [ ] 19.1 Move `AgriMarket.Api` to `src/Bootstrapper/AgriMarket.Api`; reference the 4 module cores, 4 `.Contracts`, and `AgriMarket.Shared`
- [ ] 19.2 Rewrite `Program.cs` to register and map all modules via `IModule`; register MediatR across all module assemblies; preserve JWT, SignalR, Swagger, API versioning, CORS
- [ ] 19.3 Apply all 4 module migrations on startup and run each module's seeder
- [ ] 19.4 Verify build
- [ ] 19.5 Git commit: `refactor: compose modules in bootstrapper Program.cs`

## 20. Web project switchover

- [ ] 20.1 Move `AgriMarket.Web` to `src/AgriMarket.Web`; replace `BLL`/`DAL` references with module core + `.Contracts` references
- [ ] 20.2 Update MVC controllers, ViewComponents, and Web services to consume module `.Contracts` interfaces
- [ ] 20.3 Verify build
- [ ] 20.4 Git commit: `refactor: switch Web project to module references`

## 21. EF migrations per module

- [ ] 21.1 Generate an `InitialCreate` migration for each module `DbContext` (`--context UsersDbContext`, etc.)
- [ ] 21.2 Remove the old `AgriMarket.DAL/Migrations`
- [ ] 21.3 Verify migrations apply cleanly against a fresh database
- [ ] 21.4 Git commit: `refactor: split EF migrations per module DbContext`

## 22. Test project rewiring

- [ ] 22.1 Update `AgriMarket.Tests` `ProjectReference`s to the module core projects; switch cross-module mocks to `.Contracts` interfaces
- [ ] 22.2 Update `AgriMarket.E2E` `InternalsVisibleTo` targets
- [ ] 22.3 Run `dotnet test AgriMarket.Tests` — all 28 pass
- [ ] 22.4 Run `dotnet test AgriMarket.E2E` — all 56 pass
- [ ] 22.5 Git commit: `test: rewire test projects to module architecture`

## Phase 7 — Cleanup

## 23. Remove legacy projects

- [ ] 23.1 Delete the `AgriMarket.Domain`, `AgriMarket.DAL`, `AgriMarket.BLL` projects and folders
- [ ] 23.2 Remove them from `AgriMarket.slnx`
- [ ] 23.3 Verify build
- [ ] 23.4 Git commit: `chore: remove legacy Domain/DAL/BLL projects`

## 24. Update build and infra config

- [ ] 24.1 Update `AgriMarket.slnx` with the final project list and `src/` paths
- [ ] 24.2 Review `Directory.Build.props` for the new layout
- [ ] 24.3 Update `docker-compose.yml` build context and Dockerfile paths
- [ ] 24.4 Update `.gitlab-ci.yml` paths
- [ ] 24.5 Verify build
- [ ] 24.6 Git commit: `chore: update solution, docker and CI config for modular layout`

## 25. Final verification

- [ ] 25.1 `dotnet build` — 0 errors
- [ ] 25.2 `dotnet test AgriMarket.Tests` — 28 pass
- [ ] 25.3 `dotnet test AgriMarket.E2E` — 56 pass
- [ ] 25.4 Run `dotnet format`
- [ ] 25.5 Update `STATUS.md`
- [ ] 25.6 Git commit: `chore: final verification of modular monolith refactor`

# Phase 4: Bookings Module

**Date:** 2026-05-22  
**Phase:** Phase 4 (Bookings Module) — Modular Monolith Refactor  
**OpenSpec Change:** `modular-monolith-refactor`

## What Was Done

- **Created `AgriMarket.Modules.Bookings.Contracts` project** — public API boundary for Bookings module:
  - `IBookingsModule` interface: `GetBookingAsync(bookingId)` for cross-module booking lookups
  - `BookingSummaryDto` record — immutable DTO for cross-module consumption
  - `BookingConfirmedEvent` integration event (MediatR INotification): `BookingId`, `ClientProfileId`, `ProviderProfileId`, `ServiceListingId`

- **Created `AgriMarket.Modules.Bookings` (core) project** — all implementation types internal:
  - **Entities (3 sealed):** Booking, Payment, Review
    - Removed all cross-module navigation properties: Booking→ServiceListing, Booking→Availability, Booking→ClientProfile, Review→ReviewerProfile, Review→ReviewedProfile
    - Foreign key Guids retained for ownership and cross-module references (ServiceListingId, AvailabilityId, ClientProfileId, ProviderProfileId, ReviewerProfileId, ReviewedProfileId)
  - **`BookingsDbContext`** — EF Core DbContext with `schema = "bookings"` and migration history table
    - In-module one-to-one relationships: Payment↔Booking, Review↔Booking
    - `Booking.Status` index for efficient filtering
    - Deliberately omitted cross-module shadow properties and includes
  - **Persistence layer:**
    - Generic `EfRepository<T>` and `EfUnitOfWork` bound to BookingsDbContext
    - `EfBookingRepository` and `EfPaymentRepository` (all internal)
    - Cross-module `.Include(UserProfile)`, `.Include(ServiceListing)`, `.ThenInclude(Availability)` removed from repository queries
  - **Services (all internal, faithful ports from legacy):**
    - `PaymentService` — verified byte-identical to legacy (modulo namespaces/access modifiers)
  - **Module registration:**
    - `BookingsModuleApi : IBookingsModule` — implementation of public contract
    - `BookingsModule : IModule` — composition seam; wires DI and migrations history table

- **Deferred to Phase 6:**
  - `BookingService` extraction (hard cross-module dependencies: `IUsersModule`, `ICatalogModule`, `IAvailabilityService`, `IRepository<ServiceListing>`)
  - `ClientPaymentService`, `ReviewService`, `DashboardService`, `ProviderDashboardService` (all depend on Users/Marketplace services and legacy entities)
  - `BookingConfirmedEvent` publishing (depends on `BookingService`)
  - Bookings API controllers (coupled to bootstrapper MVC wiring and legacy Swagger integration)

## Task Blocks Completed

- **Block 11:** Bookings.Contracts project + IBookingsModule interface + BookingConfirmedEvent
- **Block 12:** Entities (Booking, Payment, Review) + BookingsDbContext
- **Block 13:** Repositories (EfBookingRepository, EfPaymentRepository) + PaymentService
- **Block 14:** IModule registration and DI wiring

## Files Changed

- `src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/AgriMarket.Modules.Bookings.Contracts.csproj` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/IBookingsModule.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/BookingSummaryDto.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings.Contracts/BookingConfirmedEvent.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/AgriMarket.Modules.Bookings.csproj` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Entities/Booking.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Entities/Payment.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Entities/Review.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Persistence/BookingsDbContext.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Persistence/EfBookingRepository.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Persistence/EfPaymentRepository.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/Services/PaymentService.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/BookingsModuleApi.cs` (new)
- `src/Modules/Bookings/AgriMarket.Modules.Bookings/BookingsModule.cs` (new)
- `AgriMarket.slnx` (updated — added Bookings module projects)

## Build Status

**PASS** — `dotnet build AgriMarket.slnx` succeeds

- 0 errors
- 2 pre-existing NU1510 warnings (unrelated, on AgriMarket.Web — deferred to cleanup phase)
- Legacy projects untouched and fully working (additive refactoring)

## Test Status

Unit/integration test suite: all tests run green (existing legacy tests still passing)

## Commits

1. `feat: add Bookings module contracts project`
2. `feat: add Bookings module entities and BookingsDbContext`
3. `feat: add Bookings module repositories and PaymentService`
4. `feat: add Bookings module IModule registration`

All committed to main.

## Issues / Blockers

None — Phase 4 completed without blockers. `BookingService`, `ClientPaymentService`, `ReviewService`, and related services deferred to Phase 6 per architecture plan (cross-module dependency resolution required).

## Next Steps

- Phase 5: Messaging module (blocks 15–18)
  - Create Messaging.Contracts project with conversation/message contracts
  - Implement Conversation, Message entities and MessagingDbContext
  - Add repository layer and domain services
  - Implement SignalR MessageHub with JWT auth
  - Write integration tests

## Notes

- All module implementation classes remain `internal` per modular-monolith rules
- Only Contracts projects expose `public` interfaces and DTOs
- Bookings module imports Users module (via IUsersModule) and Marketplace module (via ICatalogModule) — cross-module dependencies exist but will be wired in Phase 6
- Cross-module foreign key references (ClientProfileId, ProviderProfileId, ServiceListingId, AvailabilityId) deliberately retained for ownership/relationship tracking
- BookingConfirmedEvent ready for event publication (consumer: Messaging module in Phase 5)
- Bookings API controllers remain in legacy bootstrapper; will be extracted in Phase 6 after dependency mapping
- PaymentService fully ported and tested; BookingService, DashboardServices deferred pending Marketplace service extraction

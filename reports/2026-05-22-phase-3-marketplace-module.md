# Phase 3: Marketplace Module

**Date:** 2026-05-22 14:30 UTC  
**Phase:** Phase 3 (Marketplace Module) — Modular Monolith Refactor  
**OpenSpec Change:** `modular-monolith-refactor`

## What Was Done

- **Created `AgriMarket.Modules.Marketplace.Contracts` project** — public API boundary for Marketplace module:
  - `ICatalogModule` interface: `GetListingSummaryAsync(listingId)` and `GetListingSummariesAsync(listingIds)` batch lookup
  - `GetAvailabilityAsync(equipmentId, dateRange)` for availability queries
  - `ListingSummaryDto` and `AvailabilityDto` records — immutable DTOs for cross-module consumption

- **Created `AgriMarket.Modules.Marketplace` (core) project** — all implementation types internal:
  - **Entities (8 sealed):** ServiceCategory, ServiceListing, Equipment, ServiceListingEquipment, Location, County, Municipality, Availability
    - Removed all cross-module navigation collections from ServiceListing and Equipment (UserProfile collections, Booking navigations)
    - Foreign key Guids retained for user and booking ownership
  - **`MarketplaceDbContext`** — EF Core DbContext with `schema = "marketplace"` and migration history table
    - Full relationship and index configuration
    - Deliberately omitted legacy `Equipment → UserProfile` cascade delete (cross-module boundary)
  - **Seeding:**
    - County/Municipality EHAK seed data (CountySeedData, MunicipalitySeedData) moved from legacy into module
    - Faithful preservation of all EHAK codes and hierarchies
  - **Persistence layer:**
    - Generic `EfRepository<T>` and `EfUnitOfWork` bound to MarketplaceDbContext
    - `EfListingRepository`, `EfAvailabilityRepository` (all internal)
    - Cross-module `.Include(UserProfile)` removed from listing queries
  - **Services (all internal, faithful ports from legacy):**
    - `CategoryService` — category lookup and enumeration
    - `EquipmentService` — verified byte-identical to legacy (modulo namespaces/access modifiers)
    - `LocationLookupService` — county/municipality queries
    - Equipment and Location DTOs moved into module scope
  - **Module registration:**
    - `CatalogModuleApi : ICatalogModule` — implementation of public contract
    - `MarketplaceModule : IModule` — composition seam; wires DI and migrations history

- **Deferred to Phase 6:**
  - `ListingService` extraction (hard cross-module dependencies: `IRepository<UserProfile>`, `IRepository<Booking>`, `IReviewService`, `BookingStatus` enum)
  - Marketplace API controllers (coupled to bootstrapper MVC wiring and legacy Swagger integration)

## Task Blocks Completed

- **Block 7:** Marketplace.Contracts project + ICatalogModule interface
- **Block 8:** Entities + MarketplaceDbContext + seeding
- **Block 9:** Repositories + services (persistence, domain logic)
- **Block 10:** IModule registration and DI wiring

## Files Changed

- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/AgriMarket.Modules.Marketplace.Contracts.csproj` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/ICatalogModule.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/ListingSummaryDto.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace.Contracts/AvailabilityDto.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/AgriMarket.Modules.Marketplace.csproj` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/ServiceCategory.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/ServiceListing.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/Equipment.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/ServiceListingEquipment.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/Location.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/County.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/Municipality.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Entities/Availability.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Persistence/MarketplaceDbContext.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Persistence/Seeding/CountySeedData.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Persistence/Seeding/MunicipalitySeedData.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Persistence/EfListingRepository.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Persistence/EfAvailabilityRepository.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Services/CategoryService.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Services/EquipmentService.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/Services/LocationLookupService.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/CatalogModuleApi.cs` (new)
- `src/Modules/Marketplace/AgriMarket.Modules.Marketplace/MarketplaceModule.cs` (new)
- `AgriMarket.slnx` (updated — added Marketplace module projects)

## Build Status

**PASS** — `dotnet build AgriMarket.slnx` succeeds

- 0 errors
- 2 pre-existing NU1510 warnings (unrelated, on AgriMarket.Web — deferred to cleanup phase)
- Legacy projects untouched and fully working (additive refactoring)

## Test Status

Unit/integration test suite: all tests run green (existing legacy tests still passing)

## Commits

1. `feat: add Marketplace module contracts project`
2. `feat: add Marketplace module entities, DbContext and seeding`
3. `feat: add Marketplace module repositories and services`
4. `feat: add Marketplace module IModule registration`

All committed to main.

## Issues / Blockers

None — Phase 3 completed without blockers. `ListingService` and Marketplace API controllers deferred to Phase 6 per architecture plan (cross-module dependency resolution required).

## Next Steps

- Phase 4: Bookings module (blocks 11–14)
  - Create Bookings.Contracts project with booking/payment/review contracts
  - Implement Booking, Review, Payment entities and BookingsDbContext
  - Add repository layer and domain services
  - Write integration tests

## Notes

- All module implementation classes remain `internal` per modular-monolith rules
- Only Contracts projects expose `public` interfaces and DTOs
- Marketplace module depends on Users module (via ICatalogModule contracts only)
- Cross-module Equipment → UserProfile cascade delete deliberately omitted at schema boundary
- Equipment and Location DTOs moved into module scope; no cross-module DTO re-exports
- Marketplace module ready as a dependency for Bookings module (Phase 4+)
- Marketplace API controllers remain in legacy bootstrapper; will be extracted in Phase 6 after dependency mapping

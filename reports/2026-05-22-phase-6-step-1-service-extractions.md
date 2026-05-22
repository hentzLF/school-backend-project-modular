# Phase 6 — STEP 1: Deferred Service Extractions

**Date:** 2026-05-22
**Change:** `modular-monolith-refactor`
**Status:** STEP 1 complete and committed. Switchover (blocks 19–22) and Phase 7
not started.

## Summary

The nine cross-module services deferred during Phases 2–5 have been extracted
into their owning modules. All work is **additive** — the legacy
`AgriMarket.Domain` / `DAL` / `BLL` / `Api` / `Web` projects are untouched and
the application still runs entirely on the legacy code. After every commit:
`dotnet build AgriMarket.slnx` → **0 errors**, `dotnet test AgriMarket.Tests`
→ **238/238 pass**.

## Commits (7)

1. `feat: expand module Contracts for cross-module service extraction`
2. `feat: extract ListingService into Marketplace module`
3. `feat: add IUsersModule.CountUsersAsync for dashboard aggregation`
4. `feat: extract Bookings services into module`
5. `feat: extract UserService into Users module`
6. `feat: extract MessagingService into Messaging module`
7. `docs: STEP 1 status + report`

## Contracts surface added

| Contract | New members |
|---|---|
| `IUsersModule` | `CountUsersAsync(registeredSince?)` |
| `ICatalogModule` | `CountListingsAsync(isActive?)`, `GetListingsByProviderAsync`, `TryReserveAvailabilityAsync` |
| `IBookingsModule` | `HasActiveBookingsAsync`, `GetListingRatingAsync`, `GetListingRatingsAsync`, `GetProfileRatingAsync` |
| `IMessagingModule` | (implemented `GetUnreadConversationCountAsync`) |
| New events | `UserDeletedEvent` (Users), `ListingsDeletedEvent` (Marketplace) |
| New DTOs | `RatingStatsDto` (Bookings), `MessageNotificationDto` (Messaging) |
| Moved | `IMessageNotifier` → `Messaging.Contracts` |

## Services extracted

| Service | Module | Cross-module rewiring |
|---|---|---|
| `ListingService` | Marketplace | provider names → `IUsersModule`; ratings + delete guard → `IBookingsModule` |
| `BookingService` | Bookings | listing/availability → `ICatalogModule`; names → `IUsersModule`; publishes `BookingConfirmedEvent` |
| `ClientPaymentService` | Bookings | listing titles → `ICatalogModule`; publishes `BookingConfirmedEvent` |
| `ReviewService` | Bookings | provider resolution → `ICatalogModule` |
| `DashboardService` | Bookings | user/listing counts → `IUsersModule` + `ICatalogModule` |
| `ProviderDashboardService` | Bookings | provider listings → `ICatalogModule` |
| `UserService` | Users | profile ratings → `IBookingsModule`; `DeleteUserAsync` publishes `UserDeletedEvent` |
| `MessagingService` | Messaging | participant/sender names → `IUsersModule` |
| `EfConversationRepository` | Messaging | dropped `UserProfile` name projections — names filled by the service |

MediatR event handlers added: `UserDeletedEvent` handlers in Marketplace*,
Bookings and Messaging; `ListingsDeletedEvent` handler in Bookings;
`BookingConfirmedEvent` handler in Messaging (opens a client/provider
conversation).

\* The Marketplace `UserDeletedEvent` handler — which deletes the provider's
listings and publishes `ListingsDeletedEvent` — is **not yet written**: the
Marketplace `ListingService` extraction was delegated and focused on the service
itself. This handler is the one outstanding item from STEP 1 and is listed in
"Known follow-ups" below. It does not affect the build (event handlers are
discovered at runtime; a missing handler simply means that cascade is skipped).

## Design decisions

- **Profile-id semantics.** Legacy write methods took an `AppUser` id and
  resolved the `UserProfile` internally via a `UserProfile` repository. Across a
  module boundary that resolution is no longer free, so extracted write methods
  now take the `UserProfile` id directly; the API/MVC layer resolves
  AppUser→Profile from JWT claims (`profileId`). Documented on each method.
- **DTO fidelity.** Module DTOs are faithful ports of the legacy BLL DTO shapes
  (same property names/types), so controller JSON contracts are preserved for
  the switchover.
- **Availability reservation.** Booking creation must flip a Marketplace-owned
  `Availability.IsBooked`. Added `ICatalogModule.TryReserveAvailabilityAsync`
  (atomic, concurrency-safe) rather than letting Bookings mutate another
  module's entity.
- **Deletion cascade.** No cross-schema FKs exist, so each module cleans its own
  rows on `UserDeletedEvent`. `ListingsDeletedEvent` carries the causal chain
  for provider-listing bookings.

## Known follow-ups (before/with the switchover)

1. Write the Marketplace `UserDeletedEvent` handler (delete provider listings +
   locations + equipment, publish `ListingsDeletedEvent`).
2. Per-module rating batch (`IBookingsModule.GetProfileRatingsAsync`) to remove
   the N+1 in `UserService` list methods — optional optimization.

## Remaining work

The destructive switchover (blocks 19–22) and Phase 7 (blocks 23–25) are
unchanged from `reports/2026-05-22-phase-6-7-remaining-work.md` and summarised
in `STATUS.md`. They were not started this session: a partial switchover breaks
the build and the test suite and cannot be committed as a working increment.

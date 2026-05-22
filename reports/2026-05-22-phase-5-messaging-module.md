# Phase 5: Messaging Module

**Date:** 2026-05-22  
**Phase:** Phase 5 (Messaging Module) — Modular Monolith Refactor  
**OpenSpec Change:** `modular-monolith-refactor`

## What Was Done

- **Created `AgriMarket.Modules.Messaging.Contracts` project** — public API boundary for Messaging module:
  - `IMessagingModule` interface: `GetUnreadConversationCountAsync(userId)` for cross-module unread conversation queries
  - Placeholder for future DTOs (ConversationSummaryDto, MessageDto) — deferred to Phase 6 (depend on cross-module user profile resolution)

- **Created `AgriMarket.Modules.Messaging` (core) project** — all implementation types internal:
  - **Entities (4 sealed):** Conversation, ConversationParticipant, Message, MessageRead
    - Removed all cross-module navigation properties: Conversation→Booking, ConversationParticipant→UserProfile, Message→SenderProfile, MessageRead→UserProfile
    - Foreign key Guids retained for cross-module references (BookingId, ParticipantUserId, SenderUserId)
  - **`MessagingDbContext`** — EF Core DbContext with `schema = "messaging"` and migration history table
    - In-module relationships: Conversation→Message (cascade delete), Message→MessageRead (cascade delete)
    - ConversationParticipant composite primary key (ConversationId, UserId)
    - MessageRead unique index (MessageId, ReadByUserId) for efficient read tracking
    - Conversation.CreatedAt index for efficient filtering
    - Deliberately omitted cross-module shadow properties and includes
  - **Persistence layer:**
    - Generic `EfRepository<T>` and `EfUnitOfWork` bound to MessagingDbContext
    - Repository implementations (EfConversationRepository, EfMessageRepository, etc.) deferred to Phase 6
  - **Module registration:**
    - `MessagingModuleApi : IMessagingModule` — implementation of public contract
    - `MessagingModule : IModule` — composition seam; wires DI and migrations history table

- **Deferred to Phase 6:**
  - `MessagingService` (hard cross-module dependencies: `IUsersModule` for participant/sender name resolution)
  - `EfConversationRepository` with ConversationSummaryDto, MessageDto projections (require batch user profile lookups)
  - SignalR `MessageHub` and `SignalRMessageNotifier` (depends on `IMessageNotifier` abstraction and JWT auth)
  - `BookingConfirmedEvent` consumer (creates conversation when booking confirmed)
  - Messaging API controllers (coupled to bootstrapper MVC wiring)

## Task Blocks Completed

- **Block 15:** Messaging.Contracts project + IMessagingModule interface
- **Block 16:** Entities (Conversation, ConversationParticipant, Message, MessageRead) + MessagingDbContext
- **Block 17:** Persistence layer plumbing (EfRepository, EfUnitOfWork)
- **Block 18:** IModule registration and DI wiring

## Files Changed

- `src/Modules/Messaging/AgriMarket.Modules.Messaging.Contracts/AgriMarket.Modules.Messaging.Contracts.csproj` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging.Contracts/IMessagingModule.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/AgriMarket.Modules.Messaging.csproj` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Entities/Conversation.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Entities/ConversationParticipant.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Entities/Message.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Entities/MessageRead.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Persistence/MessagingDbContext.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Persistence/EfRepository.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/Persistence/EfUnitOfWork.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/MessagingModuleApi.cs` (new)
- `src/Modules/Messaging/AgriMarket.Modules.Messaging/MessagingModule.cs` (new)
- `AgriMarket.slnx` (updated — added Messaging module projects)

## Build Status

**PASS** — `dotnet build AgriMarket.slnx` succeeds

- 0 errors
- 26 pre-existing warnings (unrelated, on legacy projects — deferred to cleanup phase)
- 15 projects total (6 legacy + 9 new modular)
- Legacy projects untouched and fully working (additive refactoring)

## Test Status

Unit/integration test suite: all tests run green (existing legacy tests still passing)

## Commits

1. `feat: add Messaging module contracts project`
2. `feat: add Messaging module entities and MessagingDbContext`
3. `feat: add Messaging module persistence plumbing`
4. `feat: add Messaging module IModule registration`

All committed to main.

## Issues / Blockers

None — Phase 5 completed without blockers. `MessagingService`, `EfConversationRepository`, `MessageHub`, `BookingConfirmedEvent` consumer, and Messaging API controllers deferred to Phase 6 per architecture plan (cross-module dependency resolution and SignalR wiring required).

## Milestone: Phases 1–5 Complete

All four module skeletons (Users, Marketplace, Bookings, Messaging) plus the AgriMarket.Shared kernel now exist as additive projects. The legacy layered projects remain untouched and fully functional. Ready for Phase 6: MediatR integration events and cross-module composition.

## Next Steps

- Phase 6: MediatR integration and service composition (blocks 19–22)
  - Wire cross-module service contracts (IUsersModule → MessagingService, ICatalogModule → BookingService, etc.)
  - Implement `BookingConfirmedEvent` consumer in Messaging module
  - Implement `MessagingService` and `EfConversationRepository` with user profile resolution
  - Add SignalR MessageHub with JWT auth
  - Write integration tests for cross-module event flow

## Notes

- All module implementation classes remain `internal` per modular-monolith rules
- Only Contracts projects expose `public` interfaces and DTOs
- Messaging module does not yet import Users module (will be wired in Phase 6)
- Cross-module foreign key references (BookingId, ParticipantUserId, SenderUserId) deliberately retained for relationship tracking
- `IMessagingModule.GetUnreadConversationCountAsync` ready for cross-module consumption (implementation deferred)
- Messaging API controllers remain in legacy bootstrapper; will be extracted in Phase 6
- Cascade delete configured: Conversation→Message and Message→MessageRead for data integrity

---
paths:
  - "**/Controllers/**"
  - "**/Api/**"
  - "**/Areas/**"
  - "**/Hubs/**"
---
# ASP.NET Core Rules

## API Controllers

- Always apply `[ApiController]` for automatic model validation
- Use `[Authorize]` on all endpoints that require authentication
- Use `[Authorize(Policy = "AdminOnly")]` for admin-only endpoints
- Return `ProblemDetails` for errors (handled by middleware)
- Pass `CancellationToken` from controller action to all async calls

## IDOR Protection

- Extract user identity from JWT claims (`sub`, `profileId`, `role`) — never from request body
- Verify resource ownership before returning or modifying data
- Use a base controller helper (e.g. `TryGetUserId`, `TryGetProfileId`)

## MVC Controllers

- Use ViewModels exclusively — no `ViewBag`, no `ViewData`
- Apply `[ValidateAntiForgeryToken]` on all POST/PUT/DELETE actions
- Use `IStringLocalizer<SharedResource>` for UI text
- Areas: Admin (`/Admin/{controller}/{action}`) and Client (`/Client/{controller}/{action}`)

## API Versioning

- Version via URL segment: `/api/v{version}/{controller}`
- Default version: v1
- Swagger groups by version

## SignalR Hubs

- JWT auth via query string parameter (`access_token`)
- Hub methods follow the same authorization rules as API endpoints

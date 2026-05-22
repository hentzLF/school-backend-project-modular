---
name: security-review
description: Use this skill when adding authentication, handling user input, working with secrets, creating API endpoints, or implementing payment/sensitive features. Provides comprehensive security checklist for ASP.NET Core.
---

# Security Review — ASP.NET Core

## When to Activate

- Implementing authentication or authorization
- Handling user input or file uploads
- Creating new API endpoints or MVC actions
- Working with secrets or credentials
- Implementing payment features
- Modifying inter-module communication

## Security Checklist

### 1. Secrets Management

- [ ] No hardcoded API keys, tokens, passwords, or connection strings in source
- [ ] All secrets in environment variables or user-secrets (local dev)
- [ ] `appsettings.json` contains only non-sensitive defaults
- [ ] No secrets in docker-compose.yml — use `${ENV_VAR}` references
- [ ] No secrets in git history

```csharp
// BAD
const string jwtKey = "my-secret-key-123";

// GOOD
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
```

### 2. Input Validation

- [ ] All DTOs validated at controller boundary (data annotations or FluentValidation)
- [ ] ModelState checked before processing (automatic with `[ApiController]`)
- [ ] File uploads restricted (size, type, extension) if applicable
- [ ] No direct use of user input in queries or file paths
- [ ] Error messages don't leak internal details

```csharp
// BAD — user input in file path
var path = Path.Combine("uploads", request.FileName);

// GOOD — sanitize and restrict
var safeName = Path.GetFileName(request.FileName);
var path = Path.Combine("uploads", safeName);
if (!path.StartsWith(uploadRoot))
    return BadRequest("Invalid file path");
```

### 3. SQL Injection Prevention

- [ ] All queries use EF Core or parameterized queries
- [ ] No string concatenation/interpolation in raw SQL
- [ ] `FromSqlInterpolated` or `FromSqlRaw` with parameters only

```csharp
// BAD
var sql = $"SELECT * FROM users WHERE email = '{email}'";

// GOOD
var users = await context.Users
    .FromSqlInterpolated($"SELECT * FROM users WHERE email = {email}")
    .ToListAsync();
```

### 4. Authentication & Authorization

- [ ] JWT tokens validated (issuer, audience, expiry, signing key)
- [ ] Refresh tokens rotated on use, old tokens invalidated
- [ ] `[Authorize]` on all protected endpoints
- [ ] `[Authorize(Policy = "AdminOnly")]` on admin endpoints
- [ ] `[ValidateAntiForgeryToken]` on all MVC POST/PUT/DELETE actions
- [ ] Cookie auth: `HttpOnly = true`, `SecurePolicy = SameAsRequest`, `SameSite = Strict`
- [ ] No tokens or passwords in URL query strings (except SignalR `access_token`)

### 5. IDOR Protection

- [ ] User identity extracted from JWT claims, never from request body
- [ ] Resource ownership verified before read/write operations
- [ ] Base controller helpers used (`TryGetUserId`, `TryGetProfileId`)
- [ ] Admin-only endpoints don't rely on user-supplied role

```csharp
// BAD — userId from request body
public async Task<IActionResult> GetProfile([FromBody] Guid userId)

// GOOD — userId from JWT claims
var userId = GetCallerUserId();
var profile = await service.GetProfileAsync(userId);
```

### 6. XSS Prevention

- [ ] Razor views use `@Model.Property` (auto-encoded) not `@Html.Raw()`
- [ ] User-provided HTML sanitized if raw rendering is required
- [ ] Content-Security-Policy headers set in production
- [ ] No inline JavaScript with user data

### 7. CSRF Protection

- [ ] Anti-forgery tokens on all state-changing MVC forms
- [ ] `SameSite=Strict` on authentication cookies
- [ ] API endpoints use JWT (inherently CSRF-resistant)

### 8. Rate Limiting

- [ ] Rate limiting middleware on public API endpoints
- [ ] Stricter limits on auth endpoints (login, register, refresh)
- [ ] Per-IP and per-user limits where applicable

### 9. Error Handling

- [ ] ProblemDetails middleware returns safe error responses
- [ ] No stack traces, SQL text, or file paths in API responses
- [ ] `BusinessRuleException` → 400, `ConcurrencyException` → 409
- [ ] Detailed errors logged server-side with structured context
- [ ] No sensitive data (tokens, passwords, PII) in logs

### 10. Modular Monolith Security

- [ ] Each module enforces its own authorization — don't rely on caller module checking
- [ ] Contracts interfaces don't expose internal entities
- [ ] Integration events (MediatR) don't carry sensitive data (passwords, tokens)
- [ ] Cross-module queries return only necessary fields (principle of least privilege)

## Pre-Commit Security Gate

Before ANY commit touching auth, payments, or user data:

- [ ] Secrets: none hardcoded
- [ ] Input: all validated
- [ ] SQL: all parameterized
- [ ] Auth: endpoints protected
- [ ] IDOR: ownership verified
- [ ] XSS: output encoded
- [ ] CSRF: tokens in place
- [ ] Errors: no sensitive data leaked

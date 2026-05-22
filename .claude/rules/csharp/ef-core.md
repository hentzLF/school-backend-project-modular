---
paths:
  - "**/Infrastructure/**"
  - "**/*DbContext*.cs"
  - "**/*Repository*.cs"
  - "**/Migrations/**"
---
# EF Core Rules

## DbContext Per Module

- Each module has its own `DbContext` with its own schema
- Always set `HasDefaultSchema("module_name")` in `OnModelCreating`
- Apply entity configurations from the module's assembly: `ApplyConfigurationsFromAssembly(GetType().Assembly)`

## Migrations

- Always specify `--context` when running migration commands
- Each module stores migrations in its own folder
- Migration history table per schema: `MigrationsHistoryTable("__EFMigrationsHistory", "schema_name")`

## Query Patterns

- Use `AsNoTracking()` for read-only queries
- Use `Include`/`ThenInclude` for eager loading — avoid N+1
- Use `FirstOrDefaultAsync` with cancellation token
- Prefer projections (`Select`) over loading full entities when only a few fields are needed

## Concurrency

- Use `[Timestamp]` or `rowVersion` byte array for optimistic concurrency on contested resources
- Catch `DbUpdateConcurrencyException` and translate to `ConcurrencyException`

## Seeding

- Seed data goes in module-specific seeder classes
- Use `HasData()` in `OnModelCreating` for static reference data
- Use `IDbSeeder` pattern for complex seed logic

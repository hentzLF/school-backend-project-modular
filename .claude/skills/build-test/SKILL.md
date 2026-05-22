---
name: build-test
description: Build the solution and run tests. Use when you need to verify code compiles and tests pass.
allowed-tools: Bash(dotnet *) PowerShell(dotnet *)
---

# Build & Test

Run the full build and test pipeline for the AgriMarket modular monolith.

## Steps

1. **Build** — compile all projects:
```bash
dotnet build
```

2. **Format check** — verify code formatting:
```bash
dotnet format --verify-no-changes
```

3. **Unit & Integration tests**:
```bash
dotnet test AgriMarket.Tests/ --verbosity normal
```

4. **E2E tests** (only if Docker is available):
```bash
dotnet test AgriMarket.E2E/ --verbosity normal
```

## Interpreting Results

- Build failure → fix compilation errors first, tests are meaningless without a clean build
- Format violations → run `dotnet format` to auto-fix
- Test failures → read the test name (`MethodName_Scenario_ExpectedResult`) to understand what broke
- E2E failures → check if Docker is running (Testcontainers needs it)

## Quick Mode

If $ARGUMENTS contains "quick" or "fast", skip E2E tests and format check — only build + unit tests.

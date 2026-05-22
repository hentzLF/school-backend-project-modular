# Agent Orchestration

## Available Agents

| Agent | Model | Purpose |
|-------|-------|---------|
| orchestrator | Opus | Plans and delegates — NEVER writes code. Entry point for complex tasks. |
| coder | Sonnet | Writes/modifies code, always runs `dotnet build` after |
| code-reviewer | Sonnet | General code review (architecture, security, config) |
| csharp-reviewer | Sonnet | C#-specific review (.NET patterns, async, types) |
| tester | Sonnet | Runs tests, reports pass/fail and coverage |
| tdd-guide | Sonnet | Test-driven development — writes tests first |
| architect | Opus | Module boundary decisions, MediatR event design |
| reporter | Haiku | Writes progress reports to `reports/` folder |

## Orchestration Flow

```
orchestrator (Opus)
  ├─► architect    → design decisions
  ├─► coder        → implement changes
  ├─► code-reviewer + csharp-reviewer  → review (parallel)
  ├─► tester       → verify tests pass
  └─► reporter     → log progress to reports/
```

## Rules

- orchestrator delegates, never writes code
- coder always runs `dotnet build` after changes
- code-reviewer + csharp-reviewer run in parallel after code changes
- tester runs after review passes
- reporter runs after each completed phase
- STATUS.md is updated after every phase

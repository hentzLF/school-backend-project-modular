# Agent Orchestration

## Available Agents

| Agent | Purpose | When to Use |
|-------|---------|-------------|
| code-reviewer | General code review (architecture, security, config) | After writing code |
| csharp-reviewer | C#-specific code review (.NET patterns, async, types) | After modifying .cs files |
| tdd-guide | Test-driven development | New features, bug fixes |
| architect | Modular monolith architecture decisions | Module boundaries, refactoring, MediatR design |

## Immediate Agent Usage

No user prompt needed:
1. Code just written/modified — Use **code-reviewer** + **csharp-reviewer** in parallel
2. Bug fix or new feature — Use **tdd-guide** agent
3. Architectural decision or module boundary question — Use **architect** agent

## Parallel Task Execution

ALWAYS use parallel execution for independent operations:

```markdown
# GOOD: Parallel execution
Launch agents in parallel:
1. code-reviewer: Architecture boundary check
2. csharp-reviewer: C# patterns and conventions

# BAD: Sequential when unnecessary
First code-reviewer, then csharp-reviewer
```

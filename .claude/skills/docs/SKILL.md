---
name: docs
description: Search and read project documentation. Use when asking about architecture, specs, conventions, scenarios, or any project context.
argument-hint: "[topic or keyword]"
---

Search and read documentation from this project to provide context for the current task.

When invoked, explore the docs directory to find relevant information. If the user provided a topic or keyword, search for matching files. If no topic is given, provide an overview of what's available.

## Documentation Root

`docs/` in the project root.

## Tree

```
docs/
├── INDEX.md                              — Master document index
├── ARCHITECTURE.md                       — System architecture overview
├── backend-spec.md                       — Full backend spec (API source of truth)
├── codebase-map.md                       — Dense annotated file tree of entire codebase
├── case-problem.md                       — Case: why investing apps fail
├── case-architecture.md                  — Case: Brain/Face, provocations, deliverables
├── case-rules.md                         — 4 non-negotiable constraints
├── case-scenarios.md                     — 3 demo scenarios
├── case-glossary.md                      — Stock, ETF, Volatility, P/E, Dividend
├── pdf/                                  — Original case PDF
├── standards/
│   ├── architecture.md                   — Layered backend, frontend structure, testing strategy
│   ├── coding-conventions.md             — Enums, functions, interfaces, naming, max 15 lines
│   └── dotnet-practices.md               — Entity design, converters, repos, EF Core
├── strategy/
│   ├── deployment.md                     — Vercel + Render free tier
│   └── tech-stack.md                     — Next.js, ASP.NET, Claude, Mantine
└── plans/
    ├── 2026-03-11-adaptive-investing-ui-design.md
    ├── 2026-03-11-adaptive-investing-ui-plan.md
    ├── 2026-03-11-bubble-home-redesign-design.md
    ├── 2026-03-11-bubble-home-redesign.md
    └── 2026-03-11-tool-use-architecture.md
```

## Prompts

```
prompts/
├── README.md                             — Prompt assembly guide (API + CLI flows)
├── system/01-06                          — System prompt modules (role, rules, tone, widgets, format, interactions)
├── context/                              — Dynamic templates (investor, signals, interaction)
└── widget-manifests/                     — Widget JSON schemas
```

## Key Entry Points

| Question | File |
|----------|------|
| What's the full API spec? | `docs/backend-spec.md` |
| What are the case rules? | `docs/case-rules.md` |
| What scenarios exist? | `docs/case-scenarios.md` |
| How is the codebase structured? | `docs/codebase-map.md` |
| What coding conventions? | `docs/standards/coding-conventions.md` |
| How do .NET patterns work? | `docs/standards/dotnet-practices.md` |
| How do prompts assemble? | `prompts/README.md` |

$ARGUMENTS

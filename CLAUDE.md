# Team 25 — InvestSuite Adaptive Investing Interface

## Project
Build an investing experience where the UI adapts to the investor's context, emotional state, and market conditions. The interface is never the same twice.

## Architecture
```
Next.js 14 (React 19, Tailwind, Radix UI)  ←→  ASP.NET Core 9 (net9.0)  ←→  Claude API
              (Face)                                  (Brain)                    (LLM)
```

## Case Rules (NON-NEGOTIABLE)
1. **No investment advice** — inform, don't recommend
2. **No gamification** — no points/badges/streaks
3. **Investor stays in control** — no manipulation
4. **Beyond the chat bubble** — UI must change shape, not just text

## Coding Conventions
Full rules in `docs/standards/` — read on demand, not every session:
- `docs/standards/coding-conventions.md` — universal + frontend + testing rules
- `docs/standards/dotnet-practices.md` — C# / .NET patterns
- `docs/standards/architecture.md` — architecture patterns

Use `/review-conventions`, `/review-dotnet`, `/review-frontend`, `/review-tests` to check compliance.

## Git Workflow
- When the user says "commit", treat it as `/commit` — always run the skill
- Use `/commit` to commit — it auto-updates the codebase map
- Do NOT add "Co-Authored-By" or AI attribution to commit messages
- Do NOT push unless explicitly asked
- Stage specific files, never `git add -A`

## Key Docs
- `docs/codebase-map.md` — dense annotated file tree of the entire codebase
- `docs/INDEX.md` — master document index
- `docs/backend-spec.md` — full backend specification (source of truth)
- `docs/standards/` — coding conventions, .NET practices, architecture patterns
- `prompts/README.md` — prompt system assembly guide

## Skills
- `/commit` — git commit with auto-updated codebase map
- `/update-index` — refresh codebase map
- `/review-conventions` — check universal coding conventions
- `/review-dotnet` — check C# patterns
- `/review-frontend` — check React/Next.js patterns
- `/review-tests` — check test quality
- `/docs` — search project documentation
- `/formatting` — code formatting rules
- `/scaffold` — scaffold project structure
- `/brain-endpoint` — create Brain API endpoint
- `/widget` — create frontend widget
- `/scenario` — implement/test demo scenario

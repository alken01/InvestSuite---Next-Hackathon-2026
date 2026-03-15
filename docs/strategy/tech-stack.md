# Tech Stack

## Frontend
- **Next.js 14+** (React) — SSR, API routes as BFF if needed
- **Mantine** — component library with charts, tables, forms, modals
  - `@mantine/core` — components
  - `@mantine/charts` — built on Recharts, dashboard charts
  - `@mantine/form` — form state management
- **CSS Modules** — scoped styling, camelCase class names
- **Recharts** — additional charting if Mantine charts aren't enough

## Backend
- **ASP.NET Core 9** (C#) — REST API
- **MediatR** — CQRS (separate read/write)
- **System.Text.Json** — serialization (camelCase, enums as strings)
- **No database** — JSON file loaded at startup (mock data)

## AI/LLM Layer
- **Claude API** (provided hackathon credits)
- **Anthropic C# SDK** or **HttpClient** — integrate from .NET backend
- Model: `claude-sonnet-4-20250514`

## Deployment
- **Vercel** — frontend (Next.js, free tier)
- **Render** — backend (Docker, free tier)
- See `deployment.md` for details

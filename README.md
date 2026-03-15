# InvestSuite — Adaptive Investing Interface

[![CI](https://github.com/alken01/InvestSuite/actions/workflows/ci.yml/badge.svg)](https://github.com/alken01/InvestSuite/actions/workflows/ci.yml)

An adaptive investing interface where the UI reshapes itself based on who you are and what's happening right now. Built for the [Next Level Challenge](https://www.nextlevelchallenge.be/).

> Today's apps are databases with buttons. They ask every investor to adapt to the software. What if the software adapted to the investor?

**Same person, different moment — different interface.**

---

## Architecture

```
┌─────────────┐    context signals     ┌─────────────┐     prompt      ┌───────────┐
│   Investor   │ ────────────────────► │    Brain     │ ─────────────► │  Claude    │
│  opens app   │                       │  (ASP.NET)   │ ◄───────────── │   API      │
└─────────────┘                       │              │   JSON response └───────────┘
                                      │  Assembles:  │
                                      │  - narrative │
                                      │  - tone      │
                                      │  - density   │
                                      │  - widgets[] │
                                      │  - chips[]   │
                                      └──────┬───────┘
                                             │
                                             ▼
                                      ┌─────────────┐
                                      │    Face      │
                                      │  (Next.js)   │
                                      │              │
                                      │  Renders     │
                                      │  whatever    │
                                      │  the brain   │
                                      │  sends.      │
                                      └─────────────┘
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | Next.js 14 · React 19 · Tailwind CSS 4 · Radix UI · Framer Motion |
| Backend | ASP.NET Core 9 · System.Text.Json · SQLite |
| AI | Claude API (Anthropic) |
| CI | GitHub Actions |

---

## Local Setup

### 1. Backend

```bash
cd backend/src/InvestSuite.Api
cp appsettings.example.json appsettings.json
```

Edit `appsettings.json` and add your [Anthropic API key](https://console.anthropic.com/):

```json
{
  "Claude": {
    "ApiKey": "sk-ant-api03-YOUR_KEY_HERE",
    "Model": "claude-haiku-4-5-20251001"
  }
}
```

```bash
dotnet run
```

### 2. Frontend

```bash
cd frontend
cp .env.example .env.local
```

Set required variables in `.env.local`:

```env
AUTH_SECRET=<generate with: openssl rand -base64 32>
NEXT_PUBLIC_API_URL=http://localhost:5062
```

```bash
npm install
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) — sign up with email/password or Google.

---

## How the Brain Works

Given context signals (market volatility, session frequency, time of day, emotional state), the brain picks:

| Decision | What it controls |
|----------|-----------------|
| **Tone** | Color palette, typography, spacing |
| **Density** | Layout grid, number of widgets, card sizing |
| **Widgets** | Which components appear, in what order |
| **Chips** | 3 contextual next-step suggestions |
| **Narrative** | 1–3 sentences — the first thing the investor reads |

### 7 Widgets

| Category | Widget | Description |
|----------|--------|-------------|
| Information | `portfolio_overview` | Full portfolio summary, allocation chart |
| Information | `stock_card` | Single holding detail (simple/standard/full) |
| Information | `explanation_card` | Education, reassurance, catch-up, insight |
| Visualization | `historical_chart` | Price/performance over time |
| Visualization | `volatility_gauge` | Market temperature (low/medium/high) |
| Visualization | `comparison_view` | Side-by-side two holdings |
| Action | `order_confirmation` | Buy/sell confirmation (only on explicit request) |

### 19 Contextual Chips

The brain picks 3 per response from a fixed pool:

| Category | Chips |
|----------|-------|
| Portfolio | How am I doing? · Show my allocation · Show my returns |
| Research | Tell me about {holding} · Compare my ETFs |
| Education | What are ETFs? · What are dividends? · What is P/E ratio? · How does diversification work? · Stock vs ETF? |
| Market | What happened? · Am I okay long-term? · Show the bigger picture |
| Catch-up | Catch me up · What did I miss? · Show my dividends · Anything new? |
| Navigation | Back to overview · I still want to sell |

---

## Demo Scenarios

| Scenario | Investor | What happens |
|----------|----------|-------------|
| **Calm Tuesday** | Beginner | Low volatility, evening check-in. Simple overview, educational tone. |
| **Crash Morning** | Beginner | -6.2% week, 7:42am, 2nd session. Reassuring tone, perspective widgets. |
| **Returning** | Expert | 94 days absent, +4.2% growth, dividends. Welcoming catch-up story. |
| **Research Mode** | Expert | 3rd session today, searching semiconductors. Dense, focused layout. |

---

## API

### Dashboard (proactive)
```
GET /api/brain/dashboard/{investorId}?scenario={key}
```

### Interaction (reactive)
```
POST /api/brain/interact/{investorId}
{ "message": "...", "scenario": "...", "conversationHistory": [...] }
```

Both return the same response shape. The frontend doesn't distinguish.

---

## Case Rules (Non-Negotiable)

1. **No investment advice** — inform, don't recommend
2. **No gamification** — no points, badges, streaks
3. **Investor stays in control** — no manipulation or urgency
4. **Beyond the chat bubble** — the UI itself must change shape

---

## Team

- [Alken Rrokaj](https://github.com/alken01)
- [mesyrob](https://github.com/mesyrob)
- [Viktor Gakis](https://github.com/ViktorGakis)
- [Thomas Vrolix](https://github.com/VrolixThomas)

# Architecture: Adaptive Investing UI

> "The Interface That Knows You're Not the Same Person Twice"

## Overview

This application dynamically generates a unique investing dashboard every time a user opens it. Rather than a fixed set of screens, Claude AI acts as the "brain" — it reads the investor's profile, portfolio, market conditions, and emotional state, then composes a personalized UI from a library of 7 adaptive widgets. The frontend renders whatever the brain sends. No two visits are the same.

The system uses a **backend/frontend split**:
- **Backend**: .NET 10 ASP.NET Core API (`backend/`) — data layer, Claude integration, prompt engineering
- **Frontend**: Next.js + Tailwind CSS (`app/`, `components/`, `lib/`) — widget rendering, animations, time-travel key moments

```
┌─────────────────────────────────────────────────────────┐
│                   NEXT.JS FRONTEND                      │
│                   (localhost:3000)                       │
│                                                         │
│  ┌─────────────────────────────────────────────────┐    │
│  │  KeyMomentCards (time-travel triggers)          │    │
│  │  [Calm Evening] [Market Crash] [Long Away] ...  │    │
│  └──────────────────────┬──────────────────────────┘    │
│                         │ click                         │
│  ┌──────────────────────▼──────────────────────────┐    │
│  │  AppShell (state + fetch + animation)           │    │
│  │                                                  │    │
│  │  ┌──────────────────────────────────────────┐   │    │
│  │  │  Narrative headline (animated)           │   │    │
│  │  ├──────────────────────────────────────────┤   │    │
│  │  │  Widget 1 (fade-up, delay 0.1s)         │   │    │
│  │  ├──────────────────────────────────────────┤   │    │
│  │  │  Widget 2 (fade-up, delay 0.25s)        │   │    │
│  │  ├──────────────────────────────────────────┤   │    │
│  │  │  Widget 3 (fade-up, delay 0.4s)         │   │    │
│  │  ├──────────────────────────────────────────┤   │    │
│  │  │  ...more widgets                        │   │    │
│  │  └──────────────────────────────────────────┘   │    │
│  └─────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
                         │
                    POST /api/generate-ui
                         │  (CORS)
┌────────────────────────▼────────────────────────────────┐
│              .NET 10 BACKEND (localhost:5013)            │
│                                                         │
│  ┌──────────────────┐                                   │
│  │ Features/        │                                   │
│  │ GetDashboard     │ ← Minimal API endpoint            │
│  └────────┬─────────┘                                   │
│           │                                             │
│  ┌────────▼─────────┐                                   │
│  │ BrainService     │ ← Orchestrates everything         │
│  └────────┬─────────┘                                   │
│           │                                             │
│  ┌────────▼─────────────────────────────────────────┐   │
│  │ Data Layer          │  Claude Layer              │   │
│  │                     │                            │   │
│  │ EfAccountRepository │  PromptBuilder             │   │
│  │ ScenarioRepository  │  ClaudeClient ──► Claude   │   │
│  │ StockRepository     │  ClaudeOptions    API      │   │
│  └─────────────────────┴────────────────────────────┘   │
│                                                         │
│  Other endpoints:                                       │
│  GET /api/scenarios    — list available scenarios        │
│  GET /api/health       — health check                   │
└─────────────────────────────────────────────────────────┘
```

---

## Data Flow

Every interaction follows this exact path:

```
1. User clicks a key moment card (time-travel trigger)
       ↓
2. AppShell triggers dashboard generation for the account
       ↓
3. POST /api/generate-ui  { accountId: "abc-123", scenarioId: "calm-evening" }
       ↓
4. .NET backend (GetDashboardHandler) resolves data:
   ├── EfAccountRepository.GetById("abc-123")          → account profile (SQLite)
   ├── ScenarioRepository.GetSignals("calm-evening")   → signals
   ├── StockRepository.GetPortfolio("abc-123")         → holdings
   └── StockRepository.GetAllStocks()                  → ASML detail data
       ↓
5. BrainService orchestrates the Claude call
       ↓
6. PromptBuilder.BuildSystemPrompt() + BuildUserPrompt() → combined prompt
       ↓
7. ClaudeClient.GenerateLayoutAsync() → HTTP call to Anthropic API
       ↓
8. Claude receives prompt, decides:
   ├── Which 3-6 widgets to show (out of 7 available)
   ├── What order to show them
   ├── What data/props to pass each widget
   ├── What narrative headline to write
   ├── What mood and experience level to use
       ↓
9. Claude returns UILayout JSON:
   {
     "narrative": "Good evening. Everything's steady.",
     "mood": "calm",
     "experience": "beginner",
     "widgets": [
       { "type": "narrative_card", "props": { ... } },
       { "type": "portfolio_summary", "props": { ... } },
       { "type": "insight_card", "props": { ... } }
     ]
   }
       ↓
10. AppShell receives layout, sets state
       ↓
11. Framer Motion animates:
    ├── Narrative fades in (0.4s)
    ├── Widget 1 slides up (delay 0.1s)
    ├── Widget 2 slides up (delay 0.25s)
    └── Widget N slides up (delay 0.1 + N*0.15s)
       ↓
12. WidgetRenderer routes each widget.type to its React component
       ↓
13. Each widget reads `experience` and `mood` to adapt rendering
```

---

## Project Structure

```
team-25/
│
├── backend/                                    # .NET 10 BACKEND
│   ├── src/
│   │   └── InvestSuite.Api/
│   │       ├── Program.cs                      # Minimal API setup, CORS, DI
│   │       ├── Features/
│   │       │   └── GetDashboard/
│   │       │       ├── GetDashboardHandler.cs   # Request handler
│   │       │       └── GetDashboardRequest.cs   # { ScenarioId }
│   │       ├── Infrastructure/
│   │       │   ├── Data/
│   │       │   │   ├── DataLoader.cs            # DI registration
│   │       │   │   ├── EfAccountRepository.cs    # SQLite-backed account data
│   │       │   │   ├── ScenarioRepository.cs    # 4 scenarios + signals
│   │       │   │   └── StockRepository.cs       # Stocks + portfolios
│   │       │   ├── Claude/
│   │       │   │   ├── ClaudeClient.cs          # HTTP client → Anthropic API
│   │       │   │   ├── ClaudeOptions.cs          # API key, model, config
│   │       │   │   └── PromptBuilder.cs          # System + user prompt builders
│   │       │   └── Services/
│   │       │       ├── BrainService.cs           # Orchestrates data + Claude
│   │       │       └── ChartDataGenerator.cs     # Mock price/portfolio history
│   │       └── Models/
│   │           ├── Investor.cs
│   │           ├── Holding.cs
│   │           ├── ContextSignals.cs
│   │           ├── StockDetail.cs
│   │           ├── ScenarioOption.cs
│   │           ├── UILayout.cs
│   │           └── WidgetConfig.cs
│   ├── tests/
│   │   └── InvestSuite.Tests/
│   │       ├── Features/
│   │       │   └── GetDashboardHandlerTests.cs
│   │       └── Infrastructure/
│   │           ├── PromptBuilderTests.cs
│   │           └── ChartDataGeneratorTests.cs
│   ├── InvestSuite.sln
│   └── Dockerfile
│
├── app/                                        # NEXT.JS FRONTEND
│   ├── layout.tsx                              # Root HTML, Inter font, viewport
│   ├── page.tsx                                # Renders <AppShell />
│   ├── globals.css                             # Tailwind theme + mood CSS
│   └── api/
│       └── generate-ui/
│           └── route.ts                        # Legacy API route (fallback)
│
├── components/                                 # React widget components
│   ├── AppShell.tsx                            # State, fetch, animation
│   ├── KeyMomentCards.tsx                      # Time-travel key moment triggers
│   └── widgets/
│       ├── WidgetRenderer.tsx                  # Routes type → component
│       ├── PortfolioSummary.tsx
│       ├── StockCard.tsx
│       ├── MarketPulse.tsx
│       ├── NarrativeCard.tsx
│       ├── HistoricalContext.tsx
│       ├── InsightCard.tsx
│       └── WelcomeBackCard.tsx
│
├── lib/                                        # Frontend types + data
│   ├── types.ts                                # TypeScript interfaces
│   ├── ai/                                     # Legacy AI provider (fallback)
│   └── data/                                   # Mock data (shared reference)
│
├── data/                                       # Raw CSV source files
├── docs/                                       # Architecture + plans
│
├── .env.local                                  # NEXT_PUBLIC_API_URL, etc.
├── package.json                                # bun, next, framer-motion
└── tsconfig.json
```

---

## The Two-Axis Adaptive System

Every widget renders differently based on two independent axes:

### Axis 1: Experience Level (WHO the user is)

Stable per user. Determines content complexity.

| Aspect | Beginner | Expert |
|--------|----------|--------|
| Portfolio view | Simple list with descriptions | Allocation bars with percentages |
| Stock data | Name, price, plain English | P/E ratio, analyst consensus, score dots |
| Chart style | Simple line chart concept | Candlestick-ready technical view |
| Language | "This holding is doing well" | "+77.4%, Strong Buy, Target: €820" |
| Metrics shown | Fewer, explained | Dense, abbreviation-heavy |

### Axis 2: Mood State (HOW the user feels right now)

Changes every session. Determines tone, density, and which widgets appear.

| Mood | Trigger | UI Behavior |
|------|---------|-------------|
| **Calm** | Low volatility, routine check | Light, exploratory. Educational content surfaces. Normal spacing. |
| **Anxious** | Market crash, losses, compulsive checking | Fewer widgets, more whitespace. Calming narrative leads. Historical context reassures. |
| **Focused** | Active research, multiple stocks viewed | Dense data, multiple stock cards. Minimal narrative. Tight spacing. |
| **Detached** | Long absence, out of the loop | Welcome Back card leads. Storytelling catch-up flow. Sequential reveal. |

### How Claude Decides

Claude receives both axes as context and makes three decisions:

1. **Widget selection**: Which 3-6 of the 7 widgets to include
2. **Widget ordering**: What comes first sets the tone
3. **Widget configuration**: What data and text to put in each widget

Examples:
- Beginner (anxious, crash): Narrative card first (calming), Historical context (perspective), Portfolio summary (de-emphasized losses) → 3 widgets, lots of breathing room
- Expert (focused, research): Multiple stock cards (ASML with full data), Market pulse (sector breakdown), Portfolio summary (allocation view) → 5 widgets, dense

---

## Widget Library

### 7 widgets, each with beginner/expert variants + mood adaptation:

#### 1. Portfolio Summary (`portfolio_summary`)
**Purpose**: Bird's-eye view of all holdings.
**Props**: `holdings[], totalValue, totalChangePct, changeSinceLastVisit`

- Beginner: Simple list — holding name, description, value, return percentage
- Expert: Allocation bars with percentage breakdown, color-coded by asset type (Stock/ETF/Cash)
- Anxious mood: Shows "still growing" badge when portfolio is up

#### 2. Stock Card (`stock_card`)
**Purpose**: Detail view for a single holding.
**Props**: `name, description, type, value, returnPct, price?, changeTodayPct?, peRatio?, sectorAvgPe?, dividendYieldPct?, analystConsensus?, analystTarget?, scores?`

- Beginner: Name, value, return. Mood-sensitive context message ("Your patience is paying off" or "Short-term dips are normal")
- Expert: Full metrics — P/E with sector comparison, dividend yield, analyst consensus + target price, growth/health/stability score dots

#### 3. Market Pulse (`market_pulse`)
**Purpose**: Current market conditions at a glance.
**Props**: `volatility ("Low"|"Medium"|"High"), summary, sectorHighlights?`

- Visual volatility gauge bar (green/amber/red)
- Beginner: Gauge + one-line summary
- Expert: Gauge + summary + sector breakdown table

#### 4. Narrative Card (`narrative_card`)
**Purpose**: Claude's personalized message — the human touch.
**Props**: `title, message`

- Mood-adaptive colors: blue (calm), amber (anxious), slate (focused), violet (detached)
- Mood-adaptive icons: trend line (calm), shield (anxious), lightning (focused), clock (detached)

#### 5. Historical Context (`historical_context`)
**Purpose**: "This has happened before" — perspective during volatility.
**Props**: `event, description, recoveryMonths?, previousOccurrences?`

- Beginner: Event description + average recovery time
- Expert: + Historical parallels table (date, drop %, recovery timeline)
- Anxious mood: Recovery box highlighted in green

#### 6. Insight Card (`insight_card`)
**Purpose**: Education, tips, or action items.
**Props**: `title, content, category ("education"|"action"|"tip")`

- Three categories with distinct icons: book (education), lightning (action), lightbulb (tip)
- Content generated fresh by Claude each time

#### 7. Welcome Back Card (`welcome_back_card`)
**Purpose**: Catch-up summary for returning users.
**Props**: `daysSinceLastVisit, portfolioChangePct, dividendsReceived?, pendingActions?, summary`

- Dark gradient card (slate-900 → slate-800) — visually distinct
- Grid layout: portfolio change, dividends earned, pending actions
- Claude writes the summary narrative fresh

---

## .NET Backend Architecture

### Layer Overview

```
┌──────────────────────────────────────────────┐
│  Features/GetDashboard                       │
│  GetDashboardHandler ← Minimal API endpoint  │
└──────────────┬───────────────────────────────┘
               │
┌──────────────▼───────────────────────────────┐
│  Services/BrainService                       │
│  Orchestrates: data lookup → prompt → Claude │
└──────────┬────────────────────┬──────────────┘
           │                    │
┌──────────▼──────────┐  ┌─────▼──────────────┐
│  Data Layer         │  │  Claude Layer       │
│                     │  │                     │
│  EfAccountRepository│  │  PromptBuilder      │
│  ScenarioRepository │  │  ClaudeClient       │
│  StockRepository    │  │  ClaudeOptions      │
│  DataLoader         │  │                     │
└─────────────────────┘  └─────────────────────┘
```

### Key Components

**GetDashboardHandler** — Validates the scenario ID, delegates to BrainService, returns `UILayout` JSON or error.

**BrainService** — The orchestrator. Loads investor profile, portfolio, and context signals from repositories, builds prompts via PromptBuilder, calls ClaudeClient, returns the result.

**ClaudeClient** — HTTP client that calls the Anthropic Messages API (`POST /v1/messages`). Sends system + user prompt, extracts JSON from the response text, deserializes into `UILayout`.

**PromptBuilder** — Builds the system prompt (widget catalog, rules, JSON format) and user prompt (investor data, portfolio, signals). Same logic as the TypeScript version.

**ChartDataGenerator** — Generates mock price history and portfolio value history data points for chart widgets.

**Repositories** — `EfAccountRepository` (SQLite-backed, accounts created via signup + onboarding), `ScenarioRepository` (key moment scenarios + signals), `StockRepository` (stocks + portfolios).

### Configuration

The .NET backend reads from `appsettings.json`:

```json
{
  "AllowedOrigins": "http://localhost:3000",
  "Claude": {
    "ApiKey": "your-key-here",
    "Model": "claude-sonnet-4-6",
    "BaseUrl": "https://api.anthropic.com",
    "MaxTokens": 4096
  }
}
```

CORS is configured to allow the Next.js frontend origin.

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/generate-ui` | Accepts `{ accountId, scenarioId }`, returns `UILayout` JSON |
| GET | `/api/scenarios` | Returns the available key moment scenarios |
| GET | `/api/health` | Health check |

### Running

```bash
cd backend
dotnet run --project src/InvestSuite.Api
# Runs on http://localhost:5013
```

### Tests (11 passing)

```bash
cd backend
dotnet test
```

- `GetDashboardHandlerTests` — validates request handling
- `PromptBuilderTests` — verifies prompts contain all widgets, rules, investor data
- `ChartDataGeneratorTests` — verifies chart data generation

---

## Frontend → Backend Connection

The Next.js frontend calls the .NET backend via `NEXT_PUBLIC_API_URL` env var:

```
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5013
```

AppShell.tsx reads this at runtime:
```typescript
const apiUrl = process.env.NEXT_PUBLIC_API_URL || "";
const res = await fetch(`${apiUrl}/api/generate-ui`, { ... });
```

If `NEXT_PUBLIC_API_URL` is empty, it falls back to the legacy Next.js API route.

---

## Legacy: Next.js AI Provider (Fallback)

The original Next.js API route and AI provider layer (`lib/ai/`) still exist as a fallback. If `NEXT_PUBLIC_API_URL` is not set, the frontend calls `/api/generate-ui` on the Next.js server, which uses either CLI or API mode.

| Mode | Config | How it works |
|------|--------|-------------|
| CLI | `AI_PROVIDER=cli` | Spawns `claude -p` subprocess — no API key needed |
| API | `AI_PROVIDER=api` | Calls Anthropic API via `@anthropic-ai/sdk` |

### Prompt Engineering (shared across both backends)

**System Prompt**:
- Defines Claude's role as the "adaptive UI brain"
- Lists all 7 widget types with their exact prop schemas
- Explains experience levels and mood states
- Sets rules: no investment advice, no gamification, always vary the layout
- Specifies exact JSON output format

**User Prompt**:
- Injects the specific investor profile (name, age, experience, personality)
- Includes full portfolio holdings as JSON
- Lists all context signals for the current scenario
- Provides available stock details (ASML data)
- Reminds Claude to pick 3-6 widgets and keep it unique

---

## Frontend Component Architecture

```
<RootLayout>                    (app/layout.tsx — server component)
  └── <AppShell>                (client component — manages all state)
        ├── <KeyMomentCards>    (time-travel triggers)
        │
        ├── [Empty state]       (no scenario selected)
        ├── [Loading state]     (spinner + "Composing your experience...")
        ├── [Error state]       (retry button)
        │
        └── [Layout state]      (when UILayout is received)
              ├── <motion.p>    (animated narrative headline)
              └── {widgets.map} (staggered animation)
                    └── <WidgetRenderer>
                          ├── <PortfolioSummary />
                          ├── <StockCard />
                          ├── <MarketPulse />
                          ├── <NarrativeCard />
                          ├── <HistoricalContext />
                          ├── <InsightCard />
                          └── <WelcomeBackCard />
```

### State Management

AppShell manages 4 pieces of state:

```typescript
selectedScenario: string | null    // Which key moment is active
layout: UILayout | null            // The AI-generated layout
loading: boolean                   // Show spinner during generation
error: string | null               // Error message if generation fails
```

No external state library. No context providers. Just `useState` — appropriate for this scope.

### Animation Strategy

Framer Motion handles all transitions:

- **Narrative headline**: Fade in + slide up (0.4s)
- **Each widget**: Fade in + slide up (0.4s), staggered by 150ms per widget
- **Easing**: Custom cubic bezier `[0.25, 0.46, 0.45, 0.94]` — smooth deceleration
- **AnimatePresence**: Handles widget removal when switching key moments

---

## Design System

### Colors (defined as CSS custom properties in globals.css)

```
Background:  #f8fafc (slate-50)      — app background
Surface:     #ffffff                  — card backgrounds
Text:        #0f172a (primary)        — headings, values
             #64748b (secondary)      — descriptions, labels
             #94a3b8 (tertiary)       — hints, timestamps
Border:      #e2e8f0                  — subtle card dividers
Gain:        #059669 (emerald-600)    — positive returns
Gain-light:  #d1fae5                  — positive badge background
Loss:        #e11d48 (rose-600)       — negative returns
Loss-light:  #ffe4e6                  — negative badge background
Accent:      #3b82f6 (blue-500)      — ETF bars, links
```

### Typography

- Font: Inter (Google Fonts), fallback to SF Pro Display, system fonts
- Hierarchy: 4xl bold (portfolio total) → lg semibold (card titles) → sm (body) → xs (labels)
- Line height: relaxed for body text, tight for large numbers

### Card Design

- Background: white (`bg-surface`)
- Border radius: `rounded-2xl` (1rem)
- Shadow: `shadow-sm` (subtle depth)
- Padding: `p-6` (1.5rem)
- No visible borders (depth through shadow only)

### Mood CSS Classes

Applied to the widget container, available for mood-specific styling:

```css
.mood-calm     { --mood-spacing: 1.5rem;  --mood-radius: 1rem;   }
.mood-anxious  { --mood-spacing: 2rem;    --mood-radius: 1.25rem; }
.mood-focused  { --mood-spacing: 1rem;    --mood-radius: 0.75rem; }
.mood-detached { --mood-spacing: 1.75rem; --mood-radius: 1rem;    }
```

### Mobile-First

- Max width: `max-w-lg` (32rem / 512px) centered
- All layouts are single-column, scroll-based
- Touch targets: key moment cards with adequate padding
- Responsive card grid layout

---

## Test Scenarios

### Calm Tuesday Evening (Beginner)
**Signals**: 12 sessions/month, 2 days since last visit, +0.8% 7d, low volatility, 20:15
**Expected mood**: Calm
**Expected widgets**: Portfolio summary, maybe an insight card (educational), light narrative

### Morning After Crash (Beginner)
**Signals**: 12 sessions/month, 0 days (2nd visit today), -6.2% 7d, high volatility, 07:42
**Expected mood**: Anxious
**Expected widgets**: Narrative card (calming), historical context (recovery perspective), portfolio summary (de-emphasized)

### Returning After 3 Months (Expert)
**Signals**: 0 sessions/month, 94 days since last visit, +4.2% since visit, dividends received, expired orders
**Expected mood**: Detached
**Expected widgets**: Welcome back card (catch-up), portfolio summary (since-last-visit delta), insight card (pending actions)

### Research Mode (Expert)
**Signals**: 22 sessions/month, 0 days (3rd visit today), +1.9% 7d, searched "european semiconductor", viewed 4 stocks in 8 min
**Expected mood**: Focused
**Expected widgets**: Multiple stock cards (ASML with full data), market pulse (sector view), portfolio summary (allocation)

---

## Tech Stack

### Backend (.NET)

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | ASP.NET Core (Minimal API) | .NET 10 |
| Language | C# | 13 |
| HTTP Client | System.Net.Http | built-in |
| Testing | xUnit + FluentAssertions + Moq | latest |
| AI Model | Claude Sonnet 4.6 | claude-sonnet-4-6 |

### Frontend (Next.js)

| Layer | Technology | Version |
|-------|-----------|---------|
| Framework | Next.js (App Router) | 16.1.6 |
| Runtime | React | 19.2.3 |
| Language | TypeScript | 5.x |
| Styling | Tailwind CSS | 4.x |
| Animation | Framer Motion | 12.35.2 |
| Package Manager | bun | latest |

---

## Constraints (from hackathon rules)

These are embedded in the AI system prompt:

1. **No investment advice** — The app informs, explains, contextualizes. It never recommends buying or selling.
2. **No gamification** — No points, badges, streaks, leaderboards. Engagement comes from genuine value.
3. **Investor stays in control** — No manipulation, no nudging, no false urgency.
4. **Beyond the chat bubble** — The UI itself changes shape. Not just an LLM generating paragraphs.

---

## How to Run

### Option A: Full stack (Backend + Frontend)

Terminal 1 — .NET backend:
```bash
cd backend
dotnet run --project src/InvestSuite.Api
# Runs on http://localhost:5013
```

Terminal 2 — Next.js frontend:
```bash
bun dev
# Runs on http://localhost:3000
# Calls backend via NEXT_PUBLIC_API_URL=http://localhost:5013
```

### Option B: Frontend only (legacy mode)

```bash
# In .env.local, clear or remove NEXT_PUBLIC_API_URL
bun dev
# Uses Next.js API route with CLI or API provider
```

### Running tests

```bash
cd backend && dotnet test    # 11 tests
```

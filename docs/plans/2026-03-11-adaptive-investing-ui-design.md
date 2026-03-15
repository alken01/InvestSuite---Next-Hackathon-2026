# Adaptive Investing UI — Design Document

## Core Concept

"The interface that reads the room." Claude assembles a unique UI composition on every visit by reading context signals (experience, mood, market, time) and selecting from 7 pre-built widget components. The frontend never hardcodes a screen — it renders whatever Claude sends.

## Architecture

```
User opens app → Scenario selector sends context signals
                         ↓
            Next.js API route (POST /api/generate-ui)
                         ↓
            AI Provider (Claude API or Claude Code CLI)
            with tool_use:
              - get_user_profile(userId)
              - get_portfolio(userId)
              - get_market_data()
              - get_context_signals(userId, scenario)
              - get_available_widgets()
                         ↓
            Claude returns JSON layout:
              { narrative, widgets[], mood, experience }
                         ↓
            Frontend renders widget stream with
            mood-adaptive styling + staggered animations
```

## AI Provider Modes

Two interchangeable modes controlled by `AI_PROVIDER` env var:

- **`api`** — Direct Claude API call from Next.js route. Requires `ANTHROPIC_API_KEY`. For demo/production.
- **`cli`** — Spawns `claude` CLI subprocess with same prompt + tools, captures JSON output. No API keys needed. For local dev/testing.

Both modes receive identical prompts and return identical JSON structure.

## Two-Axis Adaptive System

### Experience Axis (beginner / expert)
- Controls widget content complexity
- Beginner: simple line charts, plain English, educational framing
- Expert: candlestick charts, P/E ratios, technical metrics, analyst data
- Set per user profile, stable across sessions

### Mood Axis (calm / anxious / focused / detached)
- Controls widget tone and density
- Anxious: fewer widgets, more whitespace, calming narrative, softer colors
- Focused: dense data, more widgets, tighter spacing, minimal narrative
- Detached: storytelling flow, catch-up sequence, sequential reveal
- Calm: light, exploratory, educational content surfaced
- Changes per session based on context signals

## Widget Library (7 widgets)

### 1. Portfolio Summary
- **Purpose**: Overall portfolio state at a glance
- **Beginner**: Total value + simple % change with color coding
- **Expert**: Allocation donut chart, sector weights, cash ratio

### 2. Stock Card
- **Purpose**: Individual holding detail
- **Beginner**: Company name, line chart, plain-English description
- **Expert**: Candlestick chart, P/E ratio, analyst consensus, health/growth/stability scores

### 3. Market Pulse
- **Purpose**: Current market conditions
- **Beginner**: Simple sentiment gauge + one-line summary
- **Expert**: Volatility index, sector movers, key market data

### 4. Narrative Card
- **Purpose**: Claude's personalized message to the investor
- **Beginner**: Warm, explanatory, reassuring tone
- **Expert**: Concise market brief, action-oriented

### 5. Historical Context
- **Purpose**: "This has happened before" — perspective during volatility
- **Beginner**: Simple timeline showing past events and outcomes
- **Expert**: Recovery statistics, comparable events with data

### 6. Insight Card
- **Purpose**: Education or actionable information
- **Beginner**: "What is an ETF?", learning tips, glossary
- **Expert**: Dividend schedule, tax efficiency, rebalancing suggestions

### 7. Welcome Back Card
- **Purpose**: Catch-up summary after extended absence
- **Beginner**: Friendly summary of what changed while away
- **Expert**: Compact delta view — portfolio change, dividends received, expired orders

## Mood-Adaptive Behavior

### Anxious (e.g. Sophie, crash morning)
- Narrative Card leads with calming perspective
- Historical Context: "markets recovered X times before"
- Portfolio Summary de-emphasizes losses, shows long-term view
- Fewer widgets total, more whitespace, softer color palette

### Focused (e.g. Marc, research mode)
- No narrative fluff — dense data layout
- Multiple Stock Cards with full technical detail
- Market Pulse with sector breakdown
- More widgets, tighter spacing

### Detached (e.g. Marc, returning after 3 months)
- Welcome Back Card leads
- Portfolio Summary with since-last-visit delta
- Insight Card with pending actions (expired orders)
- Storytelling flow, sequential reveal

### Calm (e.g. Sophie, quiet Tuesday)
- Light, exploratory layout
- Insight Card with educational content
- Portfolio Summary with gentle positive framing
- Market Pulse as ambient context

## UI Design Language

- **Mobile-first**: 390px base width, scales up gracefully
- **Typography**: System font stack (SF Pro on iOS, Inter fallback)
- **Spacing**: Generous rhythm — 16/24/32px, cards breathe
- **Cards**: rounded-2xl, shadow-sm, no borders, white on gray-50 background
- **Color**: Neutral base (slate-900 text), emerald for gains, rose for losses
- **Animation**: Staggered fade-up on widget entry (150ms delay between each)
- **Scenario switcher**: Minimal pill selector at top — demo control, not part of app UI

## Tech Stack

- Next.js 14 (App Router)
- Tailwind CSS 4
- bun (package manager + runner)
- Claude API (claude-sonnet-4-6) with tool_use
- TypeScript
- Framer Motion (widget entry animations)
- Mock data from CSV files loaded as JSON constants

## Demo Flow

Scenario selector at top with 4 prebuilt scenarios:
1. Sophie — Calm Tuesday Evening
2. Sophie — Morning After Crash
3. Marc — Returning After 3 Months
4. Marc — Research Mode

Each tap fires a fresh Claude call. UI rebuilds live. Every run produces a different composition because Claude generates narrative, widget selection, ordering, and configuration fresh each time.

## Rules (from hackathon case)

- No investment advice — inform, explain, contextualize only
- No gamification — no points, badges, streaks
- Investor stays in control — no manipulation or false urgency
- Beyond the chat bubble — UI itself changes shape, not just text

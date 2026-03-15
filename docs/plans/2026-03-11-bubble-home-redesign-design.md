# Bubble-Based Adaptive Home — Design

**Goal:** Redesign the app from a card-list layout to a bubble-based exploration UI with a dark theme, two-phase loading, and modal detail overlays.

## Architecture

Three layers:

1. **Hero zone** — full viewport height. Centered portfolio value, AI narrative (italic), floating holding bubbles with slow drift animation, suggestion pills.
2. **Feed zone** — below the fold. Claude-generated cards (narrative, market pulse, insights, historical context) in dark theme. No portfolio/stock detail cards here.
3. **Bubble detail overlay** — modal slides up when tapping a bubble. Detailed stock/holding view with price, return explanation, metrics, educational content, action buttons. Dismissible.

Scenario selector moves to a subtle top-corner control (demo tool, not user-facing).

## Two-Phase Loading

**Phase 1 — Instant (no Claude):**
- `GET /api/hero/{scenarioId}` — returns portfolio total, weekly change, holdings for bubbles
- Hero renders immediately: value, bubbles floating, shimmer placeholders for narrative/pills

**Phase 2 — AI (async, parallel):**
- `POST /api/generate-ui` fires while hero is visible
- Claude generates: narrative, suggestion pills, bubble details, feed widgets
- Content fades in as it arrives
- If user taps bubble before AI ready: shimmer in detail overlay

## Layout Schema

Claude's `UILayout` response expands:

```json
{
  "narrative": "Quiet week. Your World ETF did the heavy lifting.",
  "mood": "calm",
  "experience": "beginner",
  "suggestionPills": [
    { "label": "Tell me about ASML", "targetBubble": "ASML" },
    { "label": "Why is it up?", "targetBubble": "ASML" }
  ],
  "bubbleDetails": {
    "ASML": {
      "title": "ASML Holding",
      "subtitle": "Semiconductor equipment · Netherlands",
      "price": 745.20,
      "changeTodayPct": 1.2,
      "description": "They make the only machines that can print the tiniest chips.",
      "returnExplanation": "You invested around €440 in ASML. It's now worth €482...",
      "metrics": { "peRatio": 35, "dividendYieldPct": 0.7, "marketCapB": 301 },
      "educationalNote": { "title": "What's P/E?", "content": "It means investors pay €35 for every €1 of profit..." }
    }
  },
  "widgets": [
    { "type": "narrative_card", "props": { ... } },
    { "type": "market_pulse", "props": { ... } },
    { "type": "insight_card", "props": { ... } }
  ]
}
```

- **`suggestionPills`** — 2-4 contextual pills, each links to a bubble's detail overlay
- **`bubbleDetails`** — expanded content per holding, pre-generated in the same API call
- **`widgets`** — feed cards. Excludes portfolio_summary, stock_card, welcome_back_card (replaced by hero/bubbles/narrative)

## Component Structure

**New:**
- `HeroZone` — full viewport. Portfolio value, shimmer placeholders for narrative/pills, contains BubbleField
- `BubbleField` — floating bubbles area with CSS drift animation
- `Bubble` — single circle. Name + return %. Tappable. Green/red tint.
- `BubbleDetailOverlay` — bottom-sheet modal. Stock detail view. Dismissible.
- `SuggestionPills` — horizontal pill row linking to bubble details
- `FeedZone` — renders feed widget cards in dark theme

**Modified:**
- `AppShell` — orchestrates two-phase load, scenario selector becomes subtle top control
- All existing feed widgets — restyled for dark theme

**Removed from feed:**
- `PortfolioSummary` → replaced by hero zone
- `StockCard` → replaced by bubble detail overlay
- `WelcomeBackCard` → narrative handles welcome-back

## Visual Design

**Dark theme:**
- Background: `#0a0f1a`
- Cards: `rgba(255,255,255,0.05)` with `rgba(255,255,255,0.1)` border
- Text: white primary, 50% white secondary, 30% white tertiary
- Gains: `#4ade80`, Losses: `#f87171`
- Narrative: italic, secondary color

**Bubbles:**
- Circles with `rgba(255,255,255,0.15)` border, semi-transparent fill
- Sizes: large ~120px, medium ~80px, small ~60px (based on holding value)
- Slow CSS drift animation: 15-20s cycle, each bubble offset
- Name + return % centered inside

**Bubble detail overlay:**
- Slides up from bottom, dark background
- Backdrop blur/dim
- Green left border on description quote
- 3-column metric grid
- Educational notes in lighter card

**Feed cards:**
- 16px rounded corners, dark glass style
- Mood accent colors (muted for dark bg)
- Staggered fade-up on scroll (intersection observer)

**Scenario selector:**
- Small icon/dropdown in top corner

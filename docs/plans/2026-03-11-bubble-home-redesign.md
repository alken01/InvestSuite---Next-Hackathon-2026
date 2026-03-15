# Bubble Home Redesign — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Redesign the app from a card-list layout to a dark-themed bubble-based exploration UI with two-phase loading, floating holding bubbles, suggestion pills, modal detail overlays, and a curated feed below the fold.

**Architecture:** The hero zone renders instantly from a new `GET /api/hero/{scenarioId}` endpoint (no AI). Meanwhile `POST /api/generate-ui` fires async to get Claude's narrative, suggestion pills, bubble details, and feed widgets. New frontend components (HeroZone, BubbleField, Bubble, BubbleDetailOverlay, SuggestionPills, FeedZone) replace the old card-list layout. The entire app switches to a dark theme.

**Tech Stack:** Next.js, Tailwind CSS, Framer Motion, .NET 10 backend

---

## Task 1: Dark Theme + Updated Type System

**Files:**
- Modify: `app/globals.css`
- Modify: `lib/types.ts`
- Modify: `backend/src/InvestSuite.Api/Models/UILayout.cs`

**Step 1: Replace globals.css with dark theme**

`app/globals.css`:
```css
@import "tailwindcss";

@theme {
  --font-sans: "Inter", "SF Pro Display", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;

  --color-gain: #4ade80;
  --color-gain-light: rgba(74, 222, 128, 0.15);
  --color-loss: #f87171;
  --color-loss-light: rgba(248, 113, 113, 0.15);
  --color-surface: rgba(255, 255, 255, 0.05);
  --color-surface-hover: rgba(255, 255, 255, 0.08);
  --color-background: #0a0f1a;
  --color-text-primary: #ffffff;
  --color-text-secondary: rgba(255, 255, 255, 0.5);
  --color-text-tertiary: rgba(255, 255, 255, 0.3);
  --color-border: rgba(255, 255, 255, 0.1);
  --color-accent: #60a5fa;
  --color-accent-light: rgba(96, 165, 250, 0.15);
}

html {
  font-family: var(--font-sans);
  background-color: var(--color-background);
  color: var(--color-text-primary);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

.mood-calm {
  --mood-spacing: 1.5rem;
  --mood-radius: 1rem;
  --mood-opacity: 1;
}

.mood-anxious {
  --mood-spacing: 2rem;
  --mood-radius: 1.25rem;
  --mood-opacity: 0.95;
}

.mood-focused {
  --mood-spacing: 1rem;
  --mood-radius: 0.75rem;
  --mood-opacity: 1;
}

.mood-detached {
  --mood-spacing: 1.75rem;
  --mood-radius: 1rem;
  --mood-opacity: 1;
}

.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}

/* Bubble floating animations */
@keyframes float-1 {
  0%, 100% { transform: translate(0, 0); }
  25% { transform: translate(8px, -12px); }
  50% { transform: translate(-5px, -8px); }
  75% { transform: translate(10px, 5px); }
}

@keyframes float-2 {
  0%, 100% { transform: translate(0, 0); }
  25% { transform: translate(-10px, 8px); }
  50% { transform: translate(6px, 12px); }
  75% { transform: translate(-8px, -5px); }
}

@keyframes float-3 {
  0%, 100% { transform: translate(0, 0); }
  25% { transform: translate(12px, 5px); }
  50% { transform: translate(-8px, -10px); }
  75% { transform: translate(5px, 8px); }
}

.animate-float-1 { animation: float-1 18s ease-in-out infinite; }
.animate-float-2 { animation: float-2 22s ease-in-out infinite; }
.animate-float-3 { animation: float-3 15s ease-in-out infinite; }
```

**Step 2: Update types.ts — add new types for bubbles, pills, bubble details**

Add the following types to the end of `lib/types.ts` (keep all existing types, modify `UILayout`):

```typescript
// --- Bubble Home Types ---

export interface BubbleData {
  name: string;
  value: number;
  returnPct: number;
  size: "large" | "medium" | "small";
}

export interface SuggestionPill {
  label: string;
  targetBubble: string;
}

export interface BubbleDetail {
  title: string;
  subtitle: string;
  price: number;
  changeTodayPct: number;
  description: string;
  returnExplanation: string;
  metrics: {
    peRatio?: number;
    dividendYieldPct?: number;
    marketCapB?: number;
  };
  educationalNote?: {
    title: string;
    content: string;
  };
}

export interface HeroData {
  investorName: string;
  portfolioTotal: number;
  portfolioChange7dPct: number | null;
  holdings: BubbleData[];
}

// Updated UILayout — now includes suggestion pills and bubble details
export interface UILayout {
  narrative: string;
  widgets: WidgetProps[];
  mood: MoodState;
  experience: ExperienceLevel;
  suggestionPills?: SuggestionPill[];
  bubbleDetails?: Record<string, BubbleDetail>;
}
```

**Step 3: Update backend UILayout.cs to match**

`backend/src/InvestSuite.Api/Models/UILayout.cs`:
```csharp
using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record UILayout(
    [property: JsonPropertyName("narrative")] string Narrative,
    [property: JsonPropertyName("widgets")] List<WidgetConfig> Widgets,
    [property: JsonPropertyName("mood")] string Mood,
    [property: JsonPropertyName("experience")] string Experience,
    [property: JsonPropertyName("suggestionPills")] List<SuggestionPill>? SuggestionPills = null,
    [property: JsonPropertyName("bubbleDetails")] Dictionary<string, BubbleDetail>? BubbleDetails = null
);

public record SuggestionPill(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("targetBubble")] string TargetBubble
);

public record BubbleDetail(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("subtitle")] string Subtitle,
    [property: JsonPropertyName("price")] decimal Price,
    [property: JsonPropertyName("changeTodayPct")] decimal ChangeTodayPct,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("returnExplanation")] string ReturnExplanation,
    [property: JsonPropertyName("metrics")] BubbleMetrics Metrics,
    [property: JsonPropertyName("educationalNote")] EducationalNote? EducationalNote = null
);

public record BubbleMetrics(
    [property: JsonPropertyName("peRatio")] decimal? PeRatio = null,
    [property: JsonPropertyName("dividendYieldPct")] decimal? DividendYieldPct = null,
    [property: JsonPropertyName("marketCapB")] decimal? MarketCapB = null
);

public record EducationalNote(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("content")] string Content
);
```

**Step 4: Verify builds**

```bash
cd C:/Users/thoma/Documents/team-25/backend && dotnet build
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 5: Commit**

```bash
git add app/globals.css lib/types.ts backend/src/InvestSuite.Api/Models/UILayout.cs
git commit -m "feat: dark theme and expanded type system for bubble home"
```

---

## Task 2: Hero API Endpoint

**Files:**
- Create: `backend/src/InvestSuite.Api/Features/GetHero/GetHeroHandler.cs`
- Modify: `backend/src/InvestSuite.Api/Program.cs`

**Step 1: Create GetHeroHandler**

`backend/src/InvestSuite.Api/Features/GetHero/GetHeroHandler.cs`:
```csharp
using InvestSuite.Api.Infrastructure.Data;

namespace InvestSuite.Api.Features.GetHero;

public class GetHeroHandler
{
    private readonly InvestorRepository _investors;
    private readonly StockRepository _stocks;
    private readonly ScenarioRepository _scenarios;

    public GetHeroHandler(InvestorRepository investors, StockRepository stocks, ScenarioRepository scenarios)
    {
        _investors = investors;
        _stocks = stocks;
        _scenarios = scenarios;
    }

    public IResult Handle(string scenarioId)
    {
        if (string.IsNullOrWhiteSpace(scenarioId))
            return Results.BadRequest(new { error = "scenarioId is required" });

        var signals = _scenarios.GetSignals(scenarioId);
        if (signals is null)
            return Results.BadRequest(new { error = $"Unknown scenario: {scenarioId}" });

        var investor = _investors.GetById(signals.Investor);
        if (investor is null)
            return Results.NotFound(new { error = $"Unknown investor: {signals.Investor}" });

        var holdings = _stocks.GetPortfolio(investor.Id);
        if (holdings is null)
            return Results.NotFound(new { error = $"No portfolio for: {investor.Id}" });

        // Sort by value descending, take top 6 for bubbles
        var sortedHoldings = holdings
            .Where(h => h.Type != "Cash")
            .OrderByDescending(h => h.Value)
            .Take(6)
            .ToList();

        var maxValue = sortedHoldings.Max(h => h.Value);

        var bubbles = sortedHoldings.Select(h => new
        {
            name = h.Name,
            value = h.Value,
            returnPct = h.ReturnPct,
            size = h.Value >= maxValue * 0.7m ? "large"
                 : h.Value >= maxValue * 0.3m ? "medium"
                 : "small"
        }).ToList();

        return Results.Ok(new
        {
            investorName = investor.Name,
            portfolioTotal = investor.PortfolioTotal,
            portfolioChange7dPct = signals.PortfolioChange7d,
            holdings = bubbles
        });
    }
}
```

**Step 2: Register in Program.cs**

After `builder.Services.AddScoped<GetDashboardHandler>();` add:

```csharp
builder.Services.AddScoped<GetHeroHandler>();
```

After the existing endpoints, add:

```csharp
app.MapGet("/api/hero/{scenarioId}", (string scenarioId, GetHeroHandler handler) =>
    handler.Handle(scenarioId));
```

Add the using at the top:

```csharp
using InvestSuite.Api.Features.GetHero;
```

**Step 3: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend && dotnet build
```

**Step 4: Commit**

```bash
git add backend/src/InvestSuite.Api/Features/GetHero/ backend/src/InvestSuite.Api/Program.cs
git commit -m "feat: add GET /api/hero endpoint for instant hero data"
```

---

## Task 3: Update Prompts for Bubble Home

**Files:**
- Modify: `backend/src/InvestSuite.Api/Infrastructure/Claude/PromptBuilder.cs`
- Modify: `components/widgets/manifests/portfolio_summary.json`
- Modify: `components/widgets/manifests/stock_card.json`
- Modify: `components/widgets/manifests/welcome_back_card.json`

**Step 1: Update PromptBuilder system prompt**

The system prompt needs to tell Claude to generate `suggestionPills` and `bubbleDetails` in addition to feed widgets. Update the JSON response format in both `BuildSystemPrompt()` and `BuildCliPrompt()`.

In `BuildSystemPrompt()`, replace the JSON format section (after "After gathering data via tools, respond with ONLY valid JSON"):

```
After gathering data via tools, respond with ONLY valid JSON (no markdown, no code fences):
{
  "narrative": "A one-sentence greeting or contextual headline (italic style)",
  "mood": "calm" | "anxious" | "focused" | "detached",
  "experience": "beginner" | "expert",
  "suggestionPills": [
    { "label": "Quick suggestion text", "targetBubble": "HoldingName" }
  ],
  "bubbleDetails": {
    "HoldingName": {
      "title": "Full company name",
      "subtitle": "Sector · Country",
      "price": 745.20,
      "changeTodayPct": 1.2,
      "description": "One-sentence company description in plain language",
      "returnExplanation": "Personalized explanation of their return on this holding",
      "metrics": { "peRatio": 35, "dividendYieldPct": 0.7, "marketCapB": 301 },
      "educationalNote": { "title": "What's P/E?", "content": "Beginner-friendly explanation..." }
    }
  },
  "widgets": [
    { "type": "<widget_type>", "props": { ... } }
  ]
}

IMPORTANT:
- Generate 2-4 suggestionPills linking to holdings. Labels should be natural questions.
- Generate bubbleDetails for EVERY non-cash holding in the portfolio.
- For beginners, always include educationalNote in bubbleDetails.
- Widgets are for the feed below the fold — use narrative_card, market_pulse, insight_card, historical_context only. Do NOT use portfolio_summary, stock_card, or welcome_back_card in widgets.
```

Apply the same JSON format update to the `BuildCliPrompt()` method's response format section.

**Step 2: Update manifests — mark portfolio_summary, stock_card, welcome_back_card as bubble-only**

Update `components/widgets/manifests/portfolio_summary.json` description:
```json
{
  "type": "portfolio_summary",
  "description": "DEPRECATED for feed — portfolio is now shown in the hero zone. Do NOT include this in widgets array.",
  "propsSchema": {}
}
```

Update `components/widgets/manifests/stock_card.json` description:
```json
{
  "type": "stock_card",
  "description": "DEPRECATED for feed — stock details are now shown in bubble detail overlays. Do NOT include this in widgets array.",
  "propsSchema": {}
}
```

Update `components/widgets/manifests/welcome_back_card.json` description:
```json
{
  "type": "welcome_back_card",
  "description": "DEPRECATED for feed — welcome-back messaging is now handled by the narrative field. Do NOT include this in widgets array.",
  "propsSchema": {}
}
```

**Step 3: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend && dotnet build
```

**Step 4: Commit**

```bash
git add backend/src/InvestSuite.Api/Infrastructure/Claude/PromptBuilder.cs components/widgets/manifests/
git commit -m "feat: update prompts and manifests for bubble home layout"
```

---

## Task 4: HeroZone + BubbleField + Bubble Components

**Files:**
- Create: `components/HeroZone.tsx`
- Create: `components/BubbleField.tsx`
- Create: `components/Bubble.tsx`
- Create: `components/SuggestionPills.tsx`

**Step 1: Create Bubble component**

`components/Bubble.tsx`:
```tsx
"use client";

import { motion } from "framer-motion";
import { BubbleData } from "@/lib/types";

interface Props {
  bubble: BubbleData;
  onClick: () => void;
  animationClass: string;
}

const sizeMap = {
  large: "w-[120px] h-[120px] text-base",
  medium: "w-[80px] h-[80px] text-sm",
  small: "w-[60px] h-[60px] text-xs",
};

export default function Bubble({ bubble, onClick, animationClass }: Props) {
  const isPositive = bubble.returnPct >= 0;

  return (
    <motion.button
      onClick={onClick}
      whileHover={{ scale: 1.1 }}
      whileTap={{ scale: 0.95 }}
      className={`${sizeMap[bubble.size]} ${animationClass} rounded-full flex flex-col items-center justify-center cursor-pointer transition-colors duration-300`}
      style={{
        background: isPositive
          ? "rgba(74, 222, 128, 0.08)"
          : "rgba(248, 113, 113, 0.08)",
        border: `1px solid ${isPositive ? "rgba(74, 222, 128, 0.25)" : "rgba(248, 113, 113, 0.25)"}`,
      }}
    >
      <span className="font-medium text-text-primary">{bubble.name}</span>
      <span className={`font-medium ${isPositive ? "text-gain" : "text-loss"}`}>
        {isPositive ? "+" : ""}{bubble.returnPct}%
      </span>
    </motion.button>
  );
}
```

**Step 2: Create BubbleField component**

`components/BubbleField.tsx`:
```tsx
"use client";

import { BubbleData } from "@/lib/types";
import Bubble from "./Bubble";

interface Props {
  holdings: BubbleData[];
  onBubbleTap: (name: string) => void;
}

// Pre-defined positions to scatter bubbles naturally
const positions = [
  { left: "15%", top: "10%" },
  { left: "55%", top: "5%" },
  { left: "70%", top: "35%" },
  { left: "10%", top: "50%" },
  { left: "45%", top: "55%" },
  { left: "75%", top: "65%" },
];

const floatClasses = ["animate-float-1", "animate-float-2", "animate-float-3"];

export default function BubbleField({ holdings, onBubbleTap }: Props) {
  return (
    <div className="relative w-full h-[280px]">
      {holdings.map((bubble, i) => (
        <div
          key={bubble.name}
          className="absolute"
          style={{
            left: positions[i % positions.length].left,
            top: positions[i % positions.length].top,
            transform: "translate(-50%, -50%)",
          }}
        >
          <Bubble
            bubble={bubble}
            onClick={() => onBubbleTap(bubble.name)}
            animationClass={floatClasses[i % floatClasses.length]}
          />
        </div>
      ))}
    </div>
  );
}
```

**Step 3: Create SuggestionPills component**

`components/SuggestionPills.tsx`:
```tsx
"use client";

import { motion } from "framer-motion";
import { SuggestionPill } from "@/lib/types";

interface Props {
  pills: SuggestionPill[];
  onTap: (targetBubble: string) => void;
}

export default function SuggestionPills({ pills, onTap }: Props) {
  return (
    <div className="flex flex-wrap gap-2 justify-center">
      {pills.map((pill, i) => (
        <motion.button
          key={i}
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 + i * 0.1 }}
          onClick={() => onTap(pill.targetBubble)}
          className="px-4 py-2 rounded-full text-sm font-medium border border-border text-text-secondary hover:text-text-primary hover:border-text-tertiary transition-colors cursor-pointer"
        >
          {pill.label}
        </motion.button>
      ))}
    </div>
  );
}
```

**Step 4: Create HeroZone component**

`components/HeroZone.tsx`:
```tsx
"use client";

import { motion } from "framer-motion";
import { HeroData, SuggestionPill } from "@/lib/types";
import BubbleField from "./BubbleField";
import SuggestionPills from "./SuggestionPills";

interface Props {
  hero: HeroData;
  narrative: string | null;
  suggestionPills: SuggestionPill[] | null;
  aiLoading: boolean;
  onBubbleTap: (name: string) => void;
}

export default function HeroZone({ hero, narrative, suggestionPills, aiLoading, onBubbleTap }: Props) {
  const changeIsPositive = (hero.portfolioChange7dPct ?? 0) >= 0;

  return (
    <div className="min-h-screen flex flex-col items-center justify-center px-4 relative">
      {/* Portfolio value */}
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        className="text-center mb-2"
      >
        <p className="text-5xl font-light text-text-primary tracking-tight">
          €{hero.portfolioTotal.toLocaleString()}
        </p>
        <p className="text-sm text-text-tertiary mt-1">Your portfolio</p>
        {hero.portfolioChange7dPct !== null && (
          <p className={`text-sm font-medium mt-1 ${changeIsPositive ? "text-gain" : "text-loss"}`}>
            {changeIsPositive ? "+" : ""}{hero.portfolioChange7dPct}% <span className="text-text-tertiary font-normal">this week</span>
          </p>
        )}
      </motion.div>

      {/* Narrative */}
      <div className="h-16 flex items-center justify-center my-6 max-w-xs">
        {aiLoading ? (
          <div className="h-4 w-48 bg-surface rounded animate-pulse" />
        ) : narrative ? (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            transition={{ duration: 0.6 }}
            className="text-center"
          >
            <p className="text-text-secondary italic text-sm leading-relaxed">{narrative}</p>
            <p className="text-text-tertiary text-xs mt-2">Tap a bubble to learn more</p>
          </motion.div>
        ) : null}
      </div>

      {/* Bubble field */}
      <BubbleField holdings={hero.holdings} onBubbleTap={onBubbleTap} />

      {/* Suggestion pills */}
      <div className="mt-8 min-h-[44px]">
        {aiLoading ? (
          <div className="flex gap-2 justify-center">
            <div className="h-9 w-36 bg-surface rounded-full animate-pulse" />
            <div className="h-9 w-28 bg-surface rounded-full animate-pulse" />
          </div>
        ) : suggestionPills && suggestionPills.length > 0 ? (
          <SuggestionPills pills={suggestionPills} onTap={onBubbleTap} />
        ) : null}
      </div>

      {/* Scroll hint */}
      <motion.div
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ delay: 1.5 }}
        className="absolute bottom-8"
      >
        <div className="w-5 h-8 border-2 border-text-tertiary rounded-full flex justify-center pt-1.5">
          <motion.div
            animate={{ y: [0, 8, 0] }}
            transition={{ duration: 1.5, repeat: Infinity }}
            className="w-1 h-1 bg-text-tertiary rounded-full"
          />
        </div>
      </motion.div>
    </div>
  );
}
```

**Step 5: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 6: Commit**

```bash
git add components/Bubble.tsx components/BubbleField.tsx components/SuggestionPills.tsx components/HeroZone.tsx
git commit -m "feat: add HeroZone, BubbleField, Bubble, SuggestionPills components"
```

---

## Task 5: BubbleDetailOverlay Component

**Files:**
- Create: `components/BubbleDetailOverlay.tsx`

**Step 1: Create BubbleDetailOverlay**

`components/BubbleDetailOverlay.tsx`:
```tsx
"use client";

import { motion, AnimatePresence } from "framer-motion";
import { BubbleDetail, ExperienceLevel } from "@/lib/types";

interface Props {
  detail: BubbleDetail | null;
  portfolioTotal: number;
  portfolioChangePct: number | null;
  experience: ExperienceLevel;
  open: boolean;
  onClose: () => void;
}

export default function BubbleDetailOverlay({ detail, portfolioTotal, portfolioChangePct, experience, open, onClose }: Props) {
  return (
    <AnimatePresence>
      {open && (
        <>
          {/* Backdrop */}
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            exit={{ opacity: 0 }}
            onClick={onClose}
            className="fixed inset-0 bg-black/60 backdrop-blur-sm z-40"
          />

          {/* Panel */}
          <motion.div
            initial={{ y: "100%" }}
            animate={{ y: 0 }}
            exit={{ y: "100%" }}
            transition={{ type: "spring", damping: 30, stiffness: 300 }}
            className="fixed inset-x-0 bottom-0 z-50 max-h-[90vh] overflow-y-auto"
          >
            <div className="max-w-lg mx-auto bg-background border-t border-border rounded-t-3xl px-5 pt-6 pb-8">
              {/* Handle */}
              <div className="w-10 h-1 bg-text-tertiary rounded-full mx-auto mb-6" />

              {/* Portfolio context (faded) */}
              <div className="text-center mb-6">
                <p className="text-2xl font-light text-text-tertiary">
                  €{portfolioTotal.toLocaleString()}
                </p>
                {portfolioChangePct !== null && (
                  <p className={`text-xs ${(portfolioChangePct ?? 0) >= 0 ? "text-gain" : "text-loss"}`}>
                    {(portfolioChangePct ?? 0) >= 0 ? "+" : ""}{portfolioChangePct}%
                  </p>
                )}
              </div>

              {detail ? (
                <>
                  {/* Header */}
                  <div className="flex items-baseline justify-between mb-1">
                    <h2 className="text-xl font-semibold text-text-primary">{detail.title}</h2>
                    <div className="text-right">
                      <span className="text-lg font-semibold text-text-primary">€{detail.price}</span>
                      <span className={`text-sm ml-1 ${detail.changeTodayPct >= 0 ? "text-gain" : "text-loss"}`}>
                        {detail.changeTodayPct >= 0 ? "+" : ""}{detail.changeTodayPct}%
                      </span>
                    </div>
                  </div>
                  <p className="text-sm text-text-tertiary mb-4">{detail.subtitle}</p>

                  {/* Description */}
                  <div className="border-l-2 border-gain pl-4 mb-5">
                    <p className="text-sm text-text-secondary italic leading-relaxed">{detail.description}</p>
                  </div>

                  {/* Return explanation */}
                  <div className="bg-surface border border-border rounded-2xl p-4 mb-5">
                    <p className="text-sm font-semibold text-gain mb-1">Your return: {detail.changeTodayPct >= 0 ? "+" : ""}{detail.changeTodayPct}%</p>
                    <p className="text-sm text-text-secondary leading-relaxed">{detail.returnExplanation}</p>
                  </div>

                  {/* Metrics */}
                  {experience === "expert" || (detail.metrics.peRatio || detail.metrics.dividendYieldPct || detail.metrics.marketCapB) ? (
                    <div className="grid grid-cols-3 gap-3 mb-5">
                      {detail.metrics.peRatio && (
                        <div className="bg-surface border border-border rounded-xl p-3 text-center">
                          <p className="text-lg font-semibold text-text-primary">{detail.metrics.peRatio}</p>
                          <p className="text-xs text-text-tertiary uppercase tracking-wider">P/E Ratio</p>
                        </div>
                      )}
                      {detail.metrics.dividendYieldPct !== undefined && detail.metrics.dividendYieldPct !== null && (
                        <div className="bg-surface border border-border rounded-xl p-3 text-center">
                          <p className="text-lg font-semibold text-text-primary">{detail.metrics.dividendYieldPct}%</p>
                          <p className="text-xs text-text-tertiary uppercase tracking-wider">Dividend</p>
                        </div>
                      )}
                      {detail.metrics.marketCapB && (
                        <div className="bg-surface border border-border rounded-xl p-3 text-center">
                          <p className="text-lg font-semibold text-text-primary">€{detail.metrics.marketCapB}B</p>
                          <p className="text-xs text-text-tertiary uppercase tracking-wider">Market Cap</p>
                        </div>
                      )}
                    </div>
                  ) : null}

                  {/* Educational note */}
                  {detail.educationalNote && (
                    <div className="bg-surface border border-border rounded-2xl p-4 mb-5">
                      <p className="text-sm font-semibold text-accent mb-1">{detail.educationalNote.title}</p>
                      <p className="text-sm text-text-secondary leading-relaxed">{detail.educationalNote.content}</p>
                    </div>
                  )}

                  {/* Actions */}
                  <div className="flex gap-3">
                    <button
                      onClick={onClose}
                      className="flex-1 px-4 py-3 rounded-xl border border-border text-sm font-medium text-text-secondary hover:text-text-primary transition-colors"
                    >
                      Back to home
                    </button>
                  </div>
                </>
              ) : (
                /* Loading shimmer */
                <div className="space-y-4">
                  <div className="h-6 w-40 bg-surface rounded animate-pulse" />
                  <div className="h-4 w-60 bg-surface rounded animate-pulse" />
                  <div className="h-20 bg-surface rounded-2xl animate-pulse" />
                  <div className="h-16 bg-surface rounded-2xl animate-pulse" />
                </div>
              )}
            </div>
          </motion.div>
        </>
      )}
    </AnimatePresence>
  );
}
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 3: Commit**

```bash
git add components/BubbleDetailOverlay.tsx
git commit -m "feat: add BubbleDetailOverlay modal component"
```

---

## Task 6: FeedZone Component

**Files:**
- Create: `components/FeedZone.tsx`

**Step 1: Create FeedZone**

`components/FeedZone.tsx`:
```tsx
"use client";

import { motion } from "framer-motion";
import { WidgetProps, ExperienceLevel, MoodState } from "@/lib/types";
import WidgetRenderer from "./widgets/WidgetRenderer";

interface Props {
  widgets: WidgetProps[];
  experience: ExperienceLevel;
  mood: MoodState;
  loading: boolean;
}

export default function FeedZone({ widgets, experience, mood, loading }: Props) {
  if (loading) {
    return (
      <div className="max-w-lg mx-auto px-4 py-8 space-y-4">
        {[1, 2, 3].map(i => (
          <div key={i} className="h-32 bg-surface border border-border rounded-2xl animate-pulse" />
        ))}
      </div>
    );
  }

  if (!widgets || widgets.length === 0) return null;

  // Filter out deprecated widget types from feed
  const feedWidgets = widgets.filter(
    w => !["portfolio_summary", "stock_card", "welcome_back_card"].includes(w.type)
  );

  if (feedWidgets.length === 0) return null;

  return (
    <div className={`max-w-lg mx-auto px-4 py-8 space-y-4 mood-${mood}`}>
      {feedWidgets.map((widget, index) => (
        <motion.div
          key={`${widget.type}-${index}`}
          initial={{ opacity: 0, y: 20 }}
          whileInView={{ opacity: 1, y: 0 }}
          viewport={{ once: true, margin: "-50px" }}
          transition={{
            duration: 0.4,
            delay: index * 0.1,
            ease: [0.25, 0.46, 0.45, 0.94],
          }}
        >
          <WidgetRenderer widget={widget} experience={experience} mood={mood} />
        </motion.div>
      ))}
    </div>
  );
}
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 3: Commit**

```bash
git add components/FeedZone.tsx
git commit -m "feat: add FeedZone component for scroll-down widget feed"
```

---

## Task 7: Restyle Existing Feed Widgets for Dark Theme

**Files:**
- Modify: `components/widgets/NarrativeCard.tsx`
- Modify: `components/widgets/MarketPulse.tsx`
- Modify: `components/widgets/InsightCard.tsx`
- Modify: `components/widgets/HistoricalContext.tsx`

**Step 1: Update all four feed widgets**

For each widget, replace light theme classes with dark theme equivalents. The pattern:
- `bg-white` → `bg-surface`
- `border-gray-100` → `border-border`
- `text-gray-900` / `text-slate-900` → `text-text-primary`
- `text-gray-500` / `text-gray-600` → `text-text-secondary`
- `text-gray-400` → `text-text-tertiary`
- `bg-gray-50` / `bg-blue-50` / etc → `bg-surface`
- `shadow-sm` → remove (use border instead)

Read each file, then apply the class replacements. The card wrapper pattern should be:
```
bg-surface border border-border rounded-2xl p-5
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 3: Commit**

```bash
git add components/widgets/NarrativeCard.tsx components/widgets/MarketPulse.tsx components/widgets/InsightCard.tsx components/widgets/HistoricalContext.tsx
git commit -m "feat: restyle feed widgets for dark theme"
```

---

## Task 8: Rewrite AppShell — Two-Phase Loading

**Files:**
- Modify: `components/AppShell.tsx`
- Modify: `components/ScenarioSelector.tsx`

**Step 1: Rewrite AppShell.tsx**

`components/AppShell.tsx`:
```tsx
"use client";

import { useState } from "react";
import { UILayout, HeroData, BubbleDetail } from "@/lib/types";
import { scenarios } from "@/lib/data/context-signals";
import ScenarioSelector from "./ScenarioSelector";
import HeroZone from "./HeroZone";
import FeedZone from "./FeedZone";
import BubbleDetailOverlay from "./BubbleDetailOverlay";

export default function AppShell() {
  const [selectedScenario, setSelectedScenario] = useState<string | null>(null);
  const [hero, setHero] = useState<HeroData | null>(null);
  const [layout, setLayout] = useState<UILayout | null>(null);
  const [heroLoading, setHeroLoading] = useState(false);
  const [aiLoading, setAiLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Bubble detail overlay state
  const [overlayOpen, setOverlayOpen] = useState(false);
  const [selectedBubble, setSelectedBubble] = useState<string | null>(null);

  const apiUrl = process.env.NEXT_PUBLIC_API_URL || "";

  const handleSelectScenario = async (scenarioId: string) => {
    setSelectedScenario(scenarioId);
    setHero(null);
    setLayout(null);
    setError(null);
    setHeroLoading(true);
    setAiLoading(true);

    // Phase 1: Instant hero data
    try {
      const heroRes = await fetch(`${apiUrl}/api/hero/${scenarioId}`);
      if (!heroRes.ok) throw new Error("Failed to load hero data");
      const heroData: HeroData = await heroRes.json();
      setHero(heroData);
    } catch (e) {
      const msg = e instanceof Error ? e.message : "Failed to load";
      if (msg.includes("fetch") || msg.includes("ECONNREFUSED") || msg.includes("Failed to fetch")) {
        setError(`Cannot reach backend. Is the .NET backend running? (${apiUrl || "no URL set"})`);
      } else {
        setError(msg);
      }
    } finally {
      setHeroLoading(false);
    }

    // Phase 2: AI-generated content (fires in parallel, resolves later)
    try {
      const aiRes = await fetch(`${apiUrl}/api/generate-ui`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ scenarioId }),
      });
      if (!aiRes.ok) {
        const errText = await aiRes.text();
        let errMsg = "Failed to generate UI";
        try {
          const err = JSON.parse(errText);
          errMsg = err.error || err.detail || err.title || errMsg;
        } catch { errMsg = errText || errMsg; }
        throw new Error(errMsg);
      }
      const data: UILayout = await aiRes.json();
      setLayout(data);
    } catch (e) {
      console.error("[InvestSuite] AI error:", e);
      // Don't overwrite hero error — AI failure is non-fatal, hero still works
      if (!error) {
        setError(e instanceof Error ? e.message : "AI generation failed");
      }
    } finally {
      setAiLoading(false);
    }
  };

  const handleBubbleTap = (name: string) => {
    setSelectedBubble(name);
    setOverlayOpen(true);
  };

  const currentBubbleDetail: BubbleDetail | null =
    selectedBubble && layout?.bubbleDetails?.[selectedBubble]
      ? layout.bubbleDetails[selectedBubble]
      : null;

  return (
    <div className="min-h-screen bg-background">
      <ScenarioSelector
        scenarios={scenarios}
        selected={selectedScenario}
        onSelect={handleSelectScenario}
        loading={heroLoading}
      />

      {/* Empty state */}
      {!selectedScenario && !heroLoading && (
        <div className="min-h-screen flex flex-col items-center justify-center px-4">
          <div className="w-16 h-16 bg-surface border border-border rounded-full flex items-center justify-center mx-auto mb-4">
            <svg className="w-8 h-8 text-text-tertiary" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M15.59 14.37a6 6 0 01-5.84 7.38v-4.8m5.84-2.58a14.98 14.98 0 006.16-12.12A14.98 14.98 0 009.631 8.41m5.96 5.96a14.926 14.926 0 01-5.841 2.58m-.119-8.54a6 6 0 00-7.381 5.84h4.8m2.581-5.84a14.927 14.927 0 00-2.58 5.841m2.699 2.7c-.103.021-.207.041-.311.06a15.09 15.09 0 01-2.448-2.448 14.9 14.9 0 01.06-.312m-2.24 2.39a4.493 4.493 0 00-1.757 4.306 4.493 4.493 0 004.306-1.758M16.5 9a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z" />
            </svg>
          </div>
          <h2 className="text-lg font-semibold text-text-primary mb-1">Choose a scenario</h2>
          <p className="text-sm text-text-secondary max-w-xs mx-auto text-center">
            Select an investor scenario above to see how the interface adapts to their moment.
          </p>
        </div>
      )}

      {/* Error state */}
      {error && !hero && (
        <div className="min-h-screen flex items-center justify-center px-4">
          <div className="bg-loss-light border border-loss/20 rounded-2xl p-6 text-center max-w-sm">
            <p className="text-sm text-loss font-medium mb-2">Something went wrong</p>
            <p className="text-xs text-text-secondary">{error}</p>
            <button
              onClick={() => selectedScenario && handleSelectScenario(selectedScenario)}
              className="mt-4 px-4 py-2 bg-text-primary text-background text-sm rounded-full"
            >
              Try again
            </button>
          </div>
        </div>
      )}

      {/* Hero zone */}
      {hero && (
        <>
          <HeroZone
            hero={hero}
            narrative={layout?.narrative ?? null}
            suggestionPills={layout?.suggestionPills ?? null}
            aiLoading={aiLoading}
            onBubbleTap={handleBubbleTap}
          />

          {/* Feed zone */}
          <FeedZone
            widgets={layout?.widgets ?? []}
            experience={layout?.experience ?? "beginner"}
            mood={layout?.mood ?? "calm"}
            loading={aiLoading}
          />

          {/* Footer */}
          <footer className="max-w-lg mx-auto px-4 py-8 text-center">
            <p className="text-xs text-text-tertiary">
              Powered by Claude AI — Every view is uniquely generated
            </p>
          </footer>

          {/* Bubble detail overlay */}
          <BubbleDetailOverlay
            detail={currentBubbleDetail}
            portfolioTotal={hero.portfolioTotal}
            portfolioChangePct={hero.portfolioChange7dPct}
            experience={layout?.experience ?? "beginner"}
            open={overlayOpen}
            onClose={() => setOverlayOpen(false)}
          />
        </>
      )}
    </div>
  );
}
```

**Step 2: Restyle ScenarioSelector for dark theme**

`components/ScenarioSelector.tsx`:
```tsx
"use client";

import { ScenarioOption } from "@/lib/types";

interface Props {
  scenarios: ScenarioOption[];
  selected: string | null;
  onSelect: (id: string) => void;
  loading: boolean;
}

export default function ScenarioSelector({ scenarios, selected, onSelect, loading }: Props) {
  return (
    <div className="bg-background/80 backdrop-blur-md border-b border-border sticky top-0 z-30">
      <div className="max-w-lg mx-auto px-4 py-3">
        <p className="text-xs text-text-tertiary uppercase tracking-wider font-medium mb-2">Scenario</p>
        <div className="flex gap-2 overflow-x-auto pb-1 -mx-1 px-1 scrollbar-hide">
          {scenarios.map((s) => (
            <button
              key={s.id}
              onClick={() => onSelect(s.id)}
              disabled={loading}
              className={`flex-shrink-0 px-4 py-2 rounded-full text-sm font-medium transition-all duration-200 ${
                selected === s.id
                  ? "bg-text-primary text-background"
                  : "bg-surface text-text-secondary hover:text-text-primary border border-border"
              } ${loading ? "opacity-50 cursor-not-allowed" : "cursor-pointer"}`}
            >
              {s.label}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
```

**Step 3: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 4: Commit**

```bash
git add components/AppShell.tsx components/ScenarioSelector.tsx
git commit -m "feat: rewrite AppShell with two-phase loading and bubble home"
```

---

## Task 9: Build + Smoke Test

**Step 1: Build everything**

```bash
cd C:/Users/thoma/Documents/team-25/backend && dotnet build && dotnet test
cd C:/Users/thoma/Documents/team-25 && bun run build
```

**Step 2: Run and test**

```bash
# Terminal 1: start .NET backend
cd C:/Users/thoma/Documents/team-25/backend && dotnet run --project src/InvestSuite.Api

# Terminal 2: start frontend
cd C:/Users/thoma/Documents/team-25 && bun dev
```

Open http://localhost:3000. Select a scenario. Verify:
- Hero zone appears instantly with portfolio value and floating bubbles
- Narrative and suggestion pills fade in when AI responds
- Tapping a bubble opens the detail overlay (shimmer if AI not ready, content if ready)
- Scrolling down shows feed cards in dark theme
- Suggestion pills open the correct bubble detail

**Step 3: Commit**

```bash
git add -A
git commit -m "feat: complete bubble home redesign"
```

---

## Dependency Graph

```
Task 1 (types + dark theme) ──┬──► Task 2 (hero endpoint)
                               ├──► Task 3 (prompt updates)
                               ├──► Task 4 (hero + bubble components)
                               ├──► Task 5 (bubble detail overlay)
                               ├──► Task 6 (feed zone)
                               └──► Task 7 (restyle widgets)
                                         │
Task 8 (AppShell rewrite) ◄──────────────┘ depends on 2,4,5,6
Task 9 (build + test) ◄──── depends on all
```

Tasks 2, 3, 4, 5, 6, 7 can run in parallel after Task 1.
Task 8 depends on Tasks 2, 4, 5, 6.

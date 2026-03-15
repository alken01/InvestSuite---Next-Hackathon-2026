# Adaptive Investing UI — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Build a Next.js mobile-first investing app where Claude dynamically composes the UI from a widget library based on user context signals — every visit is unique.

**Architecture:** Next.js App Router with a POST /api/generate-ui endpoint. The endpoint gathers user profile, portfolio, market data, and context signals, sends them to Claude (via API or CLI), and receives a JSON layout spec. The frontend renders the layout as a stream of adaptive widgets. Two AI provider modes: `cli` (local dev, no API key) and `api` (production).

**Tech Stack:** Next.js 14, Tailwind CSS, bun, TypeScript, Framer Motion, Claude API (claude-sonnet-4-6)

---

## Task 1: Project Scaffold

**Files:**
- Create: `package.json` (via create-next-app)
- Create: `.env.local`
- Modify: `app/layout.tsx`
- Modify: `app/globals.css`

**Step 1: Create Next.js project**

```bash
cd C:/Users/thoma/Documents/team-25
bunx create-next-app@latest . --typescript --tailwind --eslint --app --src=false --import-alias="@/*" --use-bun
```

Note: If prompted about overwriting, say yes. The `.` creates in current directory.

**Step 2: Install additional dependencies**

```bash
bun add framer-motion @anthropic-ai/sdk
```

**Step 3: Create .env.local**

```env
# AI Provider: "cli" for local dev (uses Claude Code), "api" for production
AI_PROVIDER=cli

# Only needed when AI_PROVIDER=api
ANTHROPIC_API_KEY=
```

**Step 4: Update app/globals.css**

Replace contents with:

```css
@import "tailwindcss";

@theme {
  --font-sans: "Inter", "SF Pro Display", -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;

  --color-gain: #059669;
  --color-gain-light: #d1fae5;
  --color-loss: #e11d48;
  --color-loss-light: #ffe4e6;
  --color-surface: #ffffff;
  --color-background: #f8fafc;
  --color-text-primary: #0f172a;
  --color-text-secondary: #64748b;
  --color-text-tertiary: #94a3b8;
  --color-border: #e2e8f0;
  --color-accent: #3b82f6;
  --color-accent-light: #dbeafe;
}

html {
  font-family: var(--font-sans);
  background-color: var(--color-background);
  color: var(--color-text-primary);
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

/* Mood-adaptive CSS custom properties — set by JS on the container */
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
```

**Step 5: Update app/layout.tsx**

```tsx
import type { Metadata, Viewport } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "InvestSuite — Your Portfolio, Your Moment",
  description: "An investing experience that adapts to you",
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  maximumScale: 1,
  themeColor: "#f8fafc",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <head>
        <link
          href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap"
          rel="stylesheet"
        />
      </head>
      <body className="bg-background min-h-screen">{children}</body>
    </html>
  );
}
```

**Step 6: Commit**

```bash
git add -A
git commit -m "feat: scaffold Next.js project with Tailwind, Framer Motion, and Claude SDK"
```

---

## Task 2: Data Layer + Type Definitions

**Files:**
- Create: `lib/types.ts`
- Create: `lib/data/investors.ts`
- Create: `lib/data/portfolios.ts`
- Create: `lib/data/context-signals.ts`
- Create: `lib/data/market-data.ts`

**Step 1: Create type definitions**

Create `lib/types.ts`:

```typescript
// --- Domain Types ---

export interface Investor {
  id: string;
  name: string;
  age: number;
  experienceMonths: number;
  portfolioTotal: number;
  checkFrequency: string;
  personality: string;
  experienceLevel: ExperienceLevel;
}

export interface Holding {
  name: string;
  description: string;
  type: "Stock" | "ETF" | "Cash";
  value: number;
  returnPct: number | null;
}

export interface StockDetail {
  name: string;
  price: number;
  changeTodayPct: number;
  description: string;
  marketCapB: number;
  peRatio: number;
  sectorAvgPe: number;
  dividendYieldPct: number;
  analystBuy: number;
  analystHold: number;
  analystSell: number;
  analystConsensus: string;
  analystTargetEur: number;
  scoreGrowth: number;
  scoreHealth: number;
  scoreStability: number;
}

export interface ContextSignals {
  investor: string;
  scenario: string;
  sessionCount30d: number;
  daysSinceLastSession: number;
  portfolioChange7d: number | null;
  portfolioChangeSinceLastVisit: number | null;
  dividendsReceivedSinceLastVisit: number | null;
  pendingActions: string | null;
  marketVolatility: string | null;
  timeOfDay: string | null;
  lastAction: string | null;
  searchQuery: string | null;
  emotionalStateEstimate: string;
}

// --- AI / UI Types ---

export type ExperienceLevel = "beginner" | "expert";
export type MoodState = "calm" | "anxious" | "focused" | "detached";

export type WidgetType =
  | "portfolio_summary"
  | "stock_card"
  | "market_pulse"
  | "narrative_card"
  | "historical_context"
  | "insight_card"
  | "welcome_back_card";

export interface PortfolioSummaryProps {
  holdings: Holding[];
  totalValue: number;
  totalChangePct: number | null;
  changeSinceLastVisit: number | null;
}

export interface StockCardProps {
  name: string;
  description: string;
  type: "Stock" | "ETF";
  value: number;
  returnPct: number;
  price?: number;
  changeTodayPct?: number;
  peRatio?: number;
  sectorAvgPe?: number;
  dividendYieldPct?: number;
  analystConsensus?: string;
  analystTarget?: number;
  scores?: { growth: number; health: number; stability: number };
}

export interface MarketPulseProps {
  volatility: "Low" | "Medium" | "High";
  summary: string;
  sectorHighlights?: { sector: string; changePct: number }[];
}

export interface NarrativeCardProps {
  title: string;
  message: string;
}

export interface HistoricalContextProps {
  event: string;
  description: string;
  recoveryMonths?: number;
  previousOccurrences?: { date: string; dropPct: number; recovery: string }[];
}

export interface InsightCardProps {
  title: string;
  content: string;
  category: "education" | "action" | "tip";
}

export interface WelcomeBackCardProps {
  daysSinceLastVisit: number;
  portfolioChangePct: number;
  dividendsReceived: number | null;
  pendingActions: string | null;
  summary: string;
}

export type WidgetProps =
  | { type: "portfolio_summary"; props: PortfolioSummaryProps }
  | { type: "stock_card"; props: StockCardProps }
  | { type: "market_pulse"; props: MarketPulseProps }
  | { type: "narrative_card"; props: NarrativeCardProps }
  | { type: "historical_context"; props: HistoricalContextProps }
  | { type: "insight_card"; props: InsightCardProps }
  | { type: "welcome_back_card"; props: WelcomeBackCardProps };

export interface UILayout {
  narrative: string;
  widgets: WidgetProps[];
  mood: MoodState;
  experience: ExperienceLevel;
}

export interface ScenarioOption {
  id: string;
  investor: string;
  label: string;
  description: string;
}
```

**Step 2: Create investor data**

Create `lib/data/investors.ts`:

```typescript
import { Investor } from "@/lib/types";

export const investors: Record<string, Investor> = {
  sophie: {
    id: "sophie",
    name: "Sophie",
    age: 28,
    experienceMonths: 8,
    portfolioTotal: 4830,
    checkFrequency: "Several times a day",
    personality: "Anxious beginner — panics when she sees red",
    experienceLevel: "beginner",
  },
  marc: {
    id: "marc",
    name: "Marc",
    age: 52,
    experienceMonths: 144,
    portfolioTotal: 87420,
    checkFrequency: "Once a month",
    personality: "Hands-off veteran — ignores short-term noise",
    experienceLevel: "expert",
  },
};
```

**Step 3: Create portfolio data**

Create `lib/data/portfolios.ts`:

```typescript
import { Holding } from "@/lib/types";

export const portfolios: Record<string, Holding[]> = {
  sophie: [
    { name: "ASML", description: "Chip machine monopoly", type: "Stock", value: 1490, returnPct: 9.6 },
    { name: "World ETF", description: "~1,500 global companies", type: "ETF", value: 1292, returnPct: 4.4 },
    { name: "AB InBev", description: "World's biggest brewer (Jupiler, Stella)", type: "Stock", value: 438, returnPct: -5.8 },
    { name: "Euro Stoxx 50 ETF", description: "50 biggest European companies", type: "ETF", value: 664, returnPct: 6.3 },
    { name: "KBC Group", description: "Belgian bank", type: "Stock", value: 359, returnPct: 5.0 },
    { name: "Cash", description: "", type: "Cash", value: 587, returnPct: null },
  ],
  marc: [
    { name: "S&P 500 ETF", description: "500 biggest US companies", type: "ETF", value: 20826, returnPct: 21.8 },
    { name: "ASML", description: "Same company, bought years ago", type: "Stock", value: 11178, returnPct: 77.4 },
    { name: "Bond ETF", description: "Loans to companies. Boring but stable.", type: "ETF", value: 10280, returnPct: -2.3 },
    { name: "All-World ETF", description: "~3,500 companies globally", type: "ETF", value: 8672, returnPct: 10.6 },
    { name: "SAP", description: "German business software", type: "Stock", value: 5958, returnPct: 58.9 },
    { name: "LVMH", description: "French luxury (Louis Vuitton, Dior)", type: "Stock", value: 4612, returnPct: 39.2 },
    { name: "TotalEnergies", description: "French oil & gas major", type: "Stock", value: 3290, returnPct: 14.1 },
    { name: "Novo Nordisk", description: "Danish pharma, weight-loss drugs", type: "Stock", value: 3874, returnPct: 31.5 },
    { name: "Unilever", description: "Consumer goods (Dove, Hellmann's)", type: "Stock", value: 2532, returnPct: 18.7 },
    { name: "Cash EUR + USD", description: "", type: "Cash", value: 16198, returnPct: null },
  ],
};
```

**Step 4: Create context signals data**

Create `lib/data/context-signals.ts`:

```typescript
import { ContextSignals, ScenarioOption } from "@/lib/types";

export const contextSignals: Record<string, ContextSignals> = {
  "sophie-calm": {
    investor: "sophie",
    scenario: "Calm Tuesday Evening",
    sessionCount30d: 12,
    daysSinceLastSession: 2,
    portfolioChange7d: 0.8,
    portfolioChangeSinceLastVisit: null,
    dividendsReceivedSinceLastVisit: null,
    pendingActions: null,
    marketVolatility: "Low",
    timeOfDay: "20:15",
    lastAction: null,
    searchQuery: null,
    emotionalStateEstimate: "Calm / curious",
  },
  "sophie-crash": {
    investor: "sophie",
    scenario: "Morning After a Crash",
    sessionCount30d: 12,
    daysSinceLastSession: 0,
    portfolioChange7d: -6.2,
    portfolioChangeSinceLastVisit: null,
    dividendsReceivedSinceLastVisit: null,
    pendingActions: null,
    marketVolatility: "High",
    timeOfDay: "07:42",
    lastAction: null,
    searchQuery: null,
    emotionalStateEstimate: "Anxious / checking compulsively",
  },
  "marc-returning": {
    investor: "marc",
    scenario: "Returning After 3 Months",
    sessionCount30d: 0,
    daysSinceLastSession: 94,
    portfolioChange7d: null,
    portfolioChangeSinceLastVisit: 4.2,
    dividendsReceivedSinceLastVisit: 525,
    pendingActions: "2 expired orders",
    marketVolatility: null,
    timeOfDay: null,
    lastAction: null,
    searchQuery: null,
    emotionalStateEstimate: "Detached / out of the loop",
  },
  "marc-research": {
    investor: "marc",
    scenario: "Research Mode",
    sessionCount30d: 22,
    daysSinceLastSession: 0,
    portfolioChange7d: 1.9,
    portfolioChangeSinceLastVisit: null,
    dividendsReceivedSinceLastVisit: null,
    pendingActions: null,
    marketVolatility: null,
    timeOfDay: null,
    lastAction: "Viewed 4 stocks in 8 minutes",
    searchQuery: "european semiconductor",
    emotionalStateEstimate: "Focused / researching with intent",
  },
};

export const scenarios: ScenarioOption[] = [
  {
    id: "sophie-calm",
    investor: "sophie",
    label: "Sophie — Calm Evening",
    description: "Calm Tuesday, portfolio up slightly, browsing casually",
  },
  {
    id: "sophie-crash",
    investor: "sophie",
    label: "Sophie — Crash Morning",
    description: "Markets crashed overnight, checking compulsively at 7am",
  },
  {
    id: "marc-returning",
    investor: "marc",
    label: "Marc — Back After 3 Months",
    description: "First login in 94 days, portfolio grew, has pending actions",
  },
  {
    id: "marc-research",
    investor: "marc",
    label: "Marc — Research Mode",
    description: "3rd session today, actively researching European semiconductors",
  },
];
```

**Step 5: Create market data**

Create `lib/data/market-data.ts`:

```typescript
import { StockDetail } from "@/lib/types";

export const stockDetails: Record<string, StockDetail> = {
  ASML: {
    name: "ASML",
    price: 745.2,
    changeTodayPct: 1.2,
    description: "Makes the machines that make computer chips. Near-monopoly. Every major chipmaker depends on them.",
    marketCapB: 301,
    peRatio: 35.2,
    sectorAvgPe: 28,
    dividendYieldPct: 0.7,
    analystBuy: 18,
    analystHold: 4,
    analystSell: 1,
    analystConsensus: "Strong Buy",
    analystTargetEur: 820,
    scoreGrowth: 5,
    scoreHealth: 4,
    scoreStability: 3,
  },
};

export const marketOverview = {
  indices: [
    { name: "Euro Stoxx 50", changePct: -0.3 },
    { name: "S&P 500", changePct: 0.7 },
    { name: "AEX", changePct: 0.4 },
  ],
  sectors: [
    { sector: "Technology", changePct: 1.8 },
    { sector: "Healthcare", changePct: -0.5 },
    { sector: "Financials", changePct: 0.3 },
    { sector: "Energy", changePct: -1.2 },
    { sector: "Consumer", changePct: 0.1 },
  ],
};
```

**Step 6: Commit**

```bash
git add lib/
git commit -m "feat: add type definitions and mock data layer"
```

---

## Task 3: AI Provider Abstraction

**Files:**
- Create: `lib/ai/prompt.ts`
- Create: `lib/ai/provider.ts`
- Create: `lib/ai/cli-provider.ts`
- Create: `lib/ai/api-provider.ts`

**Step 1: Create the system prompt**

Create `lib/ai/prompt.ts`:

```typescript
import { Holding, Investor, ContextSignals, StockDetail } from "@/lib/types";

export function buildSystemPrompt(): string {
  return `You are the "brain" of an adaptive investing interface. Your job is to compose a unique, personalized UI layout for an investor based on their profile, portfolio, context signals, and current market conditions.

RULES:
- NO investment advice. Never recommend buying or selling. The investor always decides.
- NO gamification. No points, badges, streaks.
- The investor stays in control. Never manipulate or create false urgency.
- Generate a DIFFERENT layout composition every time. Vary which widgets you include, their order, and the narrative content.
- Match your tone to the investor's emotional state and experience level.

AVAILABLE WIDGETS (pick 3-6 per layout, never use all 7):

1. "portfolio_summary" — Overview of the investor's portfolio
   Props: { holdings: Holding[], totalValue: number, totalChangePct: number | null, changeSinceLastVisit: number | null }

2. "stock_card" — Detail card for a specific holding
   Props: { name: string, description: string, type: "Stock"|"ETF", value: number, returnPct: number, price?: number, changeTodayPct?: number, peRatio?: number, sectorAvgPe?: number, dividendYieldPct?: number, analystConsensus?: string, analystTarget?: number, scores?: { growth: number, health: number, stability: number } }

3. "market_pulse" — Current market conditions overview
   Props: { volatility: "Low"|"Medium"|"High", summary: string, sectorHighlights?: { sector: string, changePct: number }[] }

4. "narrative_card" — Personalized message to the investor
   Props: { title: string, message: string }

5. "historical_context" — "This happened before" perspective card
   Props: { event: string, description: string, recoveryMonths?: number, previousOccurrences?: { date: string, dropPct: number, recovery: string }[] }

6. "insight_card" — Education or actionable information
   Props: { title: string, content: string, category: "education"|"action"|"tip" }

7. "welcome_back_card" — Catch-up card for returning users
   Props: { daysSinceLastVisit: number, portfolioChangePct: number, dividendsReceived: number | null, pendingActions: string | null, summary: string }

EXPERIENCE LEVELS:
- "beginner": Simple language, fewer metrics, explanatory tone, line charts
- "expert": Technical metrics (P/E, analyst consensus), candlestick style, concise

MOOD STATES (affects density and tone):
- "calm": Light, exploratory, educational content welcome
- "anxious": Fewer widgets, more whitespace, calming reassurance, perspective
- "focused": Dense data, multiple detail cards, minimal narrative
- "detached": Storytelling flow, catch-up sequence, welcome back

RESPONSE FORMAT — Return ONLY valid JSON, no markdown:
{
  "narrative": "A one-sentence greeting or contextual headline",
  "mood": "calm" | "anxious" | "focused" | "detached",
  "experience": "beginner" | "expert",
  "widgets": [
    { "type": "<widget_type>", "props": { ... } }
  ]
}

Be creative with narratives. Be specific to the moment. Make every response unique.`;
}

export function buildUserPrompt(
  investor: Investor,
  holdings: Holding[],
  signals: ContextSignals,
  stockDetails: Record<string, { price: number; changeTodayPct: number; peRatio: number; sectorAvgPe: number; dividendYieldPct: number; analystConsensus: string; analystTargetEur: number; scoreGrowth: number; scoreHealth: number; scoreStability: number }>,
): string {
  return `Generate an adaptive UI layout for this investor right now.

INVESTOR PROFILE:
- Name: ${investor.name}
- Age: ${investor.age}
- Experience: ${investor.experienceMonths} months of investing
- Portfolio total: €${investor.portfolioTotal.toLocaleString()}
- Check frequency: ${investor.checkFrequency}
- Personality: ${investor.personality}
- Experience level: ${investor.experienceLevel}

PORTFOLIO HOLDINGS:
${JSON.stringify(holdings, null, 2)}

CONTEXT SIGNALS (this session):
- Scenario: ${signals.scenario}
- Sessions in last 30 days: ${signals.sessionCount30d}
- Days since last session: ${signals.daysSinceLastSession}
- Portfolio change (7 days): ${signals.portfolioChange7d !== null ? signals.portfolioChange7d + "%" : "N/A"}
- Portfolio change since last visit: ${signals.portfolioChangeSinceLastVisit !== null ? signals.portfolioChangeSinceLastVisit + "%" : "N/A"}
- Dividends received since last visit: ${signals.dividendsReceivedSinceLastVisit !== null ? "€" + signals.dividendsReceivedSinceLastVisit : "N/A"}
- Pending actions: ${signals.pendingActions || "None"}
- Market volatility: ${signals.marketVolatility || "Normal"}
- Time of day: ${signals.timeOfDay || "Unknown"}
- Last action: ${signals.lastAction || "None"}
- Search query: ${signals.searchQuery || "None"}
- Emotional state: ${signals.emotionalStateEstimate}

AVAILABLE STOCK DETAILS (use for stock_card widgets if relevant):
${JSON.stringify(stockDetails, null, 2)}

Remember: pick 3-6 widgets, vary the composition, match tone to emotional state. Return ONLY valid JSON.`;
}
```

**Step 2: Create provider interface and factory**

Create `lib/ai/provider.ts`:

```typescript
import { UILayout, Investor, Holding, ContextSignals } from "@/lib/types";

export interface AIProvider {
  generateLayout(
    investor: Investor,
    holdings: Holding[],
    signals: ContextSignals,
    stockDetails: Record<string, any>,
  ): Promise<UILayout>;
}

export async function getAIProvider(): Promise<AIProvider> {
  const mode = process.env.AI_PROVIDER || "cli";

  if (mode === "api") {
    const { APIProvider } = await import("./api-provider");
    return new APIProvider();
  } else {
    const { CLIProvider } = await import("./cli-provider");
    return new CLIProvider();
  }
}
```

**Step 3: Create CLI provider**

Create `lib/ai/cli-provider.ts`:

```typescript
import { spawn } from "child_process";
import { AIProvider } from "./provider";
import { UILayout, Investor, Holding, ContextSignals } from "@/lib/types";
import { buildSystemPrompt, buildUserPrompt } from "./prompt";

export class CLIProvider implements AIProvider {
  async generateLayout(
    investor: Investor,
    holdings: Holding[],
    signals: ContextSignals,
    stockDetails: Record<string, any>,
  ): Promise<UILayout> {
    const systemPrompt = buildSystemPrompt();
    const userPrompt = buildUserPrompt(investor, holdings, signals, stockDetails);

    const fullPrompt = `${systemPrompt}\n\n---\n\n${userPrompt}`;

    return new Promise((resolve, reject) => {
      const proc = spawn("claude", ["-p", "--output-format", "json"], {
        shell: true,
        env: { ...process.env },
      });

      let stdout = "";
      let stderr = "";

      proc.stdout.on("data", (data: Buffer) => {
        stdout += data.toString();
      });

      proc.stderr.on("data", (data: Buffer) => {
        stderr += data.toString();
      });

      proc.on("close", (code: number | null) => {
        if (code !== 0) {
          reject(new Error(`Claude CLI exited with code ${code}: ${stderr}`));
          return;
        }

        try {
          // Claude CLI with --output-format json wraps the response
          // The actual text content is in the result field
          const cliOutput = JSON.parse(stdout);
          const textContent = cliOutput.result || stdout;

          // Extract JSON from the text (Claude might wrap it in markdown)
          const jsonMatch = typeof textContent === "string"
            ? textContent.match(/\{[\s\S]*\}/)
            : null;

          if (jsonMatch) {
            const layout = JSON.parse(jsonMatch[0]) as UILayout;
            resolve(layout);
          } else if (typeof textContent === "object") {
            resolve(textContent as UILayout);
          } else {
            reject(new Error("Could not parse UI layout from Claude CLI output"));
          }
        } catch (e) {
          // Try to extract JSON directly from stdout
          try {
            const jsonMatch = stdout.match(/\{[\s\S]*\}/);
            if (jsonMatch) {
              const layout = JSON.parse(jsonMatch[0]) as UILayout;
              resolve(layout);
              return;
            }
          } catch {
            // fall through
          }
          reject(new Error(`Failed to parse Claude CLI output: ${e}`));
        }
      });

      proc.stdin.write(fullPrompt);
      proc.stdin.end();
    });
  }
}
```

**Step 4: Create API provider**

Create `lib/ai/api-provider.ts`:

```typescript
import Anthropic from "@anthropic-ai/sdk";
import { AIProvider } from "./provider";
import { UILayout, Investor, Holding, ContextSignals } from "@/lib/types";
import { buildSystemPrompt, buildUserPrompt } from "./prompt";

export class APIProvider implements AIProvider {
  private client: Anthropic;

  constructor() {
    this.client = new Anthropic({
      apiKey: process.env.ANTHROPIC_API_KEY,
    });
  }

  async generateLayout(
    investor: Investor,
    holdings: Holding[],
    signals: ContextSignals,
    stockDetails: Record<string, any>,
  ): Promise<UILayout> {
    const response = await this.client.messages.create({
      model: "claude-sonnet-4-6",
      max_tokens: 4096,
      system: buildSystemPrompt(),
      messages: [
        {
          role: "user",
          content: buildUserPrompt(investor, holdings, signals, stockDetails),
        },
      ],
    });

    const textBlock = response.content.find((block) => block.type === "text");
    if (!textBlock || textBlock.type !== "text") {
      throw new Error("No text response from Claude API");
    }

    const jsonMatch = textBlock.text.match(/\{[\s\S]*\}/);
    if (!jsonMatch) {
      throw new Error("Could not extract JSON from Claude API response");
    }

    return JSON.parse(jsonMatch[0]) as UILayout;
  }
}
```

**Step 5: Commit**

```bash
git add lib/ai/
git commit -m "feat: add AI provider abstraction with CLI and API modes"
```

---

## Task 4: API Route

**Files:**
- Create: `app/api/generate-ui/route.ts`

**Step 1: Create the API route**

Create `app/api/generate-ui/route.ts`:

```typescript
import { NextRequest, NextResponse } from "next/server";
import { getAIProvider } from "@/lib/ai/provider";
import { investors } from "@/lib/data/investors";
import { portfolios } from "@/lib/data/portfolios";
import { contextSignals } from "@/lib/data/context-signals";
import { stockDetails } from "@/lib/data/market-data";

export async function POST(request: NextRequest) {
  try {
    const { scenarioId } = await request.json();

    if (!scenarioId || !contextSignals[scenarioId]) {
      return NextResponse.json(
        { error: "Invalid scenario ID" },
        { status: 400 },
      );
    }

    const signals = contextSignals[scenarioId];
    const investorId = signals.investor;
    const investor = investors[investorId];
    const holdings = portfolios[investorId];

    if (!investor || !holdings) {
      return NextResponse.json(
        { error: "Investor data not found" },
        { status: 404 },
      );
    }

    const provider = await getAIProvider();
    const layout = await provider.generateLayout(
      investor,
      holdings,
      signals,
      stockDetails,
    );

    return NextResponse.json(layout);
  } catch (error) {
    console.error("Error generating UI:", error);
    return NextResponse.json(
      { error: "Failed to generate UI layout", details: String(error) },
      { status: 500 },
    );
  }
}
```

**Step 2: Commit**

```bash
git add app/api/
git commit -m "feat: add POST /api/generate-ui endpoint"
```

---

## Task 5: Widget Library — Part 1 (Portfolio Summary, Stock Card, Narrative Card)

**Files:**
- Create: `components/widgets/PortfolioSummary.tsx`
- Create: `components/widgets/StockCard.tsx`
- Create: `components/widgets/NarrativeCard.tsx`

**Step 1: Create PortfolioSummary widget**

Create `components/widgets/PortfolioSummary.tsx`:

```tsx
"use client";

import { PortfolioSummaryProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends PortfolioSummaryProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency: "EUR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

function ChangeIndicator({ value, label }: { value: number | null; label: string }) {
  if (value === null) return null;
  const isPositive = value >= 0;
  return (
    <div className="flex items-center justify-between">
      <span className="text-sm text-text-secondary">{label}</span>
      <span className={`text-sm font-semibold ${isPositive ? "text-gain" : "text-loss"}`}>
        {isPositive ? "+" : ""}{value.toFixed(1)}%
      </span>
    </div>
  );
}

export default function PortfolioSummary({ holdings, totalValue, totalChangePct, changeSinceLastVisit, experience, mood }: Props) {
  const nonCashHoldings = holdings.filter((h) => h.type !== "Cash");
  const cashHolding = holdings.find((h) => h.type === "Cash");

  const totalReturn = totalChangePct ?? (
    nonCashHoldings.length > 0
      ? nonCashHoldings.reduce((sum, h) => sum + (h.returnPct ?? 0) * h.value, 0) /
        nonCashHoldings.reduce((sum, h) => sum + h.value, 0)
      : 0
  );

  const isPositive = totalReturn >= 0;

  return (
    <div className="bg-surface rounded-2xl shadow-sm p-6">
      {/* Total Value */}
      <div className="mb-6">
        <p className="text-sm text-text-secondary mb-1">Your Portfolio</p>
        <p className="text-4xl font-bold tracking-tight">{formatCurrency(totalValue)}</p>
        <div className={`inline-flex items-center mt-2 px-3 py-1 rounded-full text-sm font-medium ${
          isPositive ? "bg-gain-light text-gain" : "bg-loss-light text-loss"
        }`}>
          {isPositive ? "+" : ""}{totalReturn.toFixed(1)}%
          {mood === "anxious" && isPositive && (
            <span className="ml-1 text-text-secondary">— still growing</span>
          )}
        </div>
      </div>

      {/* Change since last visit */}
      <ChangeIndicator value={changeSinceLastVisit} label="Since last visit" />
      <ChangeIndicator value={totalChangePct} label="Last 7 days" />

      {/* Holdings breakdown */}
      {experience === "expert" ? (
        /* Expert: allocation bars */
        <div className="mt-6 space-y-3">
          <p className="text-xs text-text-tertiary uppercase tracking-wider font-medium">Allocation</p>
          {holdings.map((h) => {
            const pct = (h.value / totalValue) * 100;
            return (
              <div key={h.name} className="flex items-center gap-3">
                <div className="w-20 text-xs text-text-secondary truncate">{h.name}</div>
                <div className="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
                  <div
                    className={`h-full rounded-full ${h.type === "Cash" ? "bg-gray-300" : h.type === "ETF" ? "bg-accent" : "bg-slate-700"}`}
                    style={{ width: `${pct}%` }}
                  />
                </div>
                <div className="w-16 text-xs text-right text-text-secondary">{pct.toFixed(1)}%</div>
                {h.returnPct !== null && (
                  <div className={`w-14 text-xs text-right font-medium ${h.returnPct >= 0 ? "text-gain" : "text-loss"}`}>
                    {h.returnPct >= 0 ? "+" : ""}{h.returnPct}%
                  </div>
                )}
              </div>
            );
          })}
        </div>
      ) : (
        /* Beginner: simple list */
        <div className="mt-6 space-y-3">
          {nonCashHoldings.map((h) => (
            <div key={h.name} className="flex items-center justify-between py-2 border-b border-border last:border-0">
              <div>
                <p className="text-sm font-medium">{h.name}</p>
                <p className="text-xs text-text-tertiary">{h.description}</p>
              </div>
              <div className="text-right">
                <p className="text-sm font-medium">{formatCurrency(h.value)}</p>
                {h.returnPct !== null && (
                  <p className={`text-xs font-medium ${h.returnPct >= 0 ? "text-gain" : "text-loss"}`}>
                    {h.returnPct >= 0 ? "+" : ""}{h.returnPct}%
                  </p>
                )}
              </div>
            </div>
          ))}
          {cashHolding && (
            <div className="flex items-center justify-between py-2 text-text-secondary">
              <p className="text-sm">Cash available</p>
              <p className="text-sm font-medium">{formatCurrency(cashHolding.value)}</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
```

**Step 2: Create StockCard widget**

Create `components/widgets/StockCard.tsx`:

```tsx
"use client";

import { StockCardProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends StockCardProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

function ScoreDots({ score, max = 5 }: { score: number; max?: number }) {
  return (
    <div className="flex gap-1">
      {Array.from({ length: max }).map((_, i) => (
        <div
          key={i}
          className={`w-2 h-2 rounded-full ${i < score ? "bg-slate-700" : "bg-gray-200"}`}
        />
      ))}
    </div>
  );
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency: "EUR",
    minimumFractionDigits: value >= 100 ? 0 : 2,
    maximumFractionDigits: value >= 100 ? 0 : 2,
  }).format(value);
}

export default function StockCard({
  name, description, type, value, returnPct, price, changeTodayPct,
  peRatio, sectorAvgPe, dividendYieldPct, analystConsensus, analystTarget,
  scores, experience, mood,
}: Props) {
  const isPositive = returnPct >= 0;

  return (
    <div className="bg-surface rounded-2xl shadow-sm p-6">
      {/* Header */}
      <div className="flex items-start justify-between mb-4">
        <div>
          <div className="flex items-center gap-2">
            <h3 className="text-lg font-semibold">{name}</h3>
            <span className="text-xs px-2 py-0.5 rounded-full bg-gray-100 text-text-secondary">{type}</span>
          </div>
          <p className="text-sm text-text-secondary mt-0.5">{description}</p>
        </div>
        <div className="text-right">
          <p className="text-lg font-semibold">{formatCurrency(value)}</p>
          <p className={`text-sm font-medium ${isPositive ? "text-gain" : "text-loss"}`}>
            {isPositive ? "+" : ""}{returnPct}%
          </p>
        </div>
      </div>

      {/* Price line (if available) */}
      {price !== undefined && (
        <div className="flex items-center justify-between py-3 border-t border-border">
          <span className="text-sm text-text-secondary">Share price</span>
          <div className="flex items-center gap-2">
            <span className="text-sm font-medium">{formatCurrency(price)}</span>
            {changeTodayPct !== undefined && (
              <span className={`text-xs font-medium ${changeTodayPct >= 0 ? "text-gain" : "text-loss"}`}>
                {changeTodayPct >= 0 ? "+" : ""}{changeTodayPct}% today
              </span>
            )}
          </div>
        </div>
      )}

      {experience === "expert" ? (
        /* Expert: full metrics */
        <div className="space-y-0">
          {peRatio !== undefined && (
            <div className="flex items-center justify-between py-3 border-t border-border">
              <span className="text-sm text-text-secondary">P/E Ratio</span>
              <div className="flex items-center gap-2">
                <span className="text-sm font-medium">{peRatio}</span>
                {sectorAvgPe && (
                  <span className="text-xs text-text-tertiary">sector avg: {sectorAvgPe}</span>
                )}
              </div>
            </div>
          )}
          {dividendYieldPct !== undefined && (
            <div className="flex items-center justify-between py-3 border-t border-border">
              <span className="text-sm text-text-secondary">Dividend yield</span>
              <span className="text-sm font-medium">{dividendYieldPct}%</span>
            </div>
          )}
          {analystConsensus && (
            <div className="flex items-center justify-between py-3 border-t border-border">
              <span className="text-sm text-text-secondary">Analyst consensus</span>
              <div className="text-right">
                <span className="text-sm font-medium">{analystConsensus}</span>
                {analystTarget && (
                  <span className="text-xs text-text-tertiary ml-2">Target: {formatCurrency(analystTarget)}</span>
                )}
              </div>
            </div>
          )}
          {scores && (
            <div className="pt-3 border-t border-border space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-xs text-text-secondary">Growth</span>
                <ScoreDots score={scores.growth} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs text-text-secondary">Health</span>
                <ScoreDots score={scores.health} />
              </div>
              <div className="flex items-center justify-between">
                <span className="text-xs text-text-secondary">Stability</span>
                <ScoreDots score={scores.stability} />
              </div>
            </div>
          )}
        </div>
      ) : (
        /* Beginner: simplified view with context */
        <div className="mt-3">
          {mood === "anxious" && !isPositive ? (
            <p className="text-sm text-text-secondary bg-amber-50 rounded-xl p-3">
              This holding is down, but it makes up only a small part of your portfolio. Short-term dips are normal.
            </p>
          ) : mood === "anxious" && isPositive ? (
            <p className="text-sm text-text-secondary bg-gain-light rounded-xl p-3">
              This one is doing well. Your patience is paying off.
            </p>
          ) : null}
        </div>
      )}
    </div>
  );
}
```

**Step 3: Create NarrativeCard widget**

Create `components/widgets/NarrativeCard.tsx`:

```tsx
"use client";

import { NarrativeCardProps, MoodState } from "@/lib/types";

interface Props extends NarrativeCardProps {
  mood: MoodState;
}

const moodStyles: Record<MoodState, { bg: string; border: string; icon: string }> = {
  calm: { bg: "bg-blue-50", border: "border-blue-100", icon: "bg-blue-100" },
  anxious: { bg: "bg-amber-50", border: "border-amber-100", icon: "bg-amber-100" },
  focused: { bg: "bg-slate-50", border: "border-slate-100", icon: "bg-slate-100" },
  detached: { bg: "bg-violet-50", border: "border-violet-100", icon: "bg-violet-100" },
};

export default function NarrativeCard({ title, message, mood }: Props) {
  const style = moodStyles[mood];

  return (
    <div className={`${style.bg} border ${style.border} rounded-2xl p-6`}>
      <div className="flex items-start gap-4">
        <div className={`w-10 h-10 ${style.icon} rounded-full flex items-center justify-center flex-shrink-0 mt-0.5`}>
          <svg className="w-5 h-5 text-slate-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            {mood === "anxious" ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M9 12.75L11.25 15 15 9.75m-3-7.036A11.959 11.959 0 013.598 6 11.99 11.99 0 003 9.749c0 5.592 3.824 10.29 9 11.623 5.176-1.332 9-6.03 9-11.622 0-1.31-.21-2.571-.598-3.751h-.152c-3.196 0-6.1-1.248-8.25-3.285z" />
            ) : mood === "focused" ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M3.75 13.5l10.5-11.25L12 10.5h8.25L9.75 21.75 12 13.5H3.75z" />
            ) : mood === "detached" ? (
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
            ) : (
              <path strokeLinecap="round" strokeLinejoin="round" d="M2.25 18L9 11.25l4.306 4.307a11.95 11.95 0 015.814-5.519l2.74-1.22m0 0l-5.94-2.28m5.94 2.28l-2.28 5.941" />
            )}
          </svg>
        </div>
        <div>
          <h3 className="text-base font-semibold mb-1">{title}</h3>
          <p className="text-sm text-text-secondary leading-relaxed">{message}</p>
        </div>
      </div>
    </div>
  );
}
```

**Step 4: Commit**

```bash
git add components/widgets/
git commit -m "feat: add PortfolioSummary, StockCard, and NarrativeCard widgets"
```

---

## Task 6: Widget Library — Part 2 (MarketPulse, HistoricalContext, InsightCard, WelcomeBackCard)

**Files:**
- Create: `components/widgets/MarketPulse.tsx`
- Create: `components/widgets/HistoricalContext.tsx`
- Create: `components/widgets/InsightCard.tsx`
- Create: `components/widgets/WelcomeBackCard.tsx`

**Step 1: Create MarketPulse widget**

Create `components/widgets/MarketPulse.tsx`:

```tsx
"use client";

import { MarketPulseProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends MarketPulseProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

const volatilityConfig = {
  Low: { color: "text-gain", bg: "bg-gain-light", label: "Calm", width: "25%" },
  Medium: { color: "text-amber-600", bg: "bg-amber-50", label: "Moderate", width: "55%" },
  High: { color: "text-loss", bg: "bg-loss-light", label: "Volatile", width: "90%" },
};

export default function MarketPulse({ volatility, summary, sectorHighlights, experience, mood }: Props) {
  const config = volatilityConfig[volatility];

  return (
    <div className="bg-surface rounded-2xl shadow-sm p-6">
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-base font-semibold">Market Pulse</h3>
        <span className={`text-xs font-medium px-2.5 py-1 rounded-full ${config.bg} ${config.color}`}>
          {config.label}
        </span>
      </div>

      {/* Volatility gauge */}
      <div className="mb-4">
        <div className="w-full h-2 bg-gray-100 rounded-full overflow-hidden">
          <div
            className={`h-full rounded-full transition-all duration-1000 ${
              volatility === "Low" ? "bg-gain" : volatility === "Medium" ? "bg-amber-500" : "bg-loss"
            }`}
            style={{ width: config.width }}
          />
        </div>
      </div>

      {/* Summary */}
      <p className="text-sm text-text-secondary leading-relaxed mb-4">{summary}</p>

      {/* Sector highlights — expert only */}
      {experience === "expert" && sectorHighlights && sectorHighlights.length > 0 && (
        <div className="border-t border-border pt-4 space-y-2">
          <p className="text-xs text-text-tertiary uppercase tracking-wider font-medium">Sectors</p>
          {sectorHighlights.map((s) => (
            <div key={s.sector} className="flex items-center justify-between">
              <span className="text-sm text-text-secondary">{s.sector}</span>
              <span className={`text-sm font-medium ${s.changePct >= 0 ? "text-gain" : "text-loss"}`}>
                {s.changePct >= 0 ? "+" : ""}{s.changePct}%
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Step 2: Create HistoricalContext widget**

Create `components/widgets/HistoricalContext.tsx`:

```tsx
"use client";

import { HistoricalContextProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends HistoricalContextProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

export default function HistoricalContext({
  event, description, recoveryMonths, previousOccurrences, experience, mood,
}: Props) {
  return (
    <div className="bg-surface rounded-2xl shadow-sm p-6">
      <div className="flex items-start gap-3 mb-4">
        <div className="w-10 h-10 bg-blue-50 rounded-full flex items-center justify-center flex-shrink-0">
          <svg className="w-5 h-5 text-accent" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 6v6h4.5m4.5 0a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
        </div>
        <div>
          <h3 className="text-base font-semibold">{event}</h3>
          <p className="text-sm text-text-secondary leading-relaxed mt-1">{description}</p>
        </div>
      </div>

      {recoveryMonths && (
        <div className={`${mood === "anxious" ? "bg-gain-light" : "bg-gray-50"} rounded-xl p-4 mb-4`}>
          <p className="text-sm">
            <span className="font-medium">Average recovery:</span>{" "}
            <span className="text-text-secondary">{recoveryMonths} months</span>
          </p>
        </div>
      )}

      {/* Expert: show historical occurrences */}
      {experience === "expert" && previousOccurrences && previousOccurrences.length > 0 && (
        <div className="border-t border-border pt-4 space-y-3">
          <p className="text-xs text-text-tertiary uppercase tracking-wider font-medium">Historical parallels</p>
          {previousOccurrences.map((occ, i) => (
            <div key={i} className="flex items-center justify-between text-sm">
              <span className="text-text-secondary">{occ.date}</span>
              <span className="text-loss font-medium">{occ.dropPct}%</span>
              <span className="text-gain text-xs">{occ.recovery}</span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Step 3: Create InsightCard widget**

Create `components/widgets/InsightCard.tsx`:

```tsx
"use client";

import { InsightCardProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends InsightCardProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

const categoryStyles = {
  education: { icon: "💡", bg: "bg-amber-50", label: "Learn" },
  action: { icon: "⚡", bg: "bg-blue-50", label: "Action needed" },
  tip: { icon: "✦", bg: "bg-violet-50", label: "Tip" },
};

export default function InsightCard({ title, content, category, experience, mood }: Props) {
  const style = categoryStyles[category];

  return (
    <div className="bg-surface rounded-2xl shadow-sm p-6">
      <div className="flex items-start gap-4">
        <div className={`w-10 h-10 ${style.bg} rounded-full flex items-center justify-center flex-shrink-0 text-lg`}>
          {style.icon}
        </div>
        <div className="flex-1">
          <div className="flex items-center gap-2 mb-1">
            <h3 className="text-base font-semibold">{title}</h3>
            <span className={`text-xs px-2 py-0.5 rounded-full ${style.bg} text-text-secondary`}>
              {style.label}
            </span>
          </div>
          <p className="text-sm text-text-secondary leading-relaxed">{content}</p>
        </div>
      </div>
    </div>
  );
}
```

**Step 4: Create WelcomeBackCard widget**

Create `components/widgets/WelcomeBackCard.tsx`:

```tsx
"use client";

import { WelcomeBackCardProps, ExperienceLevel, MoodState } from "@/lib/types";

interface Props extends WelcomeBackCardProps {
  experience: ExperienceLevel;
  mood: MoodState;
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("de-DE", {
    style: "currency",
    currency: "EUR",
    minimumFractionDigits: 0,
    maximumFractionDigits: 0,
  }).format(value);
}

export default function WelcomeBackCard({
  daysSinceLastVisit, portfolioChangePct, dividendsReceived, pendingActions, summary,
  experience, mood,
}: Props) {
  const isPositive = portfolioChangePct >= 0;

  return (
    <div className="bg-gradient-to-br from-slate-900 to-slate-800 text-white rounded-2xl shadow-lg p-6">
      <div className="mb-4">
        <p className="text-sm text-slate-400 mb-1">Welcome back</p>
        <h3 className="text-xl font-bold">
          {daysSinceLastVisit} days since your last visit
        </h3>
      </div>

      <p className="text-sm text-slate-300 leading-relaxed mb-6">{summary}</p>

      {/* Key metrics */}
      <div className="grid grid-cols-2 gap-3">
        <div className="bg-white/10 rounded-xl p-4">
          <p className="text-xs text-slate-400 mb-1">Portfolio</p>
          <p className={`text-lg font-bold ${isPositive ? "text-emerald-400" : "text-rose-400"}`}>
            {isPositive ? "+" : ""}{portfolioChangePct}%
          </p>
        </div>

        {dividendsReceived !== null && dividendsReceived > 0 && (
          <div className="bg-white/10 rounded-xl p-4">
            <p className="text-xs text-slate-400 mb-1">Dividends earned</p>
            <p className="text-lg font-bold text-emerald-400">{formatCurrency(dividendsReceived)}</p>
          </div>
        )}

        {pendingActions && (
          <div className="bg-white/10 rounded-xl p-4 col-span-2">
            <p className="text-xs text-slate-400 mb-1">Needs attention</p>
            <p className="text-sm font-medium text-amber-400">{pendingActions}</p>
          </div>
        )}
      </div>
    </div>
  );
}
```

**Step 5: Commit**

```bash
git add components/widgets/
git commit -m "feat: add MarketPulse, HistoricalContext, InsightCard, and WelcomeBackCard widgets"
```

---

## Task 7: Widget Renderer + Scenario Selector + Main Page

**Files:**
- Create: `components/widgets/WidgetRenderer.tsx`
- Create: `components/ScenarioSelector.tsx`
- Create: `components/AppShell.tsx`
- Modify: `app/page.tsx`

**Step 1: Create WidgetRenderer**

Create `components/widgets/WidgetRenderer.tsx`:

```tsx
"use client";

import { WidgetProps, ExperienceLevel, MoodState } from "@/lib/types";
import PortfolioSummary from "./PortfolioSummary";
import StockCard from "./StockCard";
import MarketPulse from "./MarketPulse";
import NarrativeCard from "./NarrativeCard";
import HistoricalContext from "./HistoricalContext";
import InsightCard from "./InsightCard";
import WelcomeBackCard from "./WelcomeBackCard";

interface Props {
  widget: WidgetProps;
  experience: ExperienceLevel;
  mood: MoodState;
}

export default function WidgetRenderer({ widget, experience, mood }: Props) {
  switch (widget.type) {
    case "portfolio_summary":
      return <PortfolioSummary {...widget.props} experience={experience} mood={mood} />;
    case "stock_card":
      return <StockCard {...widget.props} experience={experience} mood={mood} />;
    case "market_pulse":
      return <MarketPulse {...widget.props} experience={experience} mood={mood} />;
    case "narrative_card":
      return <NarrativeCard {...widget.props} mood={mood} />;
    case "historical_context":
      return <HistoricalContext {...widget.props} experience={experience} mood={mood} />;
    case "insight_card":
      return <InsightCard {...widget.props} experience={experience} mood={mood} />;
    case "welcome_back_card":
      return <WelcomeBackCard {...widget.props} experience={experience} mood={mood} />;
    default:
      return null;
  }
}
```

**Step 2: Create ScenarioSelector**

Create `components/ScenarioSelector.tsx`:

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
    <div className="bg-surface border-b border-border sticky top-0 z-50">
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
                  ? "bg-slate-900 text-white shadow-md"
                  : "bg-gray-100 text-text-secondary hover:bg-gray-200"
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

**Step 3: Create AppShell**

Create `components/AppShell.tsx`:

```tsx
"use client";

import { useState } from "react";
import { motion, AnimatePresence } from "framer-motion";
import { UILayout } from "@/lib/types";
import { scenarios } from "@/lib/data/context-signals";
import ScenarioSelector from "./ScenarioSelector";
import WidgetRenderer from "./widgets/WidgetRenderer";

export default function AppShell() {
  const [selectedScenario, setSelectedScenario] = useState<string | null>(null);
  const [layout, setLayout] = useState<UILayout | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSelectScenario = async (scenarioId: string) => {
    setSelectedScenario(scenarioId);
    setLoading(true);
    setError(null);
    setLayout(null);

    try {
      const res = await fetch("/api/generate-ui", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ scenarioId }),
      });

      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.error || "Failed to generate UI");
      }

      const data: UILayout = await res.json();
      setLayout(data);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Something went wrong");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-background">
      <ScenarioSelector
        scenarios={scenarios}
        selected={selectedScenario}
        onSelect={handleSelectScenario}
        loading={loading}
      />

      <main className="max-w-lg mx-auto px-4 py-6">
        {/* Empty state */}
        {!selectedScenario && !loading && (
          <div className="text-center py-20">
            <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mx-auto mb-4">
              <svg className="w-8 h-8 text-text-tertiary" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M15.59 14.37a6 6 0 01-5.84 7.38v-4.8m5.84-2.58a14.98 14.98 0 006.16-12.12A14.98 14.98 0 009.631 8.41m5.96 5.96a14.926 14.926 0 01-5.841 2.58m-.119-8.54a6 6 0 00-7.381 5.84h4.8m2.581-5.84a14.927 14.927 0 00-2.58 5.841m2.699 2.7c-.103.021-.207.041-.311.06a15.09 15.09 0 01-2.448-2.448 14.9 14.9 0 01.06-.312m-2.24 2.39a4.493 4.493 0 00-1.757 4.306 4.493 4.493 0 004.306-1.758M16.5 9a1.5 1.5 0 11-3 0 1.5 1.5 0 013 0z" />
              </svg>
            </div>
            <h2 className="text-lg font-semibold mb-1">Choose a scenario</h2>
            <p className="text-sm text-text-secondary max-w-xs mx-auto">
              Select an investor scenario above to see how the interface adapts to their moment.
            </p>
          </div>
        )}

        {/* Loading state */}
        {loading && (
          <div className="text-center py-20">
            <div className="w-12 h-12 border-[3px] border-gray-200 border-t-slate-900 rounded-full animate-spin mx-auto mb-4" />
            <p className="text-sm text-text-secondary">Composing your experience...</p>
          </div>
        )}

        {/* Error state */}
        {error && (
          <div className="bg-loss-light border border-rose-200 rounded-2xl p-6 text-center">
            <p className="text-sm text-loss font-medium mb-2">Something went wrong</p>
            <p className="text-xs text-text-secondary">{error}</p>
            <button
              onClick={() => selectedScenario && handleSelectScenario(selectedScenario)}
              className="mt-4 px-4 py-2 bg-slate-900 text-white text-sm rounded-full"
            >
              Try again
            </button>
          </div>
        )}

        {/* Layout */}
        {layout && !loading && (
          <div className={`mood-${layout.mood}`}>
            {/* Top narrative */}
            <motion.p
              initial={{ opacity: 0, y: 10 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.4 }}
              className="text-lg font-semibold text-text-primary mb-6 leading-snug"
            >
              {layout.narrative}
            </motion.p>

            {/* Widgets */}
            <AnimatePresence mode="wait">
              <div className="space-y-4">
                {layout.widgets.map((widget, index) => (
                  <motion.div
                    key={`${widget.type}-${index}`}
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{
                      duration: 0.4,
                      delay: 0.1 + index * 0.15,
                      ease: [0.25, 0.46, 0.45, 0.94],
                    }}
                  >
                    <WidgetRenderer
                      widget={widget}
                      experience={layout.experience}
                      mood={layout.mood}
                    />
                  </motion.div>
                ))}
              </div>
            </AnimatePresence>
          </div>
        )}
      </main>

      {/* Footer */}
      <footer className="max-w-lg mx-auto px-4 py-8 text-center">
        <p className="text-xs text-text-tertiary">
          Powered by Claude AI — Every view is uniquely generated
        </p>
      </footer>
    </div>
  );
}
```

**Step 4: Update app/page.tsx**

Replace `app/page.tsx` with:

```tsx
import AppShell from "@/components/AppShell";

export default function Home() {
  return <AppShell />;
}
```

**Step 5: Commit**

```bash
git add components/ app/page.tsx
git commit -m "feat: add WidgetRenderer, ScenarioSelector, AppShell, and wire up main page"
```

---

## Task 8: Polish + Testing

**Files:**
- Modify: `app/globals.css` (add scrollbar-hide utility)
- Modify: `next.config.ts` (if needed)

**Step 1: Add scrollbar-hide CSS utility**

Append to `app/globals.css`:

```css
.scrollbar-hide {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
.scrollbar-hide::-webkit-scrollbar {
  display: none;
}
```

**Step 2: Test locally**

```bash
bun dev
```

Open http://localhost:3000 in browser. Test each scenario:
1. Click "Sophie — Calm Evening" → should show calm, beginner layout
2. Click "Sophie — Crash Morning" → should show anxious, fewer widgets, reassuring
3. Click "Marc — Back After 3 Months" → should show welcome back card, catch-up flow
4. Click "Marc — Research Mode" → should show dense, expert data

Click same scenario twice — layout should differ each time (different narrative, possibly different widget selection).

**Step 3: Fix any issues, then commit**

```bash
git add -A
git commit -m "feat: polish styles and complete adaptive investing UI"
```

---

## Dependency Graph

```
Task 1 (scaffold)
  └→ Task 2 (data + types)
       ├→ Task 3 (AI provider)
       │    └→ Task 4 (API route)
       ├→ Task 5 (widgets part 1)
       └→ Task 6 (widgets part 2)
            └→ Task 7 (assembly — depends on 4, 5, 6)
                 └→ Task 8 (polish + test)
```

Tasks 3, 5, and 6 can run in parallel after Task 2 completes.

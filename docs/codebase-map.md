# Codebase Map — InvestSuite Adaptive Investing

> Dense annotated file tree. Format: `FileName — purpose`. No prose.

## Architecture

```
Next.js 14+ (Face)  ←→  ASP.NET Core 9 (Brain)  ←→  Claude API (LLM)
```

## Root

```
CLAUDE.md           — Project rules, architecture, conventions, deadline
README.md           — Project overview, widgets, chips, scenarios, tech stack
.gitignore          — Git ignore rules
```

## Docs

```
docs/
├── INDEX.md                    — Master document index
├── ARCHITECTURE.md             — System architecture overview
├── backend-spec.md             — Full backend spec (API source of truth)
├── case-problem.md             — Case: why today's investing apps fail
├── case-architecture.md        — Case: Brain/Face, provocations, deliverables
├── case-rules.md               — Case: 4 non-negotiable constraints
├── case-scenarios.md           — Case: 3 demo scenarios
├── case-glossary.md            — Case: Stock, ETF, Volatility, P/E, Dividend
├── pdf/                        — Original case PDF
├── investitute/                — PDF case materials
├── standards/
│   ├── architecture.md         — Layered backend, frontend structure, CSS Modules, testing
│   ├── coding-conventions.md   — Enums, functions, interfaces, naming, max 15 lines
│   └── dotnet-practices.md     — Entity design, converters, repos, EF Core, DI
├── strategy/
│   ├── deployment.md           — Vercel + Render free tier
│   └── tech-stack.md           — Next.js, ASP.NET, Claude, Mantine
└── plans/
    ├── product-backlog.md                         — Prioritized task list (9 items) for incremental work
    ├── 2026-03-11-adaptive-investing-ui-design.md
    ├── 2026-03-11-adaptive-investing-ui-plan.md
    ├── 2026-03-11-bubble-home-redesign-design.md
    ├── 2026-03-11-bubble-home-redesign.md
    └── 2026-03-11-tool-use-architecture.md
```

## Prompts

```
backend/prompts/
├── README.md                   — Prompt assembly guide (two flows: API + CLI)
├── system/
│   ├── 00-system.md            — System prompt for Claude API
│   ├── 01-role.md              — Brain identity and core role
│   ├── 02-rules.md             — Non-negotiable constraints
│   ├── 03-tone-guide.md        — Signal-to-tone/density/widget mapping
│   ├── 04-widget-catalog.md    — 7 widget types with JSON schemas
│   ├── 05-output-format.md     — Response structure + 19-chip pool
│   └── 06-interaction-rules.md — 9 intent categories for interactions
├── context/
│   ├── investor-template.md    — Investor profile + portfolio template
│   ├── signals-template.md     — Context signals template
│   └── interaction-template.md — User message + history template
└── widget-manifests/
    ├── historical_context.json — Historical context widget schema
    ├── insight_card.json       — Insight card widget schema
    ├── market_pulse.json       — Market pulse widget schema
    └── narrative_card.json     — Narrative card widget schema
```

## Data

```
data/                           — (empty, data moved to backend repos)
```

## Backend

```
backend/
├── InvestSuite.slnx            — Solution file
├── Dockerfile                  — Docker containerization
├── docker-compose.yml          — Docker Compose config
├── deploy.sh                   — Deployment script
├── nuget.config                — NuGet configuration
├── .dockerignore               — Docker ignore rules
├── .env.example                — Backend env template
│
├── src/InvestSuite.Api/
│   ├── InvestSuite.Api.csproj  — Project config (net9.0, ASP.NET Core)
│   ├── Program.cs              — DI, middleware, AddControllers + MapControllers (registers INewsService)
│   ├── appsettings.example.json — Config template (copy to appsettings.json, add API key)
│   │
│   ├── Controllers/
│   │   ├── AccountController.cs       — POST create account (€1,000 cash, SQLite)
│   │   ├── LayoutController.cs        — GET context/{accountId}, POST layout (LLM), GET accounts, GET key-moments
│   │   ├── StockController.cs         — GET stock price at historical date
│   │   └── TradeController.cs         — POST buy/sell stock at historical date
│   │
│   ├── Infrastructure/
│   │   ├── Claude/
│   │   │   ├── CachedClaudeClient.cs  — In-memory cache decorator (SHA-256 key, 10min TTL)
│   │   │   ├── ClaudeClient.cs        — HTTP client: GenerateSimpleAsync<T> (single prompt → JSON)
│   │   │   ├── ClaudeOptions.cs       — Config: API key, model, base URL
│   │   │   ├── IClaudeClient.cs       — Interface for ClaudeClient
│   │   │   ├── IPromptBuilder.cs      — Interface for PromptBuilder
│   │   │   └── PromptBuilder.cs       — BuildLayoutPayloadPrompt: compact format, filtered stock details, JSON schema
│   │   ├── Data/
│   │   │   ├── DataLoader.cs          — DI registration: static repos + SQLite/EF Core + Yahoo Finance
│   │   │   ├── EfAccountRepository.cs — EF Core account repo (SQLite, wraps legacy + new accounts)
│   │   │   ├── EfTransactionRepository.cs — EF Core transaction repo (SQLite, wraps legacy + new trades)
│   │   │   ├── IAccountRepository.cs  — Interface for AccountRepository
│   │   │   ├── InvestSuiteDbContext.cs — EF Core DbContext (SQLite: Accounts, Transactions, StockPriceHistory)
│   │   │   ├── IScenarioRepository.cs — Interface for ScenarioRepository
│   │   │   ├── IStockRepository.cs    — Interface for StockRepository
│   │   │   ├── ITransactionRepository.cs — Interface for TransactionRepository
│   │   │   ├── KeyMomentRepository.cs — Static: key market moments for time-travel (COVID crash, chip shortage, etc.)
│   │   │   ├── ScenarioRepository.cs  — Static: 4 scenarios with context signals
│   │   │   ├── StockRepository.cs     — Static: 8 stock details + 2 portfolios (with shares, cost basis)
│   │   │   └── TransactionRepository.cs — Static mock: buy/sell/dividend/deposit history for both accounts
│   │   └── Services/
│   │       ├── AdaptiveLayoutService.cs        — BuildContextLayout (fast) + GenerateLayoutPayloadAsync (LLM) + 7-day portfolio change
│   │       ├── HoldingsCalculator.cs  — Compute holdings from transactions + historical prices at a given date
│   │       ├── IAdaptiveLayoutService.cs       — Interface for AdaptiveLayoutService
│   │       ├── IHistoricalPriceService.cs — Interface: GetClosingPrice + GetLatestAvailableDate
│   │       ├── INewsService.cs        — Interface + NewsItem record (Headline, Source, PublishedAt, Sentiment)
│   │       ├── YahooFinanceNewsService.cs — Fetch + cache recent news headlines from Yahoo Finance RSS
│   │       └── YahooFinancePriceService.cs — Fetch + cache historical prices; fallback to latest for future/simulated dates
│   │
│   └── Models/
│       ├── Account.cs                 — Record: Id, FirstName, LastName, Email, DOB, RiskProfile, ExperienceLevel
│       ├── ClaudeLayoutDecision.cs    — Record: deserialized LLM response (Tone, Sentiment, ViewMode, AiMessage, Suggestions, optional fields)
│       ├── ContextSignals.cs          — Record: 13 fields (accountId, scenario, mood, volatility, etc.)
│       ├── Enums.cs                   — Domain enums: RiskProfile, ExperienceLevel, TransactionType, HoldingType
│       ├── Holding.cs                 — Record: Symbol, Name, HoldingType, Shares, AverageCost, CurrentPrice, Value
│       ├── CreateAccountRequest.cs     — DTO: Name for account creation
│       ├── ErrorResponse.cs           — Record: Error + optional Detail for API errors
│       ├── KeyMoment.cs               — Record: Id, Date, Title, Description, AffectedStocks, Emotion, Volatility
│       ├── LayoutPayloadModels.cs     — LayoutRequest DTO (+ Date field) + 15 records + 13 UI enums
│       ├── ScenarioOption.cs          — Record: AccountSummary (Id, Name, ExperienceLevel, PortfolioTotal, Scenarios)
│       ├── StockDetail.cs             — Record: 15 fields (price, P/E, analysts, scores)
│       ├── StockPriceHistory.cs       — Entity: Symbol, Date, Open, High, Low, Close, Volume (SQLite)
│       ├── SellRequest.cs             — DTO: Symbol, Shares, Date for sell execution
│       ├── TradeRequest.cs            — DTO: Symbol, Amount, Date for trade execution
│       └── Transaction.cs             — Record: Id, AccountId, Type, Symbol, Shares, PricePerShare, Amount, ExecutedAt
│
└── tests/InvestSuite.Tests/
    ├── InvestSuite.Tests.csproj       — xUnit + FluentAssertions + Moq + EF Core SQLite
    ├── Controllers/
    │   └── TradeControllerTests.cs    — 15 tests: buy/sell endpoints (validation, persistence, cash balance)
    ├── Services/
    │   └── HoldingsCalculatorTests.cs — 6 tests: holdings aggregation (buy, sell, average cost, cash)
    └── Infrastructure/
        ├── PromptBuilderTests.cs       — 14 tests: account profile, holdings, transactions, signals, format
        └── RepositoryTests.cs          — 10 tests: Scenario/Stock repos, data integrity
```

## Frontend

```
frontend/
├── package.json                — Next.js 14, React 19, Tailwind, Radix UI, Clerk
├── .env.example                — Environment variable template (Clerk publishable/secret keys)
├── .users.example.json         — Empty user store template (copy to .users.json)
├── tsconfig.json               — TypeScript config
├── next.config.ts              — Next.js config
├── postcss.config.mjs          — PostCSS config
├── components.json             — shadcn/ui component config
├── eslint.config.mjs           — ESLint config
│
├── app/
│   ├── layout.tsx              — Root layout (fonts, ClerkProvider)
│   ├── page.tsx                — Home: two-step load (context → AI), Clerk session
│   ├── globals.css             — Global styles + design tokens
│   └── api/
│       ├── accounts/by-clerk-id/[clerkUserId]/route.ts — GET proxy → backend /api/accounts/by-clerk-id (lookup)
│       ├── accounts/create/route.ts        — POST proxy → backend /api/accounts (create account)
│       ├── accounts/[accountId]/buy/route.ts  — POST proxy → backend /api/trade/buy (execute buy)
│       ├── accounts/[accountId]/sell/route.ts — POST proxy → backend /api/trade/sell (execute sell)
│       ├── context/[userId]/route.ts       — GET proxy → backend /api/context/{userId} (fast, + date param)
│       ├── key-moments/route.ts            — GET proxy → backend /api/key-moments
│       ├── layout/route.ts                 — POST proxy → backend /api/layout (LLM, 45s timeout)
│       └── users/route.ts                  — GET proxy → backend /api/accounts
│
├── components/
│   ├── core/
│   │   ├── AIMessage.tsx       — AI message display
│   │   ├── BubbleStrip.tsx     — Portfolio bubble visualization
│   │   ├── BuyBubble.tsx       — Persistent floating buy FAB button (unused)
│   │   ├── CenterContent.tsx   — Centered content wrapper
│   │   ├── ExpandedCard.tsx    — Expandable card overlay
│   │   ├── InputBar.tsx        — User input bar (loading blocks input; aiLoading shows ··· only)
│   │   └── UserMenu.tsx        — User avatar dropdown with sign-out (Clerk session)
│   │
│   ├── ambient/
│   │   ├── AmbientBackground.tsx   — Background animation (sentiment → gradients)
│   │   ├── BreathingRings.tsx      — Breathing ring animation
│   │   └── FloatingBubbles.tsx     — Floating bubble animation
│   │
│   ├── ui/                     — 25 UI primitives (card, button, badge, dialog, etc.)
│   │   ├── accent-bar.tsx      — Accent color bar
│   │   ├── glass-card.tsx      — Glassmorphism card
│   │   ├── heatmap-grid.tsx    — Heatmap visualization
│   │   ├── metric-grid.tsx     — Metrics grid
│   │   ├── widget-card.tsx     — Widget container
│   │   └── ... (20 more)
│   │
│   ├── account/
│   │   ├── CreateAccountForm.tsx   — Account creation form for time-travel simulator
│   │   ├── LandingScreen.tsx       — Landing page with create account / view demos options
│   │   └── OnboardingFlow.tsx      — Multi-step onboarding (experience, risk, personality)
│   │
│   ├── time-travel/
│   │   └── KeyMomentCards.tsx      — Horizontal scrolling key market moment cards
│   │
│   ├── widgets/
│   │   ├── WidgetRenderer.tsx  — Dynamic widget dispatcher (type → component, + trade props)
│   │   ├── AIInsight.tsx       — AI insight widget
│   │   ├── BuyFlow.tsx         — Buy/order flow widget (discrete share input, slider, real trade execution)
│   │   ├── SellFlow.tsx        — Sell flow widget (share-based selling with position summary)
│   │   ├── StockPicker.tsx     — Browsable stock list for buying or selling
│   │   ├── ComparisonTable.tsx — Side-by-side comparison
│   │   ├── Dividends.tsx       — Dividends widget
│   │   ├── NewsDigest.tsx      — News digest widget
│   │   ├── OrderStatus.tsx     — Order status widget
│   │   └── SectorHeatmap.tsx   — Sector heatmap widget
│   │
│   └── expanded-sections/      — 8 detail overlays (Comparison, Context, Education, etc.)
│       └── index.ts            — Barrel export
│
├── middleware.ts               — Clerk auth middleware (protects all routes except sign-in, key-moments)
│
├── hooks/
│   └── useLayout.ts            — Two-step layout: loading (fast context, blocks input) + aiLoading (AI phase, non-blocking)
│
├── lib/
│   ├── ambient.ts              — Ambient effect config (sentiment → gradients)
│   ├── api.ts                  — API client: fetchContext, fetchLayout, fetchKeyMoments, createAccount, buyStock, sellStock (no streaming)
│   ├── theme.ts                — Theme + tone system (mood → CSS vars, with fallbacks)
│   └── utils.ts                — Utility functions
│
├── providers/
│   └── ThemeProvider.tsx        — Theme context provider
│
├── types/
│   └── layout.ts               — LayoutPayload types (enums, interfaces, widget configs, KeyMoment, Trade types)
│
└── public/
    ├── InstrumentSerif-*.ttf   — Custom fonts
    └── *.svg                   — SVG assets
```

## Skills & Commands

```
.claude/
├── commands/                   — Slash commands
│   ├── brain-endpoint.md       — Create Brain API endpoint
│   ├── commit.md               — Git commit with auto-updated codebase map
│   ├── scaffold.md             — Scaffold full project structure
│   ├── scenario.md             — Implement/test demo scenario
│   └── widget.md               — Create frontend widget
│
├── hooks/
│   └── post-commit-index.sh    — PostToolUse hook: updates map baseline + amends commit
│
├── settings.json               — Hook configuration (PostToolUse → post-commit-index)
│
└── skills/                     — Skills (SKILL.md format)
    ├── docs/SKILL.md           — Search docs, key entry point table
    ├── formatting/SKILL.md     — C# + TS formatting conventions
    ├── review-conventions/SKILL.md — Universal coding convention checks
    ├── review-dotnet/SKILL.md  — C# patterns (entities, converters, async)
    ├── review-frontend/SKILL.md — React/Next.js patterns (CSS Modules, state)
    ├── review-tests/SKILL.md   — Test quality (naming, constants, mocking)
    └── update-index/SKILL.md   — Incremental codebase map updater
```

<!-- last-indexed: 9c94749 on main at 2026-03-11 -->

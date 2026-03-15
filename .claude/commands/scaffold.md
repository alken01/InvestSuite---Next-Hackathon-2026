Scaffold the full project structure for the InvestSuite adaptive investing interface.

## What to create

### Frontend (Next.js + Mantine)
```
frontend/
├── src/
│   ├── app/
│   │   ├── layout.tsx              # Root layout with MantineProvider
│   │   ├── page.tsx                # Main adaptive interface page
│   │   └── globals.css
│   ├── components/
│   │   ├── widgets/                # Widget library
│   │   │   ├── WidgetRenderer.tsx  # Dynamic widget renderer (maps type → component)
│   │   │   ├── PortfolioOverview.tsx
│   │   │   ├── StockCard.tsx
│   │   │   ├── HistoricalChart.tsx
│   │   │   ├── VolatilityGauge.tsx
│   │   │   ├── ComparisonView.tsx
│   │   │   └── ExplanationCard.tsx
│   │   ├── layout/
│   │   │   ├── AppShell.tsx        # Main app shell
│   │   │   └── InvestorSelector.tsx # Account selector
│   │   └── narrative/
│   │       └── NarrativeBlock.tsx  # Renders the brain's narrative text
│   ├── lib/
│   │   ├── api/
│   │   │   └── brain.ts           # API client for brain endpoint
│   │   ├── types/
│   │   │   ├── widgets.ts         # Widget type definitions
│   │   │   ├── investors.ts       # Investor/portfolio types
│   │   │   └── signals.ts         # Context signal types
│   │   └── hooks/
│   │       └── useBrain.ts        # Hook to fetch and manage brain responses
│   └── styles/
│       └── theme.ts               # Mantine theme customization
├── package.json
├── next.config.js                  # API rewrites to backend
├── tsconfig.json
└── postcss.config.cjs
```

### Backend (ASP.NET Core 9)
```
backend/
├── src/
│   ├── Api/
│   │   ├── Program.cs
│   │   ├── Controllers/
│   │   │   └── BrainController.cs  # Main adaptive UI endpoint
│   │   └── Api.csproj
│   ├── Core/
│   │   ├── Models/
│   │   │   ├── Investor.cs
│   │   │   ├── Portfolio.cs
│   │   │   ├── ContextSignals.cs
│   │   │   └── WidgetResponse.cs
│   │   ├── Enums/
│   │   │   ├── EmotionalState.cs
│   │   │   ├── MarketVolatility.cs
│   │   │   └── WidgetType.cs
│   │   ├── Services/
│   │   │   ├── IBrainService.cs
│   │   │   └── BrainService.cs     # Decision engine: signals → widgets
│   │   └── Core.csproj
│   └── Infrastructure/
│       ├── Data/
│       │   ├── AppDbContext.cs
│       │   └── SeedData.cs         # Seed stock + price data
│       ├── Claude/
│       │   └── ClaudeClient.cs     # Claude API integration for narratives
│       └── Infrastructure.csproj
├── Dockerfile
└── Solution.sln
```

## Setup commands
```bash
# Frontend
npx create-next-app@latest frontend --typescript --app --src-dir
cd frontend && npm install @mantine/core @mantine/hooks @mantine/charts recharts postcss-preset-mantine postcss-simple-vars

# Backend
dotnet new sln -n InvestSuite
dotnet new webapi -n Api -o src/Api
dotnet new classlib -n Core -o src/Core
dotnet new classlib -n Infrastructure -o src/Infrastructure
dotnet sln add src/Api src/Core src/Infrastructure
```

## Key setup items
- MantineProvider in root layout
- postcss-preset-mantine in postcss config
- API rewrite in next.config.js pointing to backend
- SQLite connection in Program.cs
- Stock and price data seeded via StockRepository
- Health check endpoint at `/health`
- Swagger enabled

## Reference
- Tech stack: `docs/strategy/tech-stack.md`
- Deployment: `docs/strategy/deployment.md`
- Architecture: `docs/standards/architecture.md`
- .NET: `docs/standards/dotnet-practices.md`

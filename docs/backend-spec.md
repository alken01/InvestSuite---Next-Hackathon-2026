# Backend Specification

## Project Structure

```
backend/
├── src/
│   ├── InvestSuite.Api/
│   │   ├── Program.cs
│   │   ├── Api.csproj
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/
│   │   │   ├── BrainController.cs
│   │   │   ├── InvestorsController.cs
│   │   │   ├── ScenariosController.cs
│   │   │   └── StocksController.cs
│   │   ├── Converters/
│   │   │   ├── BrainConverters.cs
│   │   │   ├── InvestorConverters.cs
│   │   │   └── ScenarioConverters.cs
│   │   └── Middleware/
│   │       └── ExceptionMiddleware.cs
│   │
│   ├── InvestSuite.Core/
│   │   ├── Core.csproj
│   │   ├── Entities/
│   │   │   ├── Investor.cs
│   │   │   ├── Portfolio.cs
│   │   │   ├── Holding.cs
│   │   │   ├── ContextSignals.cs
│   │   │   ├── Scenario.cs
│   │   │   └── StockDetail.cs
│   │   ├── Enums/
│   │   │   ├── Tone.cs
│   │   │   ├── Density.cs
│   │   │   ├── WidgetType.cs
│   │   │   ├── HoldingType.cs
│   │   │   ├── MarketVolatility.cs
│   │   │   ├── EmotionalState.cs
│   │   │   ├── ExplanationIcon.cs
│   │   │   ├── ExplanationCategory.cs
│   │   │   └── StockDetailLevel.cs
│   │   ├── Interfaces/
│   │   │   ├── IBrainService.cs
│   │   │   ├── IInvestorRepository.cs
│   │   │   ├── IScenarioRepository.cs
│   │   │   ├── IStockRepository.cs
│   │   │   ├── IClaudeClient.cs
│   │   │   └── IPromptBuilder.cs
│   │   ├── Features/
│   │   │   ├── Dashboard/
│   │   │   │   ├── GetDashboardQuery.cs
│   │   │   │   └── GetDashboardHandler.cs
│   │   │   └── Interaction/
│   │   │       ├── ProcessInteractionCommand.cs
│   │   │       └── ProcessInteractionHandler.cs
│   │   ├── DTOs/
│   │   │   ├── DashboardResponse.cs
│   │   │   ├── BrainOutput.cs
│   │   │   ├── InteractionRequest.cs
│   │   │   ├── ConversationEntry.cs
│   │   │   ├── InvestorDto.cs
│   │   │   ├── InvestorSummaryDto.cs
│   │   │   ├── PortfolioDto.cs
│   │   │   ├── HoldingDto.cs
│   │   │   ├── ScenarioDto.cs
│   │   │   ├── ScenarioSummaryDto.cs
│   │   │   ├── SignalsDto.cs
│   │   │   ├── StockDetailDto.cs
│   │   │   ├── AnalystConsensusDto.cs
│   │   │   ├── StockScoresDto.cs
│   │   │   ├── WidgetDto.cs
│   │   │   ├── SuggestedActionDto.cs
│   │   │   └── ErrorResponse.cs
│   │   └── Exceptions/
│   │       ├── InvestorNotFoundException.cs
│   │       ├── ScenarioNotFoundException.cs
│   │       └── ClaudeApiException.cs
│   │
│   └── InvestSuite.Infrastructure/
│       ├── Infrastructure.csproj
│       ├── DependencyInjection.cs
│       ├── Data/
│       │   ├── JsonDataLoader.cs
│       │   ├── InvestorRepository.cs
│       │   ├── ScenarioRepository.cs
│       │   └── StockRepository.cs
│       ├── Claude/
│       │   ├── ClaudeClient.cs
│       │   ├── ClaudeOptions.cs
│       │   └── PromptBuilder.cs
│       └── Services/
│           ├── BrainService.cs
│           ├── WidgetEnricher.cs
│           ├── ChipResolver.cs
│           └── ChartDataGenerator.cs
│
├── tests/
│   └── InvestSuite.Tests/
│       ├── Tests.csproj
│       ├── Features/
│       │   └── GetDashboardHandlerTests.cs
│       └── Infrastructure/
│           ├── PromptBuilderTests.cs
│           └── ChartDataGeneratorTests.cs
│
├── InvestSuite.sln
└── Dockerfile
```

---

## Endpoints

### GET /api/brain/dashboard/{investorId}?scenario={scenarioKey}

The core endpoint. Returns the adaptive UI response for an investor in a given context.

**Parameters:**

| Param | Location | Type | Required | Description |
|-------|----------|------|----------|-------------|
| `investorId` | path | string | yes | Account ID (from account creation) |
| `scenario` | query | string | no | Scenario key. Defaults to first scenario for that investor. |

**Scenario keys:**

> Legacy examples shown below. In practice, signals are generated from time-travel key moments during simulator playback.

| Key | Investor | Description |
|-----|----------|-------------|
| `calm_tuesday` | (example) | Calm Tuesday evening, low volatility, +0.8% week |
| `crash_morning` | (example) | Morning after crash, high volatility, -6.2% week |
| `returning` | (example) | Back after 94 days, +4.2% growth, €525 dividends |
| `research` | (example) | Research mode, 3rd session today, searching semiconductors |

**Response 200:**

> Note: Response examples show the final enriched output sent to the frontend. Claude's raw response contains only `config` per widget; `WidgetEnricher` enriches it to the full `data` shape shown here.

```json
{
  "investor": {
    "id": "abc12345",
    "name": "Demo User",
    "age": 28,
    "experienceMonths": 8,
    "personality": "Anxious beginner - panics when she sees red",
    "checkFrequency": "Several times a day",
    "portfolio": {
      "totalValueEur": 4830,
      "holdings": [
        {
          "name": "ASML",
          "description": "Chip machine monopoly",
          "holdingType": "Stock",
          "valueEur": 1490,
          "returnPct": 9.6,
          "allocationPct": 30.8
        },
        {
          "name": "World ETF",
          "description": "~1,500 global companies",
          "holdingType": "ETF",
          "valueEur": 1292,
          "returnPct": 4.4,
          "allocationPct": 26.7
        },
        {
          "name": "AB InBev",
          "description": "World's biggest brewer (Jupiler, Stella)",
          "holdingType": "Stock",
          "valueEur": 438,
          "returnPct": -5.8,
          "allocationPct": 9.1
        },
        {
          "name": "Euro Stoxx 50 ETF",
          "description": "50 biggest European companies",
          "holdingType": "ETF",
          "valueEur": 664,
          "returnPct": 6.3,
          "allocationPct": 13.7
        },
        {
          "name": "KBC Group",
          "description": "Belgian bank",
          "holdingType": "Stock",
          "valueEur": 359,
          "returnPct": 5.0,
          "allocationPct": 7.4
        },
        {
          "name": "Cash",
          "description": "",
          "holdingType": "Cash",
          "valueEur": 587,
          "returnPct": null,
          "allocationPct": 12.2
        }
      ]
    }
  },
  "scenario": {
    "key": "crash_morning",
    "name": "Morning After a Crash",
    "signals": {
      "sessionCount30d": 12,
      "daysSinceLastSession": 0,
      "sessionNote": "2nd session today",
      "portfolioChange7dPct": -6.2,
      "portfolioChangeSinceLastVisitPct": null,
      "dividendsReceivedSinceLastVisitEur": null,
      "pendingActions": null,
      "marketVolatility": "High",
      "timeOfDay": "07:42",
      "timeNote": "unusually early",
      "lastAction": null,
      "searchQuery": null,
      "emotionalStateEstimate": "Anxious / checking compulsively"
    }
  },
  "brain": {
    "narrative": "Markets had a rough night. Your portfolio dropped, but you haven't lost anything you didn't have eight months ago. Here's the full picture.",
    "tone": "reassuring",
    "density": "low",
    "widgets": [
      {
        "type": "explanation_card",
        "data": {
          "title": "This is not unusual",
          "body": "Drops of this size have happened 12 times in the last 20 years. Markets recovered within 4 months on average. Your diversified portfolio is built to handle this.",
          "icon": "reassure",
          "category": "reassurance"
        }
      },
      {
        "type": "portfolio_overview",
        "data": {
          "totalValueEur": 4830,
          "totalReturnPct": 4.2,
          "holdings": [
            {
              "name": "ASML",
              "holdingType": "Stock",
              "valueEur": 1490,
              "returnPct": 9.6,
              "allocationPct": 30.8
            },
            {
              "name": "World ETF",
              "holdingType": "ETF",
              "valueEur": 1292,
              "returnPct": 4.4,
              "allocationPct": 26.7
            },
            {
              "name": "AB InBev",
              "holdingType": "Stock",
              "valueEur": 438,
              "returnPct": -5.8,
              "allocationPct": 9.1
            },
            {
              "name": "Euro Stoxx 50 ETF",
              "holdingType": "ETF",
              "valueEur": 664,
              "returnPct": 6.3,
              "allocationPct": 13.7
            },
            {
              "name": "KBC Group",
              "holdingType": "Stock",
              "valueEur": 359,
              "returnPct": 5.0,
              "allocationPct": 7.4
            },
            {
              "name": "Cash",
              "holdingType": "Cash",
              "valueEur": 587,
              "returnPct": null,
              "allocationPct": 12.2
            }
          ],
          "highlightHolding": null,
          "showAllocationChart": true,
          "simplifiedView": true
        }
      },
      {
        "type": "historical_chart",
        "data": {
          "title": "Your portfolio this week",
          "subject": "portfolio",
          "timeRange": "7d",
          "changePct": -6.2,
          "startValueEur": 5149,
          "endValueEur": 4830,
          "contextNote": "This week was rough, but zoom out — you're still up overall since you started investing.",
          "dataPoints": [
            { "date": "2026-03-04", "valueEur": 5149 },
            { "date": "2026-03-05", "valueEur": 5102 },
            { "date": "2026-03-06", "valueEur": 5030 },
            { "date": "2026-03-07", "valueEur": 4920 },
            { "date": "2026-03-08", "valueEur": 4870 },
            { "date": "2026-03-09", "valueEur": 4845 },
            { "date": "2026-03-10", "valueEur": 4830 }
          ]
        }
      }
    ],
    "suggestedActions": [
      { "label": "What happened?", "message": "Why did markets drop? What's going on?" },
      { "label": "Am I okay long-term?", "message": "Am I okay if I look at the bigger picture?" },
      { "label": "Show the bigger picture", "message": "Show me my portfolio over a longer time period" }
    ]
  }
}
```

**Error responses:**

| Code | When | Body |
|------|------|------|
| 404 | Invalid investorId | `{ "error": "InvestorNotFound", "message": "..." }` |
| 404 | Invalid scenario | `{ "error": "ScenarioNotFound", "message": "...", "validScenarios": ["calm_tuesday","crash_morning"] }` |
| 502 | Claude API fails | `{ "error": "BrainError", "message": "The brain could not process this request." }` |
| 500 | Unexpected error | `{ "error": "InternalError", "message": "An unexpected error occurred." }` |

---

### POST /api/brain/interact/{investorId}

User typed a message. Brain reshapes the UI based on their intent + current context.

**Parameters:**

| Param | Location | Type | Required | Description |
|-------|----------|------|----------|-------------|
| `investorId` | path | string | yes | Account ID (from account creation) |

**Request body:**

```json
{
  "message": "What is ASML?",
  "scenario": "calm_tuesday",
  "conversationHistory": [
    {
      "message": "How am I doing?",
      "narrativeSummary": "Portfolio overview showing +4.2% overall",
      "widgetTypes": ["portfolio_overview"]
    }
  ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | yes | The investor's typed message |
| `scenario` | string | no | Current scenario key. Defaults to first for that investor. |
| `conversationHistory` | ConversationEntry[] | no | Previous interactions this session (max 10). Frontend maintains this. |

**ConversationEntry:**

| Field | Type | Description |
|-------|------|-------------|
| `message` | string | What the investor typed |
| `narrativeSummary` | string | Short summary of the brain's previous narrative response |
| `widgetTypes` | string[] | Widget types from the brain's previous response |

**Response 200:** Same shape as dashboard response.

```json
{
  "investor": { ... },
  "scenario": { ... },
  "brain": {
    "narrative": "ASML makes the machines that produce computer chips. You own €1,490 worth — your biggest single holding.",
    "tone": "neutral",
    "density": "medium",
    "widgets": [
      {
        "type": "stock_card",
        "data": {
          "name": "ASML",
          "description": "Chip machine monopoly",
          "holdingType": "Stock",
          "priceEur": 745.20,
          "changeTodayPct": 1.2,
          "returnPct": 9.6,
          "valueEur": 1490,
          "detailLevel": "standard",
          "metrics": null,
          "scores": null,
          "analystConsensus": null
        }
      },
      {
        "type": "explanation_card",
        "data": {
          "title": "Why ASML matters",
          "body": "ASML has a near-monopoly on the machines that chipmakers need. Every major chip company — TSMC, Samsung, Intel — depends on them. That's why it's one of Europe's most valuable companies.",
          "icon": "learn",
          "category": "education"
        }
      },
      {
        "type": "portfolio_overview",
        "data": {
          "totalValueEur": 4830,
          "totalReturnPct": 4.2,
          "holdings": [ "..." ],
          "highlightHolding": "ASML",
          "showAllocationChart": true,
          "simplifiedView": false
        }
      }
    ],
    "suggestedActions": [
      { "label": "How does ASML make money?", "message": "How does ASML make money? Tell me more about their business." },
      { "label": "Compare ASML to my ETFs", "message": "Compare ASML to my World ETF" },
      { "label": "Back to overview", "message": "Show me my dashboard overview" }
    ]
  }
}
```

**Intent handling:**

| User says | Intent | Key widgets | Notes |
|-----------|--------|-------------|-------|
| "What is ASML?" | RESEARCH | stock_card + explanation_card | Detail level matches experience |
| "How am I doing?" | PORTFOLIO | portfolio_overview | Highlight best/worst performer |
| "Why did markets drop?" | CONTEXT | volatility_gauge + explanation_card | Factual, no future predictions |
| "Compare my ETFs" | COMPARE | comparison_view | Neutral comparison note |
| "What is P/E ratio?" | LEARN | explanation_card | Language matches experience |
| "Buy 5 shares of ASML" | ORDER | stock_card + order_confirmation | + explanation_card + historical_chart if panicking |
| "Should I buy ASML?" | ADVICE_REQUEST | stock_card + portfolio_overview | Give data, not direction |
| "Catch me up" | CATCHUP | portfolio_overview + explanation_card + historical_chart | Welcoming tone |
| "Hello" | UNCLEAR | Default dashboard widgets | Friendly acknowledgment |

**Panic sell example — during crash, user says "Sell everything":**

```json
{
  "brain": {
    "narrative": "Before you decide, here's some context on what's happening and what it means for your portfolio.",
    "tone": "reassuring",
    "density": "medium",
    "widgets": [
      {
        "type": "explanation_card",
        "data": {
          "title": "Perspective on today's drop",
          "body": "Markets dropped 6.2% this week. Drops like this have happened 12 times in the last 20 years. In every case, markets recovered within 3-8 months. Selling now would lock in these losses.",
          "icon": "reassure",
          "category": "reassurance"
        }
      },
      {
        "type": "historical_chart",
        "data": {
          "title": "Your portfolio over time",
          "subject": "portfolio",
          "timeRange": "1y",
          "changePct": 4.2,
          "startValueEur": 4636,
          "endValueEur": 4830,
          "contextNote": "Even after this week's drop, you're still up 4.2% overall.",
          "dataPoints": [ "..." ]
        }
      },
      {
        "type": "portfolio_overview",
        "data": {
          "totalValueEur": 4830,
          "totalReturnPct": 4.2,
          "holdings": [ "..." ],
          "highlightHolding": null,
          "showAllocationChart": true,
          "simplifiedView": true
        }
      }
    ],
    "suggestedActions": [
      { "label": "Show me the bigger picture", "message": "Show me my portfolio over a longer time period" },
      { "label": "I still want to sell", "message": "I want to sell everything" },
      { "label": "Back to overview", "message": "Show me my dashboard overview" }
    ]
  }
}
```

**Advice redirect example — user asks "Should I buy more ASML?":**

```json
{
  "brain": {
    "narrative": "Here's what might help you think about ASML and how it fits in your portfolio.",
    "tone": "neutral",
    "density": "medium",
    "widgets": [
      {
        "type": "stock_card",
        "data": {
          "name": "ASML",
          "description": "Chip machine monopoly",
          "holdingType": "Stock",
          "priceEur": 745.20,
          "changeTodayPct": 1.2,
          "returnPct": 9.6,
          "valueEur": 1490,
          "detailLevel": "standard",
          "metrics": null,
          "scores": null,
          "analystConsensus": null
        }
      },
      {
        "type": "portfolio_overview",
        "data": {
          "totalValueEur": 4830,
          "totalReturnPct": 4.2,
          "holdings": [ "..." ],
          "highlightHolding": "ASML",
          "showAllocationChart": true,
          "simplifiedView": false
        }
      },
      {
        "type": "explanation_card",
        "data": {
          "title": "ASML in your portfolio",
          "body": "ASML is currently 30.8% of your portfolio — your biggest single position. Adding more would increase your concentration in one company. Diversification means spreading risk across different holdings.",
          "icon": "info",
          "category": "education"
        }
      }
    ],
    "suggestedActions": [
      { "label": "Tell me more about ASML", "message": "Tell me more about ASML's business and financials" },
      { "label": "Show my allocation", "message": "Show me my portfolio allocation breakdown" },
      { "label": "Back to overview", "message": "Show me my dashboard overview" }
    ]
  }
}
```

**Error responses:** Same as dashboard endpoint (404, 502, 500).

---

### GET /api/investors

List all investors with summary info.

**Response 200:**

```json
[
  {
    "id": "abc12345",
    "name": "Demo User",
    "label": "The Anxious Beginner",
    "age": 28,
    "experienceMonths": 8,
    "portfolioTotalEur": 4830,
    "scenarios": ["calm_tuesday", "crash_morning"]
  },
  {
    "id": "def67890",
    "name": "Another User",
    "label": "The Hands-Off Veteran",
    "age": 52,
    "experienceMonths": 144,
    "portfolioTotalEur": 87420,
    "scenarios": ["returning", "research"]
  }
]
```

---

### GET /api/investors/{investorId}

Full investor detail with portfolio.

**Response 200:** Returns the `investor` object from the dashboard response (same shape as `DashboardResponse.investor`).

**Response 404:** Same as dashboard 404.

---

### GET /api/scenarios

List all scenarios.

**Response 200:**

```json
[
  {
    "key": "calm_tuesday",
    "investorId": "abc12345",
    "name": "Calm Tuesday Evening",
    "description": "Low volatility, +0.8% week, evening check-in"
  },
  {
    "key": "crash_morning",
    "investorId": "abc12345",
    "name": "Morning After a Crash",
    "description": "High volatility, -6.2% week, early morning panic check"
  },
  {
    "key": "returning",
    "investorId": "def67890",
    "name": "Returning After 3 Months",
    "description": "94 days absent, portfolio grew, dividends received"
  },
  {
    "key": "research",
    "investorId": "def67890",
    "name": "Research Mode",
    "description": "Active research session, searching semiconductors"
  }
]
```

---

### GET /api/stocks/{ticker}

Stock detail. Currently only ASML.

**Response 200:**

```json
{
  "ticker": "ASML",
  "name": "ASML",
  "description": "Makes the machines that make computer chips. Near-monopoly. Every major chipmaker depends on them.",
  "priceEur": 745.20,
  "changeTodayPct": 1.2,
  "marketCapEurB": 301,
  "peRatio": 35.2,
  "sectorAvgPe": 28,
  "dividendYieldPct": 0.7,
  "analystConsensus": {
    "consensus": "Strong Buy",
    "buy": 18,
    "hold": 4,
    "sell": 1,
    "targetEur": 820
  },
  "scores": {
    "growth": 5,
    "health": 4,
    "stability": 3
  }
}
```

**Response 404:** `{ "error": "StockNotFound", "message": "Stock 'XYZ' not found.", "availableStocks": ["ASML"] }`

---

### GET /health

```json
{
  "status": "healthy",
  "timestamp": "2026-03-11T08:30:00Z"
}
```

---

## Widget Specifications

Array order in the response = render order on frontend. No separate priority field.

### portfolio_overview

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `totalValueEur` | decimal | no | Portfolio total value |
| `totalReturnPct` | decimal | no | Overall return percentage |
| `holdings` | HoldingDto[] | no | All holdings with allocation |
| `highlightHolding` | string | yes | Name of holding to highlight (biggest mover, concern) |
| `showAllocationChart` | bool | no | Whether to render pie/donut chart |
| `simplifiedView` | bool | no | `true` = name + return only (for anxious states) |

### stock_card

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `name` | string | no | Stock/ETF name |
| `description` | string | no | One-line description |
| `holdingType` | enum | no | `Stock`, `ETF`, `Cash` |
| `priceEur` | decimal | yes | Current price (only ASML has this) |
| `changeTodayPct` | decimal | yes | Today's change (only ASML) |
| `returnPct` | decimal | no | Investor's return on this holding |
| `valueEur` | decimal | no | Current value in portfolio |
| `detailLevel` | enum | no | `Simple`, `Standard`, `Full` |
| `metrics` | object | yes | PE, dividend yield, market cap. Only when `Full`. |
| `scores` | object | yes | Growth, health, stability (1-5). Only when `Full`. |
| `analystConsensus` | object | yes | Buy/hold/sell/target. Only when `Full`. |

**Detail level rules:**
- `Simple`: name + holdingType + returnPct + valueEur
- `Standard`: + description + priceEur + changeTodayPct
- `Full`: + metrics + scores + analystConsensus (ASML only)

### historical_chart

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `title` | string | no | Chart heading |
| `subject` | string | no | `"portfolio"` or holding name |
| `timeRange` | enum | no | `7d`, `1m`, `3m`, `1y`, `5y` |
| `changePct` | decimal | no | Net change over period |
| `startValueEur` | decimal | no | Value at period start |
| `endValueEur` | decimal | no | Value at period end |
| `contextNote` | string | yes | Perspective text below chart |
| `dataPoints` | DataPoint[] | no | Synthetic time series for chart |

**DataPoint:** `{ "date": "2026-03-04", "valueEur": 5149 }`

Claude returns `changePct`, `timeRange`, `startValueEur`, `endValueEur`, `contextNote`. The backend generates `dataPoints` via `ChartDataGenerator`:
1. Number of points from timeRange (7d=7, 1m=30, 3m=90, 1y=252, 5y=1260)
2. Linear interpolation from start to end
3. Gaussian noise (±0.5% daily) for realism
4. First and last points pinned to exact values

### volatility_gauge

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `level` | enum | no | `Low`, `Medium`, `High` |
| `label` | string | no | Short one-line label |
| `detail` | string | no | 1-2 sentences of context |
| `historicalContext` | string | yes | Deeper perspective for crash scenarios |

### comparison_view

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `title` | string | no | Comparison heading |
| `items` | ComparisonItem[2] | no | Exactly 2 holdings |
| `comparisonNote` | string | no | Neutral, educational — never directive |

**ComparisonItem:** `{ name, holdingType, description, valueEur, returnPct, allocationPct }`

### explanation_card

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `title` | string | no | Card heading |
| `body` | string | no | 2-4 sentences, plain language |
| `icon` | enum | no | `Reassure`, `Learn`, `Alert`, `Info`, `Celebrate` |
| `category` | enum | no | `Education`, `Reassurance`, `Catchup`, `Insight` |

### order_confirmation

Only returned when the investor explicitly requests a buy or sell. Never shown proactively.

| Field | Type | Nullable | Description |
|-------|------|----------|-------------|
| `action` | enum | no | `Buy` or `Sell` |
| `holding` | string | no | Name of the stock/ETF |
| `holdingType` | enum | no | `Stock`, `ETF` |
| `quantity` | int | yes | Number of shares. Null if selling all. |
| `quantityLabel` | string | yes | Human-readable label when quantity is null (e.g. "All shares") |
| `priceEur` | decimal | yes | Current price per share (buy orders) |
| `totalEur` | decimal | yes | Total order cost (buy orders) |
| `currentValueEur` | decimal | yes | Current position value (sell orders) |
| `returnPct` | decimal | yes | Current return on this holding (sell orders) |
| `availableCashEur` | decimal | yes | Investor's available cash (buy orders) |
| `insufficientFunds` | bool | no | `true` if totalEur > availableCashEur |
| `summary` | string | no | One-line plain-language order description |
| `warning` | string | yes | Factual warning (insufficient funds, selling at a loss). Never used to discourage trading. |

**Panic/crash conditions:** Show `explanation_card` (reassurance) + `historical_chart` (perspective) BEFORE `order_confirmation` in the widget array. The investor may still proceed — that's their right.

---

## Enums

```csharp
public enum Tone
{
    Reassuring,
    Celebratory,
    Neutral,
    Focused,
    Welcoming
}

public enum Density
{
    Low,
    Medium,
    High
}

public enum WidgetType
{
    PortfolioOverview,
    StockCard,
    HistoricalChart,
    VolatilityGauge,
    ComparisonView,
    ExplanationCard,
    OrderConfirmation
}

public enum HoldingType
{
    Stock,
    ETF,
    Cash
}

public enum MarketVolatility
{
    Low,
    Medium,
    High
}

public enum EmotionalState
{
    Calm,
    Curious,
    Anxious,
    Focused,
    Detached,
    Compulsive
}

public enum ExplanationIcon
{
    Reassure,
    Learn,
    Alert,
    Info,
    Celebrate
}

public enum ExplanationCategory
{
    Education,
    Reassurance,
    Catchup,
    Insight
}

public enum StockDetailLevel
{
    Simple,
    Standard,
    Full
}

public enum OrderAction
{
    Buy,
    Sell
}
```

---

## Tone → Frontend Behavior

| Tone | Color palette | Typography | Spacing |
|------|--------------|------------|---------|
| `Reassuring` | Warm muted (soft blues, greens) | Larger body text, softer weight | Generous whitespace |
| `Celebratory` | Warm accents (gold, green highlights) | Standard with accent highlights | Standard |
| `Neutral` | Default palette | Standard | Standard |
| `Focused` | High contrast, minimal color | Compact, denser type | Tight spacing |
| `Welcoming` | Warm, inviting | Friendly, slightly larger | Generous |

## Density → Frontend Layout

| Density | Grid | Max widgets | Widget sizing |
|---------|------|-------------|---------------|
| `Low` | Single column | 2-3 | Large, full-width cards |
| `Medium` | Mixed (some 2-col) | 3-4 | Medium cards |
| `High` | 2-col grid | 4-6 | Compact cards |

---

## Data Flow

### Dashboard Flow (GET /api/brain/dashboard/{investorId})

```
HTTP GET
  │
  ▼
BrainController.GetDashboard(investorId, scenario)
  │
  ▼
GetDashboardQuery → MediatR → GetDashboardHandler
  │
  ├──▶ IInvestorRepository.GetByIdAsync(investorId)
  │      └── Returns Investor entity with Portfolio + Holdings
  │
  ├──▶ IScenarioRepository.GetByKeyAsync(investorId, scenarioKey)
  │      └── Returns Scenario entity with ContextSignals
  │
  ├──▶ IPromptBuilder.BuildDashboardPromptAsync(investor, scenario)
  │      ├── Reads prompts/system/01-role.md
  │      ├── Reads prompts/system/02-rules.md
  │      ├── Reads prompts/system/03-tone-guide.md
  │      ├── Reads prompts/system/04-widget-catalog.md
  │      ├── Reads prompts/system/05-output-format.md
  │      ├── Fills prompts/context/investor-template.md with investor data
  │      ├── Fills prompts/context/signals-template.md with signal data
  │      └── Returns (systemPrompt, userMessage)
  │
  ├──▶ IClaudeClient.GetBrainResponseAsync(systemPrompt, userMessage)
  │      ├── Calls Claude API (claude-sonnet-4-20250514)
  │      ├── Max tokens: 2000, Temperature: 0.7
  │      ├── Parses JSON response into BrainOutput (config-based widgets)
  │      └── On failure: throws ClaudeApiException
  │
  ├──▶ WidgetEnricher.Enrich(brainOutput.Widgets, investor, scenario)
  │      ├── For each widget: maps config → full data
  │      ├── Looks up holdings in investor.portfolio
  │      ├── Looks up prices in stock_prices (via StockRepository)
  │      ├── Generates dataPoints for historical_chart (via ChartDataGenerator)
  │      ├── Computes order totals/warnings for order_confirmation
  │      ├── Validates holding references exist — drops invalid widgets
  │      └── Returns enriched widgets with full data
  │
  ├──▶ ChipResolver.Resolve(brainOutput.SuggestedActions)
  │      ├── Maps chip IDs to SuggestedActionDto (id, label, message)
  │      ├── Resolves {holding} templates in about_holding chips
  │      └── Returns 3 SuggestedActionDto items
  │
  └──▶ Assembles DashboardResponse
         ├── investor → InvestorDto (via converter)
         ├── scenario → ScenarioDto (via converter)
         └── brain → enriched BrainOutput (widgets with data, resolved chips)
  │
  ▼
HTTP 200 OK → DashboardResponse JSON
```

### Interaction Flow (POST /api/brain/interact/{investorId})

```
HTTP POST { message, scenario, conversationHistory }
  │
  ▼
BrainController.Interact(investorId, request)
  │
  ▼
ProcessInteractionCommand → MediatR → ProcessInteractionHandler
  │
  ├──▶ IInvestorRepository.GetByIdAsync(investorId)
  │      └── Returns Investor entity with Portfolio + Holdings
  │
  ├──▶ IScenarioRepository.GetByKeyAsync(investorId, scenarioKey)
  │      └── Returns Scenario entity with ContextSignals
  │
  ├──▶ IPromptBuilder.BuildInteractionPromptAsync(investor, scenario, message, conversationHistory)
  │      ├── Reads prompts/system/01-role.md
  │      ├── Reads prompts/system/02-rules.md
  │      ├── Reads prompts/system/03-tone-guide.md
  │      ├── Reads prompts/system/04-widget-catalog.md
  │      ├── Reads prompts/system/05-output-format.md
  │      ├── Reads prompts/system/06-interaction-rules.md      ← NEW
  │      ├── Fills prompts/context/investor-template.md with investor data
  │      ├── Fills prompts/context/signals-template.md with signal data
  │      ├── Fills prompts/context/interaction-template.md     ← NEW
  │      │     with user message + conversation history
  │      └── Returns (systemPrompt, userMessage)
  │
  ├──▶ IClaudeClient.GetBrainResponseAsync(systemPrompt, userMessage)
  │      └── Same as dashboard flow (returns config-based widgets)
  │
  ├──▶ WidgetEnricher.Enrich(brainOutput.Widgets, investor, scenario)
  │      └── Same as dashboard flow
  │
  ├──▶ ChipResolver.Resolve(brainOutput.SuggestedActions)
  │      └── Same as dashboard flow
  │
  └──▶ Assembles DashboardResponse
         └── Same shape as dashboard — investor + scenario + enriched brain
  │
  ▼
HTTP 200 OK → DashboardResponse JSON
```

### Key Difference Between Flows

The two flows share most of the pipeline. The only differences:
1. **Interaction** includes `06-interaction-rules.md` in the system prompt
2. **Interaction** fills `interaction-template.md` with the user's message + conversation history
3. **Dashboard** uses `GetDashboardQuery` (query). **Interaction** uses `ProcessInteractionCommand` (command — it has a side effect: the user's message).

Both return the exact same `DashboardResponse` shape. The frontend doesn't distinguish between them — it just renders whatever the brain sends.

---

## Widget Enrichment

Claude returns widget **instructions** (type + config). The backend enriches them into full data using the investor's portfolio and data file. The frontend receives fully populated widgets — it never sees the config-only format.

### Architecture

```
Claude returns:                    WidgetEnricher produces:
─────────────                      ────────────────────────
{ type: "stock_card",              { type: "stock_card",
  config: {                          data: {
    holding: "ASML",                   name: "ASML",
    detail_level: "standard"           description: "Chip machine monopoly",
  }                                    holdingType: "Stock",
}                                      priceEur: 745.20,
                                       changeTodayPct: 1.2,
                                       returnPct: 9.6,
                                       valueEur: 1490,
                                       detailLevel: "standard",
                                       metrics: null,
                                       scores: null,
                                       analystConsensus: null
                                     }
                                   }
```

### Per-Widget Enrichment

**portfolio_overview**
- Config from Claude: `{ highlightHolding?, showAllocationChart, simplifiedView }`
- Backend adds: `totalValueEur`, `totalReturnPct`, `holdings[]` (all from investor data + computed `allocationPct`)

**stock_card**
- Config from Claude: `{ holding, detailLevel }`
- Backend looks up holding in portfolio + `stock_prices`
- Adds: `name`, `description`, `holdingType`, `valueEur`, `returnPct`, `priceEur`, `changeTodayPct`
- If `detailLevel == "full"` and holding is ASML: also adds `metrics`, `scores`, `analystConsensus`
- If holding not found in portfolio: widget is dropped, replaced with `explanation_card` saying data unavailable

**historical_chart**
- Config from Claude: `{ title, subject, timeRange, contextNote? }`
- Backend derives `changePct` from context signals (e.g. `portfolio_change_7d_pct` for 7d)
- Computes `endValueEur` = portfolio total, `startValueEur` = endValue / (1 + changePct/100)
- Generates `dataPoints` via `ChartDataGenerator`

**volatility_gauge**
- Config from Claude: `{ label, detail, historicalContext? }`
- Backend fills `level` from `context_signals.market_volatility`
- Defaults to `"Low"` if signal is null

**comparison_view**
- Config from Claude: `{ title, holdings[2], comparisonNote }`
- Backend looks up each holding in portfolio
- Fills: `name`, `holdingType`, `description`, `valueEur`, `returnPct`, `allocationPct` for each

**explanation_card**
- Config from Claude: `{ title, body, icon, category }`
- **No enrichment.** Passed through unchanged. This is the only widget where Claude provides all content.

**order_confirmation**
- Config from Claude: `{ action, holding, quantity?, quantityLabel? }`
- Backend computes everything:
  - `priceEur` from `stock_prices[holding]`
  - Buy: `totalEur` = quantity * priceEur, `availableCashEur` from Cash holding, `insufficientFunds` = totalEur > availableCashEur
  - Sell: `currentValueEur` and `returnPct` from portfolio holding
  - `summary`: generated string describing the order
  - `warning`: generated if insufficient funds (buy) or selling at a loss (sell). Never used to discourage trading.

### Validation

Before enrichment, `WidgetEnricher` validates:
- All widget types exist in `WidgetType` enum
- All `holding` references exist in the investor's portfolio
- Invalid widgets are dropped (not sent to frontend)
- At least one widget must survive validation — if all are dropped, fall back to `portfolio_overview`

### Chip Resolution

Claude returns chip IDs (e.g. `["about_holding:ASML", "compare_etfs", "back_to_overview"]`). `ChipResolver` maps each ID to a `SuggestedActionDto`:

```csharp
public record SuggestedActionDto(string Id, string Label, string Message);
```

**Chip Pool (19 chips):**

| ID | Label | Message |
|----|-------|---------|
| `portfolio_status` | How am I doing? | How is my portfolio doing? |
| `show_allocation` | Show my allocation | Show me my portfolio allocation breakdown |
| `show_returns` | Show my returns | Show me how my investments have performed |
| `about_holding:{holding}` | Tell me about {holding} | Tell me about {holding} |
| `compare_etfs` | Compare my ETFs | Compare my ETFs side by side |
| `what_are_etfs` | What are ETFs? | What are ETFs and how do they work? |
| `what_are_dividends` | What are dividends? | What are dividends and how do they work? |
| `what_is_pe` | What is P/E ratio? | What is P/E ratio and what does it mean? |
| `how_diversification` | How does diversification work? | How does diversification work? |
| `stock_vs_etf` | Stock vs ETF? | What's the difference between a stock and an ETF? |
| `what_happened` | What happened? | Why did markets drop? What's going on? |
| `ok_long_term` | Am I okay long-term? | Am I okay if I look at the bigger picture? |
| `bigger_picture` | Show the bigger picture | Show me my portfolio over a longer time period |
| `catch_me_up` | Catch me up | Catch me up on what happened while I was away |
| `what_did_i_miss` | What did I miss? | What did I miss since my last visit? |
| `show_dividends` | Show my dividends | Show me my dividend income |
| `anything_new` | Anything new? | Is there anything new or notable in my portfolio? |
| `back_to_overview` | Back to overview | Show me my dashboard overview |
| `still_want_to_sell` | I still want to sell | I want to sell everything |

For `about_holding:{holding}`, `ChipResolver` replaces `{holding}` in both label and message with the actual holding name.

### Data sources summary

| Source | What |
|--------|------|
| **PDF (verbatim)** | Investor profiles, portfolios (name, type, value, return), context signals, ASML full detail |
| **Mocked by us** | Prices + daily change for all non-ASML holdings, total portfolio return |
| **Computed at runtime** | `allocationPct`, `dataPoints` for charts, order totals/warnings, chip label/message resolution |
| **Claude decides** | Narrative, tone, density, widget selection + config, chip selection |
| **Claude writes** | `explanation_card` content, `historical_chart` contextNote, `volatility_gauge` label/detail, narrative text |

---

## Infrastructure Details

### JsonDataLoader
- Reads `data/investsuite_case_data.json` at startup
- Parses into strongly-typed entity objects
- Registered as singleton — data loaded once, served from memory
- Computes `allocationPct` for each holding on load
- Merges `stock_prices` into holdings (attaches `priceEur`, `changeTodayPct` to each holding)
- Loads `total_return_pct` per investor

### InvestorRepository
- Depends on `JsonDataLoader`
- `GetByIdAsync(string id)` → `Investor?`
- `GetAllAsync()` → `IReadOnlyList<Investor>`

### ScenarioRepository
- Depends on `JsonDataLoader`
- `GetByKeyAsync(string investorId, string scenarioKey)` → `Scenario?`
- `GetAllAsync()` → `IReadOnlyList<Scenario>`
- `GetByInvestorIdAsync(string investorId)` → `IReadOnlyList<Scenario>`

### StockRepository
- Depends on `JsonDataLoader`
- `GetByNameAsync(string name)` → `StockDetail?` (looks up `stock_prices` by holding name)
- `GetAllAsync()` → `IReadOnlyList<StockDetail>`
- Only ASML returns full detail (metrics, scores, analyst). Others return price + change only.

### PromptBuilder
- Reads markdown files from `prompts/` directory on construction (cached)
- `BuildDashboardPromptAsync(Investor, Scenario)` → `(string systemPrompt, string userMessage)`
- `BuildInteractionPromptAsync(Investor, Scenario, string message, ConversationEntry[]?)` → `(string systemPrompt, string userMessage)`
- Dashboard system prompt: `01-role` + `02-rules` + `03-tone-guide` + `04-widget-catalog` + `05-output-format`
- Interaction system prompt: same as dashboard + `06-interaction-rules`
- Dashboard user message: `investor-template` + `signals-template`
- Interaction user message: `investor-template` + `signals-template` + `interaction-template`
- Template filling: replaces `{{placeholder}}` tokens with actual values
- Handles `{{#each holdings}}` loop for portfolio table
- Handles `{{#if conversationHistory}}` conditional block
- Handles `{{#each conversationHistory}}` loop for conversation entries

### ClaudeClient
- Wraps Anthropic C# SDK or HttpClient
- Configured via `ClaudeOptions` (model, maxTokens, temperature, apiKey)
- `GetBrainResponseAsync(string systemPrompt, string userMessage)` → `BrainOutput`
- Parses Claude's JSON response with `System.Text.Json`
- `BrainOutput` contains: narrative, tone, density, widgets[] (config-based), suggestedActions (chip IDs)
- Validates all widget types exist in enum
- Does NOT validate holding names or enrich data — that's `WidgetEnricher`'s job
- Throws `ClaudeApiException` on any failure

### ChartDataGenerator
- `GenerateDataPoints(decimal startValue, decimal endValue, string timeRange)` → `DataPoint[]`
- Static/pure — no dependencies, fully testable
- Algorithm: linear interpolation + gaussian noise (±0.5% daily)
- Pins first/last points to exact values

### BrainService
- Implements `IBrainService`
- Orchestrates: get investor → get scenario → build prompt → call Claude → generate chart data → return
- `GetDashboardAsync(string investorId, string? scenarioKey)` → `DashboardResponse`
- `ProcessInteractionAsync(string investorId, InteractionRequest request)` → `DashboardResponse`

### WidgetEnricher
- Takes Claude's config-based widgets + investor data + scenario → produces enriched widgets with full data
- Validates holding references exist in portfolio
- Drops invalid widgets, falls back to `portfolio_overview` if all are dropped
- Calls `ChartDataGenerator` internally for `historical_chart` widgets
- Depends on: `IStockRepository`, `ChartDataGenerator`

### ChipResolver
- Takes Claude's chip IDs (string[]) → produces `SuggestedActionDto[]`
- Resolves `about_holding:{holding}` templates
- Validates chip IDs against the fixed pool of 19
- Invalid chip IDs are replaced with `portfolio_status`

### DependencyInjection.cs
```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<JsonDataLoader>();
        services.AddScoped<IInvestorRepository, InvestorRepository>();
        services.AddScoped<IScenarioRepository, ScenarioRepository>();
        services.AddScoped<IStockRepository, StockRepository>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IClaudeClient, ClaudeClient>();
        services.AddScoped<IBrainService, BrainService>();
        services.AddSingleton<ChartDataGenerator>();
        services.AddScoped<WidgetEnricher>();
        services.AddSingleton<ChipResolver>();
        services.Configure<ClaudeOptions>(configuration.GetSection("Claude"));
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(GetDashboardHandler).Assembly));
        return services;
    }
}
```

### Program.cs Outline
```csharp
var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5144";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.Run();
```

### JSON Serialization Rules
- `System.Text.Json` with `camelCase` property naming
- Enums serialized as strings (via `JsonStringEnumConverter`)
- Null values omitted (`WhenWritingNull`)

### ExceptionMiddleware Mapping
| Exception | HTTP Code |
|-----------|-----------|
| `InvestorNotFoundException` | 404 |
| `ScenarioNotFoundException` | 404 |
| `ClaudeApiException` | 502 |
| Any other `Exception` | 500 |

### Claude API Configuration (appsettings.json)
```json
{
  "Claude": {
    "ApiKey": "",
    "Model": "claude-sonnet-4-20250514",
    "MaxTokens": 2000,
    "Temperature": 0.7
  }
}
```
ApiKey loaded from environment variable `CLAUDE_API_KEY` in production.

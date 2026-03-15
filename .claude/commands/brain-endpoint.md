Create a new Brain API endpoint for the adaptive investing interface.

The Brain is the backend decision engine that evaluates context signals and returns a widget stream.

## Input
- Endpoint name/purpose: $ARGUMENTS

## What to generate
1. A new ASP.NET Core controller action or minimal API endpoint
2. A MediatR command/query + handler (or direct service if simple)
3. Logic that reads context signals (investor profile, market state, behavior, time)
4. Returns a response with `narrative` (string) + `widgets` (array of widget configs)

## Response format
```json
{
  "narrative": "string — contextual text for the investor",
  "widgets": [
    { "type": "widget_type", "data": { ... }, "priority": 1 }
  ]
}
```

## Widget types available
- `portfolio_overview` — holdings summary, allocation breakdown
- `stock_card` — individual stock with key metrics
- `historical_chart` — price/performance timeline
- `volatility_gauge` — current market temperature
- `comparison_view` — side-by-side stock/ETF comparison
- `explanation_card` — educational content, reassurance, context

## Context signals to consider
- `session_count_30d`, `days_since_last_session`
- `portfolio_change_7d`, `portfolio_change_since_last_visit`
- `market_volatility` (Low/Medium/High)
- `time_of_day`, `emotional_state_estimate`
- `last_action`, `search_query`
- `dividends_received_since_last_visit`, `pending_actions`

## Rules (non-negotiable)
- No investment advice — inform, don't recommend
- No gamification
- No manipulation or artificial urgency
- Investor always stays in control

## Reference
- Data: `data/investsuite_case_data.json`
- Architecture: `docs/case-architecture.md`
- .NET patterns: `docs/standards/dotnet-practices.md`

Create a new frontend widget component for the adaptive investing interface.

The Face is the frontend that renders widgets dynamically based on what the Brain sends.

## Input
- Widget name/purpose: $ARGUMENTS

## What to generate
1. A React component in `src/components/widgets/` using Mantine + CSS Modules
2. TypeScript interface for the widget's data props
3. Registration in the widget renderer/registry so the Brain can invoke it by type string

## Widget contract
Each widget receives:
```typescript
interface WidgetProps {
  type: string;
  data: Record<string, unknown>;
  priority?: number;
}
```

## Design principles
- Adaptive density — widget should support compact/normal/expanded modes
- Tone-aware — support calm/urgent/neutral visual variants
- Responsive — works on mobile and desktop
- Animated transitions — components should smoothly appear/disappear
- No gamification visuals (no badges, streaks, points)
- No directive language ("you should buy/sell")

## Existing widget types for reference
- `portfolio_overview` — holdings summary, allocation pie/bar chart
- `stock_card` — stock name, price, change, key metrics
- `historical_chart` — Recharts line/area chart with time range
- `volatility_gauge` — visual meter (low/medium/high)
- `comparison_view` — two stocks/ETFs side by side
- `explanation_card` — text card with optional icon, educational tone

## Tech
- Mantine components (`Card`, `Text`, `Badge`, `Progress`, `RingProgress`, etc.)
- CSS Modules (camelCase class names)
- Recharts for any charts
- Function declarations (not arrow functions)
- Named interfaces for all props

## Reference
- Architecture: `docs/case-architecture.md`
- Frontend patterns: `docs/standards/architecture.md`
- Coding conventions: `docs/standards/coding-conventions.md`
- Mock data: `data/investsuite_case_data.json`

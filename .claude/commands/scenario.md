Implement or test a demo scenario for the adaptive investing interface.

## Input
- Scenario name or description: $ARGUMENTS

## Available scenarios (from the case)

### Calm Evening
- Low volatility, +0.8% week, 20:15, calm/curious
- Expected: gentle overview, learning opportunities, light density

### Morning After a Crash
- High volatility, -6.2% week, 07:42 (unusually early), 2nd session today
- Expected: reassurance first, historical context, reduced density, perspective before action

### Quiet Week (4 days of nothing)
- Flat markets, daily check-ins, nothing happening
- Expected: progressively different content each day, not repeating the same screen

### Returning After 3 Months
- 94 days absent, +4.2% growth, dividends received, expired orders
- Expected: catch-up narrative, "here's what happened while you were away"

### Research Mode
- 3rd session today, viewed 4 stocks in 8 mins, searching "european semiconductor"
- Expected: high density, research tools, stock comparisons, minimal narrative

## What to do
1. Create/update the context signal payload for this scenario
2. Ensure the Brain endpoint processes these signals and returns appropriate widgets
3. Verify the frontend renders the correct widget combination
4. Test the transition: same investor, different context → different experience

## Key proof point
The prototype must show the **same investor in 2 different contexts** getting a **different experience**. This is the core deliverable.

## Reference
- Scenarios doc: `docs/case-scenarios.md`
- Rules: `docs/case-rules.md`

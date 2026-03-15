## Output Format

Respond with ONLY valid JSON. No markdown. No explanation. No preamble. Just the JSON object.

```json
{
  "narrative": "string — 1-3 short sentences that set the emotional tone. This is the first thing the investor reads.",
  "tone": "reassuring | celebratory | neutral | focused | welcoming",
  "density": "low | medium | high",
  "widgets": [
    {
      "type": "widget_type_from_catalog",
      "config": { ... }
    }
  ],
  "suggestedActions": ["chip_id_1", "chip_id_2", "chip_id_3"]
}
```

### Rules for the response

**narrative:**
- 1-3 sentences maximum
- Exception: for `welcoming` tone (returning after long absence), up to 4 sentences allowed for catch-up storytelling
- Speaks directly to the investor ("your portfolio", "you", not "the investor")
- Matches the tone field
- Never contains investment advice
- For beginners: simple words, no jargon
- For veterans: can be more concise and technical

**tone:**
- Must be one of: `reassuring`, `celebratory`, `neutral`, `focused`, `welcoming`

**density:**
- `low` = 2-3 widgets max
- `medium` = 3-4 widgets
- `high` = 4-6 widgets
- The number of widgets in your response MUST match the density level

**widgets:**
- Ordered by priority (most important first)
- Each widget must use a type from the widget catalog
- Each widget's config must follow the exact schema from the catalog
- Available types: `portfolio_overview`, `stock_card`, `historical_chart`, `volatility_gauge`, `comparison_view`, `explanation_card`, `order_confirmation`
- Use only holding names that exist in the investor's portfolio — do not invent holdings
- Do not include financial data (prices, values, returns) — the backend fills these from real data
- `explanation_card` is the only widget where you provide all content (title, body, icon, category)
- `order_confirmation` is ONLY used when the investor explicitly requests a buy or sell

**suggestedActions:**
- Always exactly 3 chip IDs from the pool below
- These are contextual — choose chips that match the investor's state, scenario, and what was just shown
- For `about_holding`, append the holding name with a colon: `"about_holding:ASML"`
- After an interaction, the third chip must always be `"back_to_overview"`
- On initial dashboard load (no prior interaction), all 3 should be relevant to the investor's current context
- NEVER include `still_want_to_sell` unless you just showed perspective on a sell order
- Only include `show_dividends` for investors who have received dividends

**Chip Pool:**

| ID | Label shown to investor |
|----|------------------------|
| `portfolio_status` | How am I doing? |
| `show_allocation` | Show my allocation |
| `show_returns` | Show my returns |
| `about_holding:{holding}` | Tell me about {holding} |
| `compare_etfs` | Compare my ETFs |
| `what_are_etfs` | What are ETFs? |
| `what_are_dividends` | What are dividends? |
| `what_is_pe` | What is P/E ratio? |
| `how_diversification` | How does diversification work? |
| `stock_vs_etf` | Stock vs ETF? |
| `what_happened` | What happened? |
| `ok_long_term` | Am I okay long-term? |
| `bigger_picture` | Show the bigger picture |
| `catch_me_up` | Catch me up |
| `what_did_i_miss` | What did I miss? |
| `show_dividends` | Show my dividends |
| `anything_new` | Anything new? |
| `back_to_overview` | Back to overview |
| `still_want_to_sell` | I still want to sell |

**suggestedActions selection by context:**

Anxious beginner + crash:
- `["what_happened", "ok_long_term", "bigger_picture"]`

Calm beginner + normal:
- `["portfolio_status", "about_holding:ASML", "what_are_etfs"]`

Veteran returning:
- `["catch_me_up", "show_dividends", "what_did_i_miss"]`

Veteran research mode:
- `["about_holding:ASML", "compare_etfs", "show_allocation"]`

Flat markets + repeated check-ins:
- Day 1: `["portfolio_status", "about_holding:ASML", "what_are_etfs"]`
- Day 2: `["about_holding:World ETF", "stock_vs_etf", "show_allocation"]`
- Day 3: `["what_are_dividends", "how_diversification", "show_returns"]`
- Day 4: `["anything_new", "about_holding:KBC Group", "bigger_picture"]`

After any interaction:
- First two chips: relevant follow-ups to what was just shown
- Third chip: `"back_to_overview"`

After order with perspective (panic sell):
- `["bigger_picture", "still_want_to_sell", "back_to_overview"]`

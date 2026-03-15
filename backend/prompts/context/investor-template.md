## Investor Profile

**Name:** {{investor_name}}
**Age:** {{investor_age}}
**Experience:** {{experience_months}} months investing
**Personality:** {{personality}}
**Check frequency:** {{check_frequency}}

## Portfolio (total: €{{portfolio_total}})

| Holding | Type | Value | Return |
|---------|------|-------|--------|
{{#each holdings}}
| {{name}} | {{type}} | €{{value_eur}} | {{return_pct}}% |
{{/each}}

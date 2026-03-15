# Prompt Assembly

The brain's Claude prompt is built from modular pieces at runtime.

## Two Flows

### Dashboard (initial load)
System prompt: `01` + `02` + `03` + `04` + `05`
User message: `investor-template` + `signals-template`

### Interaction (user typed a message)
System prompt: `01` + `02` + `03` + `04` + `05` + `06`
User message: `investor-template` + `signals-template` + `interaction-template`

## System Prompt Files
Concatenated in order:
1. `system/01-role.md` — Identity and mission
2. `system/02-rules.md` — Non-negotiable constraints
3. `system/03-tone-guide.md` — How to map signals to tone, density, widget strategy
4. `system/04-widget-catalog.md` — Available widget types and their data schemas
5. `system/05-output-format.md` — Required JSON response structure
6. `system/06-interaction-rules.md` — Intent classification and response rules for user messages (interaction flow only)

## Context Templates (dynamic per request)
1. `context/investor-template.md` — Filled with investor profile + portfolio (both flows)
2. `context/signals-template.md` — Filled with current context signals (both flows)
3. `context/interaction-template.md` — Filled with user message + conversation history (interaction flow only)

## How to edit
- Change how Claude reacts to anxiety → edit `03-tone-guide.md`
- Add a new widget type → edit `04-widget-catalog.md`
- Change the response shape → edit `05-output-format.md`
- Change how Claude handles user messages → edit `06-interaction-rules.md`
- Change what investor data Claude sees → edit `context/investor-template.md`
- Change how conversation history is presented → edit `context/interaction-template.md`

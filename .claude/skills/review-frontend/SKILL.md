---
name: review-frontend
description: Review React / Next.js / TypeScript code for frontend standards compliance. Checks CSS Modules, component patterns, state management.
argument-hint: "[file or directory path]"
---

Review React / Next.js / TypeScript code for compliance with our frontend standards and fix violations.

## Input
- File or area to review: $ARGUMENTS (if empty, review all changed .ts/.tsx/.css files)

## Rules

### Component Pattern
- Function declarations: `function MyComponent()` not `const MyComponent = () =>`
- Logic-first: hooks/handlers at top, JSX return at bottom
- Named interfaces for ALL props: `interface MyComponentProps { ... }`

### Styling (MANDATORY)
- CSS Modules only — no inline styles
- camelCase class names: `.chartContainer`
- State modifiers: `.isActive`, `.isDisabled`
- CSS custom properties for theming

### TypeScript
- Enums over string unions (MANDATORY)
- Named interfaces over anonymous types (MANDATORY)
- No `any` — use `unknown` + type guards
- No barrel exports

### State Management
1. Local state (`useState`) → 2. Context + Reducer → 3. URL state → 4. Server state

### Case Rules (NON-NEGOTIABLE)
- No investment advice | No gamification | No manipulation | No directive language

## Process
1. Read target files
2. Check each rule
3. Fix violations directly
4. Run `npm run build` or `npx tsc --noEmit` to verify

## Reference
- `docs/standards/architecture.md`
- `docs/standards/coding-conventions.md`

$ARGUMENTS

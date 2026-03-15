---
name: review-conventions
description: Review code for compliance with coding conventions and fix violations. Use on any file to check enums, naming, function style, max length.
argument-hint: "[file or directory path]"
---

Review code for compliance with our coding conventions and fix any violations.

## Input
- File or area to review: $ARGUMENTS (if empty, review all staged/changed files)

## Mandatory Rules (MUST fix)

### Universal
1. **Enums over string unions** — no `type X = "a" | "b"` in TS, no `if (status == "active")` in C#
2. **Function declarations over arrow functions** — no `const fn = () => {}`. Use `function fn() {}`
3. **Named interfaces over anonymous types** — no `(data: { name: string })`
4. **No `any` types** — use `unknown` + type guards
5. **Max 15 lines per function** — extract helpers
6. **No hardcoded magic numbers** — use named constants

### TypeScript Naming
- Components: PascalCase | Props: `{Component}Props` | Hooks: `use{Feature}`
- Handlers: `handle{Action}` | Booleans: `is/has` prefix | Constants: SCREAMING_SNAKE_CASE
- CSS: camelCase (CSS Modules only) | Tests: `*.test.ts`

### C# Naming
- Classes/Properties: PascalCase | Interfaces: `I` prefix | Private fields: `_camelCase`
- Constants: SCREAMING_SNAKE_CASE | Async: `Async` suffix | Enums: PascalCase values
- File-scoped namespaces | No `.Result`/`.Wait()` | No `#region` | Named parameters in constructors

## Anti-Patterns to Flag
- Barrel exports | Inline styles | Conversion logic in controllers | Fallback code paths

## Process
1. Read the target files
2. Check each rule
3. Fix violations directly
4. Run build/lint to verify

## Reference
- `docs/standards/coding-conventions.md`

$ARGUMENTS

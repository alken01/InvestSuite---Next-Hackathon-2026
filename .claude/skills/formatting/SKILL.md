---
name: formatting
description: Code formatting conventions for this project. Use when writing or editing code to ensure consistent style.
---

Apply these formatting rules when writing or editing code in this project.

## C# Rules

### 1. Vertical parameter lists
When a method call, constructor, or lambda has **4+ parameters**, put each on its own line:

```csharp
var result = new GetDashboardRequest(
    scenarioId: request.ScenarioId,
    investorId: request.InvestorId,
    mood: request.Mood,
    experience: request.Experience
);
```

### 2. Short parameter lists
3 or fewer short params — keep on one line:
```csharp
var investor = new Investor(id, name, age);
```

### 3. Fluent chains
Each `.Method()` on its own line:
```csharp
var sorted = holdings
    .Where(h => h.Type != "Cash")
    .OrderByDescending(h => h.Value)
    .Take(6)
    .ToList();
```

### 4. General C#
- File-scoped namespaces (`namespace X;`)
- Allman braces for types and methods
- No braces for single-line `if`/`else` with one statement
- 4-space indent

## TypeScript / React Rules

### 1. Function declarations
```typescript
// GOOD
function MyComponent() { }
function handleSubmit() { }

// BAD
const MyComponent = () => { };
const handleSubmit = () => { };
```

### 2. CSS Modules
```typescript
import styles from './Component.module.css';
// camelCase class names: styles.chartContainer
```

### 3. Import order
1. Built-in (`react`, `next`)
2. External packages
3. Internal absolute (`@/...`)
4. Relative imports

$ARGUMENTS

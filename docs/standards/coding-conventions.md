# Coding Conventions

Extracted from established patterns across our projects (Panoptifi, SongEditor, ProviderService).

## Universal Rules

### Enums over String Unions (MANDATORY)
```typescript
// BAD
type Status = "active" | "inactive" | "pending";

// GOOD
enum Status {
  Active = "active",
  Inactive = "inactive",
  Pending = "pending",
}
```

```csharp
// BAD - string comparisons
if (status == "active") { }

// GOOD - enum
if (status == Status.Active) { }
```

### Function Declarations over Arrow Functions (MANDATORY)
```typescript
// BAD
const handleSubmit = () => { };
const MyComponent = () => { };

// GOOD
function handleSubmit() { }
function MyComponent() { }
```
Reserve `const` for primitive values only.

### Named Interfaces over Anonymous Types (MANDATORY)
```typescript
// BAD
function process(data: { name: string; value: number }) { }

// GOOD
interface ProcessInput {
  name: string;
  value: number;
}
function process(data: ProcessInput) { }
```

### No `any` Types
Use `unknown` + type guards instead.

---

## Naming Conventions

### TypeScript / React
| Element | Convention | Example |
|---------|-----------|---------|
| Components | PascalCase | `DashboardHeader` |
| Props interfaces | `{Component}Props` | `DashboardHeaderProps` |
| Hooks | `use{Feature}` | `useDetection` |
| Event handlers | `handle{Action}` | `handleSubmit` |
| Boolean props | `is/has` prefix | `isLoading`, `hasError` |
| Constants | SCREAMING_SNAKE_CASE | `MAX_RETRY_COUNT` |
| CSS classes | camelCase (CSS Modules) | `.chartContainer` |
| Files (components) | PascalCase | `ChartPanel.tsx` |
| Files (utilities) | camelCase | `formatDate.ts` |
| Tests | `*.test.ts` | `formatDate.test.ts` |

### C# / .NET
| Element | Convention | Example |
|---------|-----------|---------|
| Classes/Properties | PascalCase | `UserService` |
| Interfaces | `I` prefix | `IUserService` |
| Private fields | `_camelCase` | `_userRepository` |
| Constants | SCREAMING_SNAKE_CASE | `MAX_RETRY_COUNT` |
| Async methods | `Async` suffix | `GetUserAsync` |
| DB tables | snake_case | `user_profiles` |
| DB columns | snake_case | `created_at` |
| Enums | PascalCase | `TransactionType.Debit` |

---

## Code Organization

### Max Function Length: 15 Lines
Extract helper functions/components when functions get too long. Each function should do one thing.

### Logic-First Components
Main components read like a high-level outline:
```typescript
function Dashboard() {
  const data = useDashboardData();

  function handleRefresh() { /* ... */ }

  return (
    <div>
      <DashboardHeader onRefresh={handleRefresh} />
      <MetricsPanel data={data.metrics} />
      <ChartSection data={data.charts} />
    </div>
  );
}
```

### Import Order (enforced by ESLint)
1. Built-in modules (`react`, `next`)
2. External packages
3. Internal absolute imports (`@/...`)
4. Relative imports (minimize these)

---

## Anti-Patterns to Avoid

- **No barrel exports** — import directly from component files
- **No inline styles** — use CSS Modules
- **No fallback code paths** — complete migrations, don't maintain parallel paths
- **No object initializers for entities** (C#) — use constructors with named parameters
- **No conversion logic in controllers** — move to dedicated converter classes
- **No `#region` blocks** in new code
- **No `.Result` or `.Wait()`** — use async/await
- **No hardcoded magic numbers** — use named constants

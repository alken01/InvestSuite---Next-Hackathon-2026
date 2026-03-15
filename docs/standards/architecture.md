# Architecture Patterns

Common patterns used across our projects, adapted for hackathon speed.

## Backend Architecture (C# / .NET)

### Layered Structure
```
ProjectName/
├── ProjectName.Api/              # Controllers, Hubs, Middleware
├── ProjectName.Core/             # Entities, Enums, Interfaces, DTOs
│   └── Features/                 # MediatR Commands/Queries/Handlers (CQRS)
├── ProjectName.Infrastructure/   # DbContext, Repositories, External APIs
└── ProjectName.Jobs/             # Background workers (if needed)
```

### Key Patterns
1. **CQRS via MediatR** — separate read/write operations
2. **Repository Pattern** — interfaces in Core, implementations in Infrastructure
3. **Unit of Work** — transaction management (SaveChanges at UoW level, not repos)
4. **Factory Methods** — `Entity.Create()` static methods, private constructors
5. **Named Parameters** — always use named params in constructor calls

### Controller Pattern
```csharp
[HttpPost("endpoint")]
public async Task<IActionResult> Operation([FromBody] RequestDto request)
{
    var command = request.ToCommand();
    var result = await _mediator.Send(command);
    return result.IsSuccess
        ? Ok(result.Value)
        : BadRequest(result.Error);
}
```
Controllers should be thin — no business logic, no data transformation.

### API Response Standards
| Code | Usage |
|------|-------|
| 200 | Success + data |
| 201 | Created + Location header |
| 400 | Validation errors |
| 404 | Not found |
| 422 | Business rule violation |
| 429 | Rate limited + Retry-After |
| 500 | Internal error (global middleware) |

### Global Error Handling
```csharp
// ExceptionMiddleware catches all exceptions
// Map to specific HTTP responses:
// ValidationException → 400
// EntityNotFoundException → 404
// BusinessRuleException → 422
// Everything else → 500
```

---

## Frontend Architecture (React / Next.js)

### Directory Structure
```
src/
├── app/                    # Next.js App Router pages
├── components/             # UI components (PascalCase)
│   ├── {feature}/          # Feature-grouped components
│   │   ├── Component.tsx
│   │   └── Component.module.css
├── lib/
│   ├── api/                # API client methods
│   ├── hooks/              # Custom hooks
│   ├── types/              # TypeScript types/interfaces
│   ├── utils/              # Utility functions
│   └── constants/          # App-wide constants
├── contexts/               # React Context providers
└── styles/                 # Global CSS, design tokens
```

### Context Split Pattern (prevents unnecessary re-renders)
```
{Feature}Context/
├── {Feature}ConfigContext.tsx    # Config state (changes rarely)
├── {Feature}StateContext.tsx     # Runtime state (changes often)
├── {Feature}ActionsContext.tsx   # Action handlers
├── hooks.ts                     # useFeature(), useFeatureConfig()
├── types.ts                     # Interfaces
└── index.ts                     # Public API exports
```

### Component Composition
```
Feature/
├── Feature.tsx              # Main component (orchestrator)
├── Feature.module.css       # Scoped styles
├── FeatureHeader.tsx        # Sub-component
├── FeatureContent.tsx       # Sub-component
└── FeatureActions.tsx       # Sub-component
```

### State Management Hierarchy
1. **Local state** (`useState`) — component-specific
2. **Context + Reducer** — feature-wide shared state
3. **URL state** — shareable/bookmarkable state
4. **Server state** — API data (fetch on mount / SWR / React Query)

---

## Frontend-Backend Integration

### OpenAPI Type Generation
```bash
# After building backend, sync types:
npm run generate-types
# Generates TypeScript types from swagger.json
```

### Real-time Communication (SignalR)
```
Client → HTTP Request → 202 Accepted + groupName
Client → SignalR join group
Server → OnStatusChanged / OnComplete / OnError events
Client → Auto-refetch on completion
```

---

## Styling

### CSS Modules Only
```typescript
// Component.tsx
import styles from './Component.module.css';

function Component() {
  return <div className={styles.container}>...</div>;
}
```

### CSS Custom Properties for Theming
```css
:root {
  --color-bg-primary: #0a0a0f;
  --color-text-primary: #f0f0f5;
  --spacing-sm: 8px;
  --spacing-md: 16px;
  --radius-md: 6px;
  --transition-normal: 200ms;
}
```

### Class Naming
- camelCase in CSS Modules: `.chartContainer`, `.headerTitle`
- State modifiers: `.isActive`, `.isDisabled`, `.isLoading`

---

## Testing Strategy

### 4-Layer Testing (from ProviderService)
1. **Unit Tests** — pure functions, isolated logic
2. **Component Tests** — render + interaction
3. **Integration Tests** — feature workflows, API handlers
4. **E2E Tests** — full user flows

### Test Naming Convention
```
{MethodName}_{Scenario}_{ExpectedResult}
```
Example: `Debit_ValidRequest_ReturnsSuccessfulResponse`

### Test Patterns
- Use test builders / factories for test data
- Use named constants (not magic numbers)
- Mock at boundaries (APIs, DB), not internal functions
- Sequential execution for memory safety in large suites

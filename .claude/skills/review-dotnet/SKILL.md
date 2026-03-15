---
name: review-dotnet
description: Review C# / ASP.NET Core code for .NET backend practices compliance. Checks entity design, converters, repos, async patterns, type safety, and architectural rules.
argument-hint: "[file or directory path]"
---

Review C# / ASP.NET Core code for compliance with our .NET backend practices and fix all violations.

## Input
- File or area to review: $ARGUMENTS (if empty, review all changed .cs files)

---

## Rules

### Entity Design
- Private setters on all properties
- Static `Create()` factory methods only — no `new Entity { Prop = x }` object initializers
- Constructors use named parameters
- No anemic models — entities own their validation and state transitions
- Use `required` keyword on DTOs where applicable

### Return Types
- **Never use tuples as return values** — always define a dedicated class or record
- Use `Result<T>` / `OneOf<T>` pattern for operations that can fail instead of throwing exceptions for control flow
- Prefer `record` for immutable value objects and DTOs
- Return typed domain objects, never `object` or `dynamic`

### Type Safety — Enums and Classes
- Use `enum` for any closed set of named values (status, type, category, etc.) — never raw strings or ints
- Use a strongly typed class or `record` instead of primitive obsession (`UserId`, `OrderAmount`, etc.)
- Use `sealed` on classes not designed for inheritance
- Prefer `readonly struct` for small, frequently allocated value types

### Interfaces
- Extract an interface (`IXxxService`, `IXxxRepository`) for every injectable dependency
- Program to interfaces, never to concrete types in constructors
- Keep interfaces focused — ISP applies; split fat interfaces
- Use `IReadOnlyList<T>`, `IReadOnlyDictionary<K,V>` on return types when mutation is not intended

### Constants — Zero Magic Strings/Numbers
- **Every literal string or number used as a value must be a `private const` or `public static readonly`**
- Group related constants in a dedicated `static class` (e.g., `ErrorMessages`, `RouteConstants`, `ClaimTypes`)
- Enum members replace any string-based switch/if chains
- Config keys must reference typed options classes via `IOptions<T>` — no raw `IConfiguration["key"]` inline

```csharp
// BAD
if (user.Role == "admin") { ... }

// GOOD
private const string AdminRole = "admin";
// or even better:
public enum UserRole { Admin, User, Moderator }
if (user.Role == UserRole.Admin) { ... }
```

### Converter / Mapper Pattern
- Extension methods on the source type: `public static XDto ToDto(this X entity)`
- ALL transformation logic in converters — never in controllers or services
- Use named parameters in all method calls
- Converters live in a `Converters/` or `Mappers/` folder, one file per domain type

### Controller Pattern
- Thin controllers — zero business logic, zero data transformation
- Inject services via interface only
- Proper HTTP codes: `200 OK` / `201 Created` / `400 Bad Request` / `404 Not Found` / `422 Unprocessable Entity` / `500 Internal Server Error`
- Use `[ProducesResponseType]` attributes for Swagger accuracy
- Return `IActionResult` or `ActionResult<T>`, never raw objects

### Service / Repository Pattern
- Services depend on `IRepository<T>` interfaces, never on `DbContext` directly
- One responsibility per service — no god classes
- Repositories return domain entities, not `IQueryable<T>` outside the data layer

### Async Patterns
- **No `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`** — always `await`
- `Async` suffix on every async method
- Pass and respect `CancellationToken` all the way down
- Use `ConfigureAwait(false)` in library/non-UI code
- Never `async void` except for event handlers

### Naming
| Target | Convention |
|---|---|
| Classes, Methods, Properties | `PascalCase` |
| Private fields | `_camelCase` |
| Local variables, parameters | `camelCase` |
| Interfaces | `IPrefix` |
| Async methods | `XxxAsync` suffix |
| Constants | `SCREAMING_SNAKE_CASE` |
| Type parameters | `T`, `TEntity`, `TResult` |

### Code Quality
- File-scoped namespaces (`namespace Foo.Bar;`)
- No `#region` blocks
- Max 15 lines per method — extract helpers otherwise
- No `var` when the type is not obvious from the right-hand side
- Null checks via `ArgumentNullException.ThrowIfNull()` or nullable reference types (`#nullable enable`)
- Prefer pattern matching and switch expressions over if-else chains
- Use `is` / `is not` for null checks: `if (x is null)`
- No `catch (Exception e)` swallowing — always log and rethrow or handle specifically

```csharp
// BAD
public (User user, string error) GetUser(int id) { ... }

// GOOD
public sealed record GetUserResult(User User, string? Error);
public async Task<GetUserResult> GetUserAsync(int id, CancellationToken ct) { ... }
```

### Dependency Injection
- Register via interfaces only
- Use `AddScoped` / `AddSingleton` / `AddTransient` deliberately — comment why if non-obvious
- No service locator pattern (`IServiceProvider` injected into domain classes)

### Logging
- Inject `ILogger<T>` — the only dependency exempt from the constant rule (message strings may be inline literals in `LogXxx` calls for structured logging readability)
- Use structured logging: `_logger.LogInformation("Order {OrderId} created", orderId)`
- Use log levels correctly: `Debug` for dev noise, `Information` for domain events, `Warning` for recoverable issues, `Error` for failures

### Testing
- Naming: `{MethodName}_{Scenario}_{ExpectedResult}`
- Named constants for all test inputs — no magic values in assertions
- Mock at boundaries using interfaces
- Use FluentAssertions for all assertions
- Prefer `[Theory]` + `[InlineData]` over duplicated `[Fact]` tests
- Arrange / Act / Assert with blank lines separating each section

---

## Process
1. Read all target `.cs` files
2. Check every rule category above
3. Fix violations directly in the files
4. Ensure all new types have corresponding interfaces where injectable
5. Replace every magic string/number with a constant or enum
6. Run `dotnet build` — fix any compilation errors
7. Run `dotnet test` if tests exist — all must pass

## Reference
- `docs/standards/dotnet-practices.md`
- `docs/standards/coding-conventions.md`

$ARGUMENTS

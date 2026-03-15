---
name: review-tests
description: Review test code for testing standards compliance. Checks naming, constants, mocking strategy, edge case coverage.
argument-hint: "[test file or directory]"
---

Review test code for compliance with our testing standards and fix violations.

## Input
- Test file or area to review: $ARGUMENTS (if empty, review all test files)

## Test Naming Convention (MANDATORY)
```
{MethodName}_{Scenario}_{ExpectedResult}
```
Examples: `GetById_ValidId_ReturnsInvestor`, `Handle_EmptyScenarioId_ReturnsBadRequest`

## Rules

### Structure
- Arrange / Act / Assert pattern
- One assertion concept per test
- Group related tests with comment headers

### Data
- Named constants: `const CURRENT_PRICE = 745.20m;`
- No magic numbers
- Shared fixtures as `private static readonly`
- `[Theory]` + `[InlineData]` for parameterized tests

### Mocking
- Mock at boundaries (APIs, DB), NOT internal functions
- Prefer real instances for simple data classes
- C#: Moq | TS: Jest

### Assertions
- C#: FluentAssertions | TS: Jest matchers
- Verify behavior, not implementation
- Check error message content, not just error type

### Coverage
- Edge cases (null, empty, boundary values)
- Error paths tested, not just happy path
- Tests are independent (no shared mutable state)

## Process
1. Read test files
2. Check each rule
3. Fix violations directly
4. Run test suite to verify

## Reference
- `docs/standards/architecture.md` (Testing Strategy section)

$ARGUMENTS

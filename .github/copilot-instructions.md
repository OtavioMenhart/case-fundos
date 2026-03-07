# GitHub Copilot Instructions

## Project Overview

This solution follows a **Domain-Driven Design (DDD)** architecture divided into the following layers. Every change or new feature **must** respect this structure.

```
CaseItau.API          → Presentation layer (Controllers, DTOs/ViewModels, DI registration)
CaseItau.Application  → Application layer (Use cases, Services, Interfaces consumed by API)
CaseItau.Domain       → Domain layer (Entities, Value Objects, Domain Interfaces, Domain Services)
CaseItau.Infra        → Infrastructure layer (Repository implementations, Database, External services)
CaseItau.Tests        → Unit and integration tests (mirrors source layer structure)
```

### Dependency flow (inner layers must never depend on outer layers)

```
API → Application → Domain
Infra → Domain
Tests → any layer under test
```

---

## General Rules

### Always show the plan first
Before writing or modifying any code, **present a clear step-by-step plan** describing:
- Which files will be created or modified
- Which layer each file belongs to
- Why each change is necessary

Only proceed with implementation after the plan is stated.

---

## Architecture & Design

### Domain-Driven Design (DDD)
- Place **Entities** and **Value Objects** in `CaseItau.Domain/Entities` and `CaseItau.Domain/ValueObjects`.
- Define **repository interfaces** and **domain service interfaces** in `CaseItau.Domain/Interfaces`.
- **Never** reference infrastructure or application concerns from the domain layer.

### Dependency Injection (DI)
- Register all dependencies in `CaseItau.API/Program.cs` or in dedicated extension methods (e.g., `ServiceCollectionExtensions`).
- Use constructor injection everywhere; avoid service locator patterns.
- Prefer interface abstractions over concrete implementations across layer boundaries.

### Application Layer
- Implement use-case logic as **services** or **handlers** inside `CaseItau.Application/Services`.
- Define the interfaces that the API depends on inside `CaseItau.Application/Interfaces`.
- Use **DTOs** (`CaseItau.Application/DTOs`) to carry data between the API and the application layer — never expose domain entities directly to the API.

### Infrastructure Layer
- Implement interfaces defined in `CaseItau.Domain/Interfaces` inside `CaseItau.Infra/Repositories`.
- Keep all database, file-system, or external-service concerns inside this layer.

---

## Coding Standards

### Simplicity
- Prefer the simplest solution that fulfills the requirement.
- Avoid over-engineering; introduce abstractions only when they provide clear value.
- Keep methods small and focused on a single responsibility.

### .NET 10
- Always prefer the latest **idiomatic .NET 10** APIs and language features (e.g., primary constructors, collection expressions, `System.Text.Json` improvements, minimal APIs where appropriate).
- Use `async/await` throughout; avoid blocking calls (`.Result`, `.Wait()`).
- Leverage built-in DI, logging (`ILogger<T>`), and configuration (`IOptions<T>`) instead of third-party alternatives when the built-in is sufficient.

### One Class / Interface Per File
- Every class, record, interface, or enum must reside in **its own file**.
- The file name must match the type name exactly (e.g., `FundoService.cs` for `FundoService`).

### Naming Conventions
- Classes / Interfaces / Methods: `PascalCase`
- Parameters / Local variables: `camelCase`
- Private fields: `_camelCase`
- Constants: `PascalCase` or `UPPER_SNAKE_CASE` (be consistent within a file)

---

## Documentation

### XML Documentation (English only)
- All **public** types, methods, properties, and constructors must have XML documentation comments in **English**.
- Use `<summary>`, `<param>`, `<returns>`, and `<exception>` tags as appropriate.
- Internal implementation details may use inline comments in English when the logic is non-obvious.

```csharp
/// <summary>
/// Retrieves a fund by its unique code.
/// </summary>
/// <param name="codigo">The unique code that identifies the fund.</param>
/// <returns>The <see cref="FundoDto"/> matching the provided code, or <c>null</c> if not found.</returns>
public Task<FundoDto?> GetByCodigo(string codigo);
```

---

## Unit Tests (`CaseItau.Tests`)

- Mirror the source layer structure inside the test project (e.g., `Tests/Application/Services/FundoServiceTests.cs`).
- Use **xUnit** as the test framework.
- Use **Moq** (or `NSubstitute`) for mocking dependencies.
- Follow the **Arrange / Act / Assert** pattern with a blank line separating each section.
- Name test methods using the pattern: `MethodName_StateUnderTest_ExpectedBehavior`.
- Every public method in the Application and Domain layers must have corresponding unit tests.
- Aim for high coverage on business logic; infrastructure tests should be integration tests and kept separate.

```csharp
[Fact]
public async Task GetByCodigo_WhenFundoExists_ReturnsFundoDto()
{
    // Arrange
    var repositoryMock = new Mock<IFundoRepository>();
    repositoryMock.Setup(r => r.GetByCodigo("ITAUTESTE01"))
                  .ReturnsAsync(new Fundo { Codigo = "ITAUTESTE01" });
    var service = new FundoService(repositoryMock.Object);

    // Act
    var result = await service.GetByCodigo("ITAUTESTE01");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("ITAUTESTE01", result.Codigo);
}
```

---

## Security & Quality

- **Never** concatenate user input into SQL queries — always use parameterized queries or an ORM.
- **Never** expose connection strings in source code; use `appsettings.json` + environment variables + `IConfiguration`.
- Validate inputs at the API boundary (controller or DTO validation attributes).
- Return appropriate HTTP status codes (`404 NotFound`, `400 BadRequest`, `201 Created`, etc.) instead of swallowing errors silently.

# .NET Backend Practices

Specific patterns for the C# / ASP.NET Core backend.

## Project Structure

For a hackathon, we can simplify to 2-3 projects:

```
Solution/
├── ProjectName.Api/              # Controllers + Program.cs + Middleware
├── ProjectName.Core/             # Entities, DTOs, Enums, Interfaces, MediatR handlers
└── ProjectName.Infrastructure/   # DbContext, Repos, External API clients
```

Skip the Jobs project unless we need background processing.

## Entity Design

```csharp
public class Portfolio
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public decimal TotalValue { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Portfolio() { } // EF Core

    public static Portfolio Create(string name, decimal initialValue)
    {
        return new Portfolio
        {
            Name = name,
            TotalValue = initialValue,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

- Private setters
- Static `Create()` factory methods
- Private parameterless constructor for EF Core

## CQRS with MediatR

```csharp
// Command
public record CreatePortfolioCommand(string Name, decimal InitialValue) : IRequest<PortfolioDto>;

// Handler
public class CreatePortfolioHandler : IRequestHandler<CreatePortfolioCommand, PortfolioDto>
{
    private readonly IPortfolioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePortfolioHandler(IPortfolioRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PortfolioDto> Handle(CreatePortfolioCommand command, CancellationToken ct)
    {
        var portfolio = Portfolio.Create(
            name: command.Name,
            initialValue: command.InitialValue
        );

        _repository.Add(portfolio);
        await _unitOfWork.SaveChangesAsync(ct);

        return portfolio.ToDto();
    }
}
```

## Repository Pattern

```csharp
// Interface in Core
public interface IPortfolioRepository
{
    Task<Portfolio?> GetByIdAsync(int id, CancellationToken ct);
    void Add(Portfolio portfolio);
}

// Implementation in Infrastructure
public class PortfolioRepository : IPortfolioRepository
{
    private readonly AppDbContext _context;

    public PortfolioRepository(AppDbContext context) => _context = context;

    public async Task<Portfolio?> GetByIdAsync(int id, CancellationToken ct)
        => await _context.Portfolios.FindAsync(new object[] { id }, ct);

    public void Add(Portfolio portfolio) => _context.Portfolios.Add(portfolio);
    // NO SaveChanges here — Unit of Work handles it
}
```

## Converter / Mapper Pattern

```csharp
// Extension methods on the source type
public static class PortfolioConverters
{
    public static PortfolioDto ToDto(this Portfolio portfolio)
    {
        return new PortfolioDto(
            Id: portfolio.Id,
            Name: portfolio.Name,
            TotalValue: portfolio.TotalValue
        );
    }

    public static CreatePortfolioCommand ToCommand(this CreatePortfolioRequest request)
    {
        return new CreatePortfolioCommand(
            Name: request.Name,
            InitialValue: request.InitialValue
        );
    }
}
```

- Extend the type that **holds the data**
- All transformation logic in converters, not controllers
- Use named parameters

## DI Registration

```csharp
// Program.cs
builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePortfolioHandler).Assembly));
```

## EditorConfig

```ini
[*.cs]
indent_style = space
indent_size = 4

# Naming
dotnet_naming_rule.private_fields.symbols = private_fields
dotnet_naming_rule.private_fields.style = _camelCase
dotnet_naming_rule.public_members.style = PascalCase
dotnet_naming_rule.constants.style = SCREAMING_SNAKE_CASE

# Preferences
csharp_style_namespace_declarations = file_scoped
csharp_using_directive_placement = outside_namespace
csharp_prefer_simple_using_statement = true
```

## Database (EF Core)

```csharp
// Configuration
public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolios");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.TotalValue).HasColumnName("total_value").HasPrecision(18, 2);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
    }
}
```

- Table names: snake_case
- Column names: snake_case
- Use `HasPrecision` for decimal columns

## Hackathon Shortcuts (acceptable for 24h)

- Skip Unit of Work if only one DbContext — call `SaveChangesAsync` directly
- Skip MediatR if app is simple — inject repos directly into controllers
- Use records for DTOs: `public record PortfolioDto(int Id, string Name, decimal TotalValue);`
- Use SQLite instead of PostgreSQL for zero-config setup
- Skip repository interfaces if not writing tests

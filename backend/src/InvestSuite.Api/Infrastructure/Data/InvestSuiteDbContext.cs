using InvestSuite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class InvestSuiteDbContext : DbContext
{
    public DbSet<AccountEntity> Accounts => Set<AccountEntity>();
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
    public DbSet<StockPriceHistory> StockPriceHistory => Set<StockPriceHistory>();

    public InvestSuiteDbContext(DbContextOptions<InvestSuiteDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountEntity>(e =>
        {
            e.ToTable("Accounts");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasMaxLength(50);
            e.Property(a => a.FirstName).HasMaxLength(100);
            e.Property(a => a.LastName).HasMaxLength(100);
            e.Property(a => a.CashBalance).HasColumnType("decimal(18,2)");
            e.Property(a => a.RiskProfile).HasMaxLength(50);
            e.Property(a => a.ExperienceLevel).HasMaxLength(50);
            e.Property(a => a.Personality).HasMaxLength(200);
            e.Property(a => a.ClerkUserId).HasMaxLength(200);
            e.HasIndex(a => a.ClerkUserId).IsUnique().HasFilter("ClerkUserId IS NOT NULL");
            e.HasIndex(a => new { a.FirstName, a.LastName }).IsUnique();
        });

        modelBuilder.Entity<TransactionEntity>(e =>
        {
            e.ToTable("Transactions");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasMaxLength(50);
            e.Property(t => t.AccountId).HasMaxLength(50);
            e.Property(t => t.Symbol).HasMaxLength(20);
            e.Property(t => t.Shares).HasColumnType("decimal(18,6)");
            e.Property(t => t.PricePerShare).HasColumnType("decimal(18,4)");
            e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
            e.HasIndex(t => t.AccountId);
        });

        modelBuilder.Entity<StockPriceHistory>(e =>
        {
            e.ToTable("StockPriceHistory");
            e.HasKey(p => p.Id);
            e.Property(p => p.Symbol).HasMaxLength(20);
            e.Property(p => p.Open).HasColumnType("decimal(18,4)");
            e.Property(p => p.High).HasColumnType("decimal(18,4)");
            e.Property(p => p.Low).HasColumnType("decimal(18,4)");
            e.Property(p => p.Close).HasColumnType("decimal(18,4)");
            e.HasIndex(p => new { p.Symbol, p.Date }).IsUnique();
        });

    }
}

public class AccountEntity
{
    public string Id { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public decimal CashBalance { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? RiskProfile { get; set; }
    public string? ExperienceLevel { get; set; }
    public string? Personality { get; set; }
    public string? ClerkUserId { get; set; }
}

public class TransactionEntity
{
    public string Id { get; set; } = "";
    public string AccountId { get; set; } = "";
    public string Type { get; set; } = "buy"; // todo: enum
    public string? Symbol { get; set; }
    public decimal? Shares { get; set; }
    public decimal? PricePerShare { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset ExecutedAt { get; set; }
}

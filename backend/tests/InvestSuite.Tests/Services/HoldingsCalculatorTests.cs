using FluentAssertions;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using InvestSuite.Api.Models;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InvestSuite.Tests.Services;

public class HoldingsCalculatorTests : IDisposable
{
    private const string ACCOUNT_ID = "test-acc";
    private const string SYMBOL_ASML = "ASML";
    private const string SYMBOL_SAP = "SAP";
    private const decimal ASML_PRICE = 745.20m;
    private const decimal SAP_PRICE = 220.50m;
    private const decimal INITIAL_DEPOSIT = 5000m;
    private static readonly DateOnly AS_OF_DATE = new(2025, 9, 15);

    private readonly InvestSuiteDbContext _db;
    private readonly EfTransactionRepository _transactions;
    private readonly Mock<IHistoricalPriceService> _prices;
    private readonly Mock<IStockRepository> _stocks;

    public HoldingsCalculatorTests()
    {
        var options = new DbContextOptionsBuilder<InvestSuiteDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _db = new InvestSuiteDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _transactions = new EfTransactionRepository(_db);

        _prices = new Mock<IHistoricalPriceService>();
        _stocks = new Mock<IStockRepository>();

        _prices.Setup(p => p.GetClosingPrice(SYMBOL_ASML, It.IsAny<DateOnly>()))
            .Returns(ASML_PRICE);
        _prices.Setup(p => p.GetClosingPrice(SYMBOL_SAP, It.IsAny<DateOnly>()))
            .Returns(SAP_PRICE);

        _stocks.Setup(s => s.GetStock(SYMBOL_ASML))
            .Returns(new StockDetail(SYMBOL_ASML, ASML_PRICE, 1.2m, "Chip machines",
                301m, 35.2m, 28m, 0.7m, 18, 4, 1, "Strong Buy", 820m, 5, 4, 3));
        _stocks.Setup(s => s.GetStock(SYMBOL_SAP))
            .Returns(new StockDetail(SYMBOL_SAP, SAP_PRICE, 0.5m, "Enterprise software",
                270m, 25m, 30m, 1.1m, 20, 5, 0, "Buy", 250m, 4, 3, 2));
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    private HoldingsCalculator CreateCalculator() =>
        new(_transactions, _prices.Object, _stocks.Object);

    // ── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public void ComputeHoldings_SingleBuy_ReturnsCorrectPosition()
    {
        AddBuy(SYMBOL_ASML, shares: 2m, amount: 1360m);

        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        var asml = holdings.First(h => h.Symbol == SYMBOL_ASML);
        asml.Shares.Should().Be(2m);
        asml.AverageCost.Should().Be(680m);
        asml.CurrentPrice.Should().Be(ASML_PRICE);
        asml.Value.Should().Be(Math.Round(2m * ASML_PRICE, 0));
        asml.ReturnPct.Should().Be(Math.Round((ASML_PRICE - 680m) / 680m * 100, 1));
    }

    [Fact]
    public void ComputeHoldings_BuyThenSell_ReducesShares()
    {
        AddBuy(SYMBOL_ASML, shares: 4m, amount: 2980m);
        AddSell(SYMBOL_ASML, shares: 1m, amount: 745.20m);

        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        var asml = holdings.First(h => h.Symbol == SYMBOL_ASML);
        asml.Shares.Should().Be(3m);
    }

    [Fact]
    public void ComputeHoldings_SellAll_ExcludesZeroPosition()
    {
        AddBuy(SYMBOL_ASML, shares: 2m, amount: 1490m);
        AddSell(SYMBOL_ASML, shares: 2m, amount: 1490m);

        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        holdings.Should().NotContain(h => h.Symbol == SYMBOL_ASML);
    }

    [Fact]
    public void ComputeHoldings_MultipleBuys_AveragesCost()
    {
        AddBuy(SYMBOL_ASML, shares: 1m, amount: 700m);
        AddBuy(SYMBOL_ASML, shares: 1m, amount: 800m);

        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        var asml = holdings.First(h => h.Symbol == SYMBOL_ASML);
        asml.Shares.Should().Be(2m);
        asml.AverageCost.Should().Be(750m); // (700 + 800) / 2
    }

    [Fact]
    public void ComputeHoldings_IncludesCashHolding()
    {
        AddBuy(SYMBOL_ASML, shares: 1m, amount: 745m);

        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        var cash = holdings.First(h => h.Symbol == "CASH");
        cash.Type.Should().Be(HoldingType.Cash);
        cash.Value.Should().Be(Math.Round(INITIAL_DEPOSIT - 745m, 0));
    }

    [Fact]
    public void ComputeHoldings_NoTransactions_ReturnsCashOnly()
    {
        var holdings = CreateCalculator().ComputeHoldings(ACCOUNT_ID, AS_OF_DATE, INITIAL_DEPOSIT);

        holdings.Should().HaveCount(1);
        holdings[0].Symbol.Should().Be("CASH");
        holdings[0].Value.Should().Be(INITIAL_DEPOSIT);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void AddBuy(string symbol, decimal shares, decimal amount)
    {
        _db.Transactions.Add(new TransactionEntity
        {
            Id = $"tx-{Guid.NewGuid():N}"[..12],
            AccountId = ACCOUNT_ID,
            Type = "Buy",
            Symbol = symbol,
            Shares = shares,
            PricePerShare = amount / shares,
            Amount = amount,
            ExecutedAt = AS_OF_DATE.ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc)
        });
        _db.SaveChanges();
    }

    private void AddSell(string symbol, decimal shares, decimal amount)
    {
        _db.Transactions.Add(new TransactionEntity
        {
            Id = $"tx-{Guid.NewGuid():N}"[..12],
            AccountId = ACCOUNT_ID,
            Type = "Sell",
            Symbol = symbol,
            Shares = shares,
            PricePerShare = amount / shares,
            Amount = amount,
            ExecutedAt = AS_OF_DATE.ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc)
        });
        _db.SaveChanges();
    }
}

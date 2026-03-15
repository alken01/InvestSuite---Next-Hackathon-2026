using FluentAssertions;
using InvestSuite.Api.Controllers;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using InvestSuite.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace InvestSuite.Tests.Controllers;

public class TradeControllerTests : IDisposable
{
    private const string ACCOUNT_ID = "test-acc";
    private const string VALID_SYMBOL = "ASML";
    private const string VALID_DATE = "2025-09-15";
    private const decimal CLOSING_PRICE = 745.20m;
    private const decimal INITIAL_CASH = 1000m;

    private readonly InvestSuiteDbContext _db;
    private readonly EfAccountRepository _accounts;
    private readonly EfTransactionRepository _transactions;
    private readonly Mock<IHistoricalPriceService> _prices;
    private readonly Mock<IStockRepository> _stocks;
    private readonly TradeController _controller;

    public TradeControllerTests()
    {
        var options = new DbContextOptionsBuilder<InvestSuiteDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        _db = new InvestSuiteDbContext(options);
        _db.Database.OpenConnection();
        _db.Database.EnsureCreated();

        _accounts = new EfAccountRepository(_db);
        _transactions = new EfTransactionRepository(_db);

        _prices = new Mock<IHistoricalPriceService>();
        _stocks = new Mock<IStockRepository>();

        _prices.Setup(p => p.GetClosingPrice(VALID_SYMBOL, It.IsAny<DateOnly>()))
            .Returns(CLOSING_PRICE);
        _stocks.Setup(s => s.GetStock(VALID_SYMBOL))
            .Returns(new StockDetail(VALID_SYMBOL, CLOSING_PRICE, 1.2m, "Chip machines",
                301m, 35.2m, 28m, 0.7m, 18, 4, 1, "Strong Buy", 820m, 5, 4, 3));

        SeedAccount();

        _controller = new TradeController(
            _accounts, _transactions, _prices.Object, _stocks.Object, _db);
    }

    private void SeedAccount()
    {
        _db.Accounts.Add(new AccountEntity
        {
            Id = ACCOUNT_ID,
            FirstName = "Test",
            LastName = "User",
            CashBalance = INITIAL_CASH,
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Database.CloseConnection();
        _db.Dispose();
    }

    // ── Buy: happy path ─────────────────────────────────────────────────

    [Fact]
    public void Buy_ValidRequest_ReturnsOkWithTransactionDetails()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: 250m, VALID_DATE);

        var result = _controller.Buy(ACCOUNT_ID, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ToDynamic(ok.Value!);
        ((string)body.symbol).Should().Be(VALID_SYMBOL);
        ((decimal)body.shares).Should().Be(Math.Round(250m / CLOSING_PRICE, 6));
        ((decimal)body.price_per_share).Should().Be(CLOSING_PRICE);
        ((decimal)body.amount).Should().Be(250m);
    }

    [Fact]
    public void Buy_ValidRequest_DeductsCashBalance()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: 250m, VALID_DATE);

        var result = _controller.Buy(ACCOUNT_ID, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ToDynamic(ok.Value!);
        ((decimal)body.remaining_cash).Should().Be(INITIAL_CASH - 250m);
    }

    [Fact]
    public void Buy_ValidRequest_PersistsTransaction()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: 250m, VALID_DATE);

        _controller.Buy(ACCOUNT_ID, request);

        var txs = _db.Transactions.Where(t => t.AccountId == ACCOUNT_ID).ToList();
        txs.Should().HaveCount(1);
        txs[0].Type.Should().Be("Buy");
        txs[0].Symbol.Should().Be(VALID_SYMBOL);
        txs[0].Amount.Should().Be(250m);
    }

    // ── Buy: validation errors ──────────────────────────────────────────

    [Fact]
    public void Buy_UnknownAccount_ReturnsNotFound()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: 100m, VALID_DATE);

        var result = _controller.Buy("nonexistent", request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Buy_UnknownSymbol_ReturnsBadRequest()
    {
        var request = new TradeRequest("FAKE", Amount: 100m, VALID_DATE);

        var result = _controller.Buy(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Buy_InsufficientCash_ReturnsBadRequest()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: INITIAL_CASH + 1m, VALID_DATE);

        var result = _controller.Buy(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Buy_ZeroAmount_ReturnsBadRequest()
    {
        var request = new TradeRequest(VALID_SYMBOL, Amount: 0m, VALID_DATE);

        var result = _controller.Buy(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Sell: happy path ────────────────────────────────────────────────

    [Fact]
    public void Sell_ValidRequest_ReturnsOkWithTransactionDetails()
    {
        SeedBuyTransaction(shares: 2m, amount: 1490m);
        var request = new SellRequest(VALID_SYMBOL, Shares: 1m, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ToDynamic(ok.Value!);
        ((string)body.symbol).Should().Be(VALID_SYMBOL);
        ((decimal)body.shares).Should().Be(1m);
        ((decimal)body.price_per_share).Should().Be(CLOSING_PRICE);
        ((decimal)body.amount).Should().Be(Math.Round(1m * CLOSING_PRICE, 2));
    }

    [Fact]
    public void Sell_ValidRequest_AddsCashBalance()
    {
        var buyAmount = 500m;
        SeedBuyTransaction(shares: 2m, amount: buyAmount);
        var sellShares = 1m;
        var request = new SellRequest(VALID_SYMBOL, Shares: sellShares, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ToDynamic(ok.Value!);
        var expectedCash = (INITIAL_CASH - buyAmount) + Math.Round(sellShares * CLOSING_PRICE, 2);
        ((decimal)body.remaining_cash).Should().Be(expectedCash);
    }

    [Fact]
    public void Sell_ValidRequest_PersistsTransaction()
    {
        SeedBuyTransaction(shares: 2m, amount: 1490m);
        var request = new SellRequest(VALID_SYMBOL, Shares: 1m, VALID_DATE);

        _controller.Sell(ACCOUNT_ID, request);

        var txs = _db.Transactions
            .Where(t => t.AccountId == ACCOUNT_ID && t.Type == "Sell")
            .ToList();
        txs.Should().HaveCount(1);
        txs[0].Symbol.Should().Be(VALID_SYMBOL);
    }

    // ── Sell: validation errors ─────────────────────────────────────────

    [Fact]
    public void Sell_UnknownAccount_ReturnsNotFound()
    {
        var request = new SellRequest(VALID_SYMBOL, Shares: 1m, VALID_DATE);

        var result = _controller.Sell("nonexistent", request);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Sell_UnknownSymbol_ReturnsBadRequest()
    {
        var request = new SellRequest("FAKE", Shares: 1m, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Sell_InsufficientShares_ReturnsBadRequest()
    {
        SeedBuyTransaction(shares: 1m, amount: 745m);
        var request = new SellRequest(VALID_SYMBOL, Shares: 5m, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Sell_NoHoldings_ReturnsBadRequest()
    {
        var request = new SellRequest(VALID_SYMBOL, Shares: 1m, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Sell_ZeroShares_ReturnsBadRequest()
    {
        var request = new SellRequest(VALID_SYMBOL, Shares: 0m, VALID_DATE);

        var result = _controller.Sell(ACCOUNT_ID, request);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SeedBuyTransaction(decimal shares, decimal amount)
    {
        var date = DateOnly.Parse(VALID_DATE);
        _db.Transactions.Add(new TransactionEntity
        {
            Id = $"tx-seed-{Guid.NewGuid():N}"[..12],
            AccountId = ACCOUNT_ID,
            Type = "Buy",
            Symbol = VALID_SYMBOL,
            Shares = shares,
            PricePerShare = CLOSING_PRICE,
            Amount = amount,
            ExecutedAt = date.ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc)
        });

        var account = _db.Accounts.First(a => a.Id == ACCOUNT_ID);
        account.CashBalance -= amount;
        _db.SaveChanges();
    }

    private static dynamic ToDynamic(object obj)
    {
        var type = obj.GetType();
        var dict = new System.Dynamic.ExpandoObject() as IDictionary<string, object?>;
        foreach (var prop in type.GetProperties())
        {
            dict[prop.Name] = prop.GetValue(obj);
        }
        return dict;
    }
}

using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Infrastructure.Services;
using InvestSuite.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvestSuite.Api.Controllers;

[ApiController]
[Route("api/accounts/{accountId}")]
public sealed class TradeController : ControllerBase
{
    private readonly EfAccountRepository _accounts;
    private readonly EfTransactionRepository _transactions;
    private readonly IHistoricalPriceService _prices;
    private readonly IStockRepository _stocks;
    private readonly InvestSuiteDbContext _db;

    public TradeController(
        EfAccountRepository accounts,
        EfTransactionRepository transactions,
        IHistoricalPriceService prices,
        IStockRepository stocks,
        InvestSuiteDbContext db)
    {
        _accounts = accounts;
        _transactions = transactions;
        _prices = prices;
        _stocks = stocks;
        _db = db;
    }

    [HttpPost("buy")]
    public ActionResult<object> Buy(string accountId, [FromBody] TradeRequest request)
    {
        var account = _accounts.GetEntityById(accountId);
        if (account is null)
            return NotFound(new ErrorResponse($"Unknown account: {accountId}"));

        if (_stocks.GetStock(request.Symbol) is null)
            return BadRequest(new ErrorResponse($"Unknown stock: {request.Symbol}"));

        if (!DateOnly.TryParse(request.Date, out var date))
            return BadRequest(new ErrorResponse("Invalid date format"));

        var price = _prices.GetClosingPrice(request.Symbol, date);
        if (!price.HasValue)
            return BadRequest(new ErrorResponse($"No price data for {request.Symbol} on {request.Date}"));

        if (request.Amount <= 0)
            return BadRequest(new ErrorResponse("Amount must be positive"));

        if (request.Amount > account.CashBalance)
            return BadRequest(new ErrorResponse($"Insufficient cash: €{account.CashBalance:N2} available"));

        var shares = Math.Round(request.Amount / price.Value, 6);

        var tx = new TransactionEntity
        {
            Id = $"tx-{Guid.NewGuid():N}"[..12],
            AccountId = accountId,
            Type = "Buy",
            Symbol = request.Symbol,
            Shares = shares,
            PricePerShare = price.Value,
            Amount = request.Amount,
            ExecutedAt = date.ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc)
        };

        _transactions.AddTransaction(tx);
        account.CashBalance -= request.Amount;
        _db.SaveChanges();

        return Ok(new
        {
            transaction_id = tx.Id,
            symbol = request.Symbol,
            shares,
            price_per_share = price.Value,
            amount = request.Amount,
            remaining_cash = account.CashBalance
        });
    }

    [HttpPost("sell")]
    public ActionResult<object> Sell(string accountId, [FromBody] SellRequest request)
    {
        var account = _accounts.GetEntityById(accountId);
        if (account is null)
            return NotFound(new ErrorResponse($"Unknown account: {accountId}"));

        if (_stocks.GetStock(request.Symbol) is null)
            return BadRequest(new ErrorResponse($"Unknown stock: {request.Symbol}"));

        if (!DateOnly.TryParse(request.Date, out var date))
            return BadRequest(new ErrorResponse("Invalid date format"));

        var price = _prices.GetClosingPrice(request.Symbol, date);
        if (!price.HasValue)
            return BadRequest(new ErrorResponse($"No price data for {request.Symbol} on {request.Date}"));

        if (request.Shares <= 0)
            return BadRequest(new ErrorResponse("Shares must be positive"));

        var holdingsCalc = new HoldingsCalculator(_transactions, _prices, _stocks);
        var holdings = holdingsCalc.ComputeHoldings(accountId, date, 1000m);
        var holding = holdings.FirstOrDefault(h => h.Symbol == request.Symbol);

        if (holding is null || holding.Shares < request.Shares)
        {
            var owned = holding?.Shares ?? 0m;
            return BadRequest(new ErrorResponse(
                $"Insufficient shares: you own {owned:N6} shares of {request.Symbol}"));
        }

        var amount = Math.Round(request.Shares * price.Value, 2);

        var tx = new TransactionEntity
        {
            Id = $"tx-{Guid.NewGuid():N}"[..12],
            AccountId = accountId,
            Type = "Sell",
            Symbol = request.Symbol,
            Shares = request.Shares,
            PricePerShare = price.Value,
            Amount = amount,
            ExecutedAt = date.ToDateTime(new TimeOnly(14, 30), DateTimeKind.Utc)
        };

        _transactions.AddTransaction(tx);
        account.CashBalance += amount;
        _db.SaveChanges();

        return Ok(new
        {
            transaction_id = tx.Id,
            symbol = request.Symbol,
            shares = request.Shares,
            price_per_share = price.Value,
            amount,
            remaining_cash = account.CashBalance
        });
    }
}

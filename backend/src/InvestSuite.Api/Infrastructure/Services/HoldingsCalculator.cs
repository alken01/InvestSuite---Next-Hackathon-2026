using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Services;

public sealed class HoldingsCalculator
{
    private readonly EfTransactionRepository _transactions;
    private readonly IHistoricalPriceService _prices;
    private readonly IStockRepository _stocks;

    public HoldingsCalculator(
        EfTransactionRepository transactions,
        IHistoricalPriceService prices,
        IStockRepository stocks)
    {
        _transactions = transactions;
        _prices = prices;
        _stocks = stocks;
    }

    public IReadOnlyList<Holding> ComputeHoldings(string accountId, DateOnly asOfDate, decimal initialDeposit)
    {
        var txs = _transactions.GetAllUpToDate(accountId, asOfDate);

        // Aggregate: symbol → (totalShares, totalCost)
        var positions = new Dictionary<string, (decimal Shares, decimal Cost)>();
        var cashSpent = 0m;

        foreach (var tx in txs)
        {
            if (tx.Symbol is null) continue;

            if (tx.Type == TransactionType.Buy)
            {
                if (!positions.ContainsKey(tx.Symbol))
                    positions[tx.Symbol] = (0m, 0m);

                var pos = positions[tx.Symbol];
                var shares = tx.Shares ?? 0m;
                positions[tx.Symbol] = (pos.Shares + shares, pos.Cost + tx.Amount);
                cashSpent += tx.Amount;
            }
            else if (tx.Type == TransactionType.Sell)
            {
                if (!positions.ContainsKey(tx.Symbol)) continue;
                var pos = positions[tx.Symbol];
                var sharesToSell = tx.Shares ?? 0m;
                var costReduction = pos.Shares > 0 ? sharesToSell * (pos.Cost / pos.Shares) : 0m;
                positions[tx.Symbol] = (pos.Shares - sharesToSell, pos.Cost - costReduction);
                cashSpent -= tx.Amount;
            }
        }

        var holdings = new List<Holding>();
        foreach (var (symbol, pos) in positions)
        {
            if (pos.Shares <= 0) continue;

            var currentPrice = _prices.GetClosingPrice(symbol, asOfDate);
            if (!currentPrice.HasValue) continue;

            var avgCost = pos.Cost / pos.Shares;
            var value = pos.Shares * currentPrice.Value;
            var returnPct = avgCost > 0
                ? Math.Round((currentPrice.Value - avgCost) / avgCost * 100, 1)
                : 0m;

            var stockDetail = _stocks.GetStock(symbol);
            holdings.Add(new Holding(
                Symbol: symbol,
                Name: stockDetail?.Name ?? symbol,
                Type: HoldingType.Stock,
                Shares: pos.Shares,
                AverageCost: Math.Round(avgCost, 2),
                CurrentPrice: currentPrice.Value,
                Value: Math.Round(value, 0),
                ReturnPct: returnPct
            ));
        }

        // Add remaining cash
        var remainingCash = initialDeposit - cashSpent;
        if (remainingCash > 0)
        {
            holdings.Add(new Holding(
                Symbol: "CASH",
                Name: "Cash",
                Type: HoldingType.Cash,
                Shares: 0m,
                AverageCost: 0m,
                CurrentPrice: 0m,
                Value: Math.Round(remainingCash, 0),
                ReturnPct: null
            ));
        }

        return holdings.OrderByDescending(h => h.Value).ToList();
    }
}

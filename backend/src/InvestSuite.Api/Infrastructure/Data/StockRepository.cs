using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class StockRepository : IStockRepository
{
    private static readonly Dictionary<string, StockDetail> Stocks = new()
    {
        ["ASML"] = new StockDetail(
            Name: "ASML",
            Price: 745.20m,
            ChangeTodayPct: 1.2m,
            Description: "Makes the machines that make computer chips. Near-monopoly.",
            MarketCapB: 301m,
            PeRatio: 35.2m,
            SectorAvgPe: 28m,
            DividendYieldPct: 0.7m,
            AnalystBuy: 18, AnalystHold: 4, AnalystSell: 1,
            AnalystConsensus: "Strong Buy",
            AnalystTargetEur: 820m,
            ScoreGrowth: 5, ScoreHealth: 4, ScoreStability: 3),

        ["ABI"] = new StockDetail(
            Name: "AB InBev",
            Price: 54.75m,
            ChangeTodayPct: -0.4m,
            Description: "World's biggest brewer — Jupiler, Stella Artois, Corona.",
            MarketCapB: 108m,
            PeRatio: 18.5m,
            SectorAvgPe: 20m,
            DividendYieldPct: 1.4m,
            AnalystBuy: 12, AnalystHold: 8, AnalystSell: 2,
            AnalystConsensus: "Buy",
            AnalystTargetEur: 62m,
            ScoreGrowth: 3, ScoreHealth: 3, ScoreStability: 4),

        ["KBC"] = new StockDetail(
            Name: "KBC Group",
            Price: 71.80m,
            ChangeTodayPct: 0.3m,
            Description: "Belgian bank-insurer. Strong in Belgium and Central Europe.",
            MarketCapB: 30m,
            PeRatio: 8.2m,
            SectorAvgPe: 10m,
            DividendYieldPct: 5.1m,
            AnalystBuy: 10, AnalystHold: 6, AnalystSell: 1,
            AnalystConsensus: "Buy",
            AnalystTargetEur: 82m,
            ScoreGrowth: 3, ScoreHealth: 4, ScoreStability: 4),

        ["SAP"] = new StockDetail(
            Name: "SAP",
            Price: 198.60m,
            ChangeTodayPct: 0.8m,
            Description: "German enterprise software giant. Cloud transition driving growth.",
            MarketCapB: 245m,
            PeRatio: 42.1m,
            SectorAvgPe: 35m,
            DividendYieldPct: 0.9m,
            AnalystBuy: 20, AnalystHold: 5, AnalystSell: 0,
            AnalystConsensus: "Strong Buy",
            AnalystTargetEur: 230m,
            ScoreGrowth: 5, ScoreHealth: 4, ScoreStability: 4),

        ["LVMH"] = new StockDetail(
            Name: "LVMH",
            Price: 768.67m,
            ChangeTodayPct: -0.6m,
            Description: "French luxury conglomerate — Louis Vuitton, Dior, Hennessy.",
            MarketCapB: 385m,
            PeRatio: 24.3m,
            SectorAvgPe: 22m,
            DividendYieldPct: 1.7m,
            AnalystBuy: 22, AnalystHold: 3, AnalystSell: 1,
            AnalystConsensus: "Strong Buy",
            AnalystTargetEur: 850m,
            ScoreGrowth: 4, ScoreHealth: 5, ScoreStability: 4),

        ["TTE"] = new StockDetail(
            Name: "TotalEnergies",
            Price: 65.80m,
            ChangeTodayPct: 0.5m,
            Description: "French oil & gas major transitioning to renewables.",
            MarketCapB: 155m,
            PeRatio: 7.8m,
            SectorAvgPe: 9m,
            DividendYieldPct: 5.4m,
            AnalystBuy: 14, AnalystHold: 7, AnalystSell: 2,
            AnalystConsensus: "Buy",
            AnalystTargetEur: 72m,
            ScoreGrowth: 3, ScoreHealth: 4, ScoreStability: 3),

        ["NOVO"] = new StockDetail(
            Name: "Novo Nordisk",
            Price: 96.85m,
            ChangeTodayPct: 1.8m,
            Description: "Danish pharma leader in diabetes and weight-loss drugs.",
            MarketCapB: 430m,
            PeRatio: 38.5m,
            SectorAvgPe: 25m,
            DividendYieldPct: 1.1m,
            AnalystBuy: 24, AnalystHold: 4, AnalystSell: 0,
            AnalystConsensus: "Strong Buy",
            AnalystTargetEur: 115m,
            ScoreGrowth: 5, ScoreHealth: 5, ScoreStability: 4),

        ["UNA"] = new StockDetail(
            Name: "Unilever",
            Price: 50.64m,
            ChangeTodayPct: 0.2m,
            Description: "Consumer goods giant — Dove, Hellmann's, Ben & Jerry's.",
            MarketCapB: 128m,
            PeRatio: 19.2m,
            SectorAvgPe: 21m,
            DividendYieldPct: 3.3m,
            AnalystBuy: 11, AnalystHold: 10, AnalystSell: 2,
            AnalystConsensus: "Hold",
            AnalystTargetEur: 53m,
            ScoreGrowth: 3, ScoreHealth: 4, ScoreStability: 5)
    };

    public StockDetail? GetStock(string symbol) =>
        Stocks.TryGetValue(symbol, out var stock) ? stock : null;

    public IReadOnlyDictionary<string, StockDetail> GetAllStocks() => Stocks;
}

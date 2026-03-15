namespace InvestSuite.Api.Models;

public record StockDetail(
    string Name,
    decimal Price,
    decimal ChangeTodayPct,
    string Description,
    decimal MarketCapB,
    decimal PeRatio,
    decimal SectorAvgPe,
    decimal DividendYieldPct,
    int AnalystBuy,
    int AnalystHold,
    int AnalystSell,
    string AnalystConsensus,
    decimal AnalystTargetEur,
    int ScoreGrowth,
    int ScoreHealth,
    int ScoreStability
);

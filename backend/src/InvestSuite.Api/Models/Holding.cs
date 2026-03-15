namespace InvestSuite.Api.Models;

public record Holding(
    string Symbol,
    string Name,
    HoldingType Type,
    decimal Shares,
    decimal AverageCost,
    decimal CurrentPrice,
    decimal Value,
    decimal? ReturnPct
);

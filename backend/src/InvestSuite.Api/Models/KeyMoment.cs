namespace InvestSuite.Api.Models;

public record KeyMoment(
    int Id,
    DateOnly Date,
    string Title,
    string Description,
    string[] AffectedStocks,
    string Emotion,
    string Volatility
);

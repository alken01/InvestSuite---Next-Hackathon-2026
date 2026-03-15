namespace InvestSuite.Api.Models;

public record ContextSignals(
    string AccountId,
    string Scenario,
    int SessionCount30d,
    int DaysSinceLastSession,
    decimal? PortfolioChange7d,
    decimal? PortfolioChangeSinceLastVisit,
    decimal? DividendsReceivedSinceLastVisit,
    string? PendingActions,
    string? MarketVolatility,
    string? TimeOfDay,
    string? LastAction,
    string? SearchQuery,
    string EmotionalStateEstimate
);

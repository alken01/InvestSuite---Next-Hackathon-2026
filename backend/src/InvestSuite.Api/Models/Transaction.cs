namespace InvestSuite.Api.Models;

public record Transaction(
    string Id,
    string AccountId,
    TransactionType Type,
    string? Symbol,
    string? Name,
    decimal? Shares,
    decimal? PricePerShare,
    decimal Amount,
    DateTimeOffset ExecutedAt,
    string? Note = null
);

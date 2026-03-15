using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Claude;

public interface IPromptBuilder
{
    string SystemPrompt { get; }

    string BuildLayoutPayloadPrompt(
        Account account,
        IReadOnlyList<Holding> holdings,
        IReadOnlyList<Transaction> recentTransactions,
        ContextSignals signals,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        string? userQuery = null,
        KeyMoment? activeMoment = null,
        DateOnly? asOfDate = null);
}

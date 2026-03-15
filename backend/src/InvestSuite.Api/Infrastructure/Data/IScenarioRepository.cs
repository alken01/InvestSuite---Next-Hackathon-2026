using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public interface IScenarioRepository
{
    ContextSignals? GetDefaultSignals(string accountId);
    ContextSignals? GetSignals(string accountId, string? scenario);
    IReadOnlyList<string> GetScenariosForAccount(string accountId);
}

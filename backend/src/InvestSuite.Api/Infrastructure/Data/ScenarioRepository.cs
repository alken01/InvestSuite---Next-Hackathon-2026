using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class ScenarioRepository : IScenarioRepository
{
    private static readonly Dictionary<string, List<ContextSignals>> SignalsByAccount = new();

    public ContextSignals? GetDefaultSignals(string accountId)
    {
        return SignalsByAccount.TryGetValue(accountId, out var list) && list.Count > 0
            ? list[0]
            : null;
    }

    public ContextSignals? GetSignals(string accountId, string? scenario)
    {
        if (!SignalsByAccount.TryGetValue(accountId, out var list)) return null;
        if (string.IsNullOrEmpty(scenario)) return list.Count > 0 ? list[0] : null;
        return list.Find(s => s.Scenario == scenario);
    }

    public IReadOnlyList<string> GetScenariosForAccount(string accountId)
    {
        return SignalsByAccount.TryGetValue(accountId, out var list)
            ? list.Select(s => s.Scenario).ToList()
            : [];
    }
}

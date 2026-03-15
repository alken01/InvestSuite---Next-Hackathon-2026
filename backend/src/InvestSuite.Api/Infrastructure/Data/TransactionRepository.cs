using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class TransactionRepositoryMock : ITransactionRepository
{
    private static readonly Dictionary<string, List<Transaction>> TransactionsByAccount = new();

    public IReadOnlyList<Transaction> GetAll(string accountId) =>
        TransactionsByAccount.TryGetValue(accountId, out var txs) ? txs : [];

    public IReadOnlyList<Transaction> GetRecent(string accountId, int count = 10)
    {
        if (!TransactionsByAccount.TryGetValue(accountId, out var txs)) return [];
        return txs.OrderByDescending(t => t.ExecutedAt).Take(count).ToList();
    }
}

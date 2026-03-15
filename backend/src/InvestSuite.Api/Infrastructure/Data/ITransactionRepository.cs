using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public interface ITransactionRepository
{
    IReadOnlyList<Transaction> GetAll(string accountId);
    IReadOnlyList<Transaction> GetRecent(string accountId, int count = 10);
}

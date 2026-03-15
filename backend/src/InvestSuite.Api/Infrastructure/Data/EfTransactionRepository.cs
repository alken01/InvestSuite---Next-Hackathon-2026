using InvestSuite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Infrastructure.Data;

public sealed class EfTransactionRepository : ITransactionRepository
{
    private readonly InvestSuiteDbContext _db;

    public EfTransactionRepository(InvestSuiteDbContext db) => _db = db;

    public IReadOnlyList<Transaction> GetAll(string accountId)
    {
        return _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .AsEnumerable()
            .OrderBy(t => t.ExecutedAt)
            .Select(t => ToModel(t))
            .ToList();
    }

    public IReadOnlyList<Transaction> GetRecent(string accountId, int count = 10)
    {
        return _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .AsEnumerable()
            .OrderByDescending(t => t.ExecutedAt)
            .Take(count)
            .Select(t => ToModel(t))
            .ToList();
    }

    public IReadOnlyList<Transaction> GetAllUpToDate(string accountId, DateOnly asOfDate)
    {
        var cutoff = new DateTimeOffset(asOfDate.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
        return _db.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .AsEnumerable()
            .Where(t => t.ExecutedAt <= cutoff)
            .OrderBy(t => t.ExecutedAt)
            .Select(t => ToModel(t))
            .ToList();
    }

    public void AddTransaction(TransactionEntity entity)
    {
        _db.Transactions.Add(entity);
    }

    private static Transaction ToModel(TransactionEntity e) =>
        new(
            Id: e.Id,
            AccountId: e.AccountId,
            Type: Enum.Parse<TransactionType>(e.Type, ignoreCase: true),
            Symbol: e.Symbol,
            Name: e.Symbol,
            Shares: e.Shares,
            PricePerShare: e.PricePerShare,
            Amount: e.Amount,
            ExecutedAt: e.ExecutedAt
        );
}

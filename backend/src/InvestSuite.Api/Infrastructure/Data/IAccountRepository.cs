using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public interface IAccountRepository
{
    Account? GetById(string id);
    IReadOnlyDictionary<string, Account> GetAll();
}

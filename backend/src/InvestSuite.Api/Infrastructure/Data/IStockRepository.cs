using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public interface IStockRepository
{
    StockDetail? GetStock(string symbol);
    IReadOnlyDictionary<string, StockDetail> GetAllStocks();
}

namespace InvestSuite.Api.Infrastructure.Services;

public interface IHistoricalPriceService
{
    Task SeedPricesIfNeededAsync(CancellationToken ct = default);
    decimal? GetClosingPrice(string symbol, DateOnly date);
    DateOnly? GetLatestAvailableDate(string symbol);
}

using System.Text.Json;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestSuite.Api.Infrastructure.Services;

public sealed class YahooFinancePriceService : IHistoricalPriceService
{
    private static readonly Dictionary<string, string> TICKER_MAP = new()
    {
        ["ASML"] = "ASML.AS",
        ["ABI"] = "ABI.BR",
        ["KBC"] = "KBC.BR",
        ["SAP"] = "SAP.DE",
        ["LVMH"] = "MC.PA",
        ["TTE"] = "TTE.PA",
        ["NOVO"] = "NOVO-B.CO",
        ["UNA"] = "UNA.AS"
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _http;
    private readonly ILogger<YahooFinancePriceService> _logger;

    public YahooFinancePriceService(
        IServiceScopeFactory scopeFactory,
        HttpClient http,
        ILogger<YahooFinancePriceService> logger)
    {
        _scopeFactory = scopeFactory;
        _http = http;
        _logger = logger;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
    }

    public async Task SeedPricesIfNeededAsync(CancellationToken ct = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvestSuiteDbContext>();

        var existingCount = await db.StockPriceHistory.CountAsync(ct);
        if (existingCount > 100)
        {
            _logger.LogInformation("Price cache has {Count} rows — skipping seed", existingCount);
            return;
        }

        _logger.LogInformation("Seeding historical prices from Yahoo Finance...");

// Use dynamic range to ensure recent data is always available for the hackathon scenarios
        var start = DateTimeOffset.UtcNow.AddYears(-4).ToUnixTimeSeconds();
        var end = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();

        foreach (var (appSymbol, yahooTicker) in TICKER_MAP)
        {
            try
            {
                var prices = await FetchPricesAsync(yahooTicker, start, end, ct);
                foreach (var p in prices)
                {
                    p.Symbol = appSymbol;
                }

                db.StockPriceHistory.AddRange(prices);
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Seeded {Count} prices for {Symbol}", prices.Count, appSymbol);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch prices for {Symbol} — will use fallback", appSymbol);
            }
        }
    }

    public DateOnly? GetLatestAvailableDate(string symbol)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvestSuiteDbContext>();
        return db.StockPriceHistory
            .AsNoTracking()
            .Where(p => p.Symbol == symbol)
            .OrderByDescending(p => p.Date)
            .Select(p => (DateOnly?)p.Date)
            .FirstOrDefault();
    }

    public decimal? GetClosingPrice(string symbol, DateOnly date)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InvestSuiteDbContext>();

        // Exact date or nearest previous trading day (up to 5 days back)
        var minDate = date.AddDays(-5);
        var price = db.StockPriceHistory
            .AsNoTracking()
            .Where(p => p.Symbol == symbol && p.Date <= date && p.Date >= minDate)
            .OrderByDescending(p => p.Date)
            .FirstOrDefault();

        // Fallback: if date is a future/simulated date with no recent data, use the latest available price
        if (price is null)
        {
            price = db.StockPriceHistory
                .AsNoTracking()
                .Where(p => p.Symbol == symbol)
                .OrderByDescending(p => p.Date)
                .FirstOrDefault();
        }

        return price?.Close;
    }

    private async Task<List<StockPriceHistory>> FetchPricesAsync(
        string yahooTicker, long start, long end, CancellationToken ct)
    {
        var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{yahooTicker}"
            + $"?period1={start}&period2={end}&interval=1d";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);

        var result = doc.RootElement
            .GetProperty("chart")
            .GetProperty("result")[0];

        var timestamps = result.GetProperty("timestamp").EnumerateArray().ToArray();
        var quote = result.GetProperty("indicators").GetProperty("quote")[0];
        var opens = quote.GetProperty("open").EnumerateArray().ToArray();
        var highs = quote.GetProperty("high").EnumerateArray().ToArray();
        var lows = quote.GetProperty("low").EnumerateArray().ToArray();
        var closes = quote.GetProperty("close").EnumerateArray().ToArray();
        var volumes = quote.GetProperty("volume").EnumerateArray().ToArray();

        var prices = new List<StockPriceHistory>();
        for (var i = 0; i < timestamps.Length; i++)
        {
            if (closes[i].ValueKind == JsonValueKind.Null) continue;

            var dt = DateTimeOffset.FromUnixTimeSeconds(timestamps[i].GetInt64());
            prices.Add(new StockPriceHistory
            {
                Date = DateOnly.FromDateTime(dt.UtcDateTime),
                Open = GetDecimalSafe(opens[i]),
                High = GetDecimalSafe(highs[i]),
                Low = GetDecimalSafe(lows[i]),
                Close = GetDecimalSafe(closes[i]),
                Volume = volumes[i].ValueKind == JsonValueKind.Null ? 0 : volumes[i].GetInt64()
            });
        }

        return prices;
    }

    private static decimal GetDecimalSafe(JsonElement el) =>
        el.ValueKind == JsonValueKind.Null ? 0m : (decimal)el.GetDouble();
}

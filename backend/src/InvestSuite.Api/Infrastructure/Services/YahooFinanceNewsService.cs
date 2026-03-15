using System.Text.Json;

namespace InvestSuite.Api.Infrastructure.Services;

public sealed class YahooFinanceNewsService : INewsService
{
    private static readonly Dictionary<string, string> TICKER_MAP = new()
    {
        ["ASML"] = "ASML.AS",
        ["ABI"]  = "ABI.BR",
        ["KBC"]  = "KBC.BR",
        ["SAP"]  = "SAP.DE",
        ["LVMH"] = "MC.PA",
        ["TTE"]  = "TTE.PA",
        ["NOVO"] = "NOVO-B.CO",
        ["UNA"]  = "UNA.AS",
    };

    private static readonly string[] POSITIVE_WORDS =
        ["surge", "rise", "gain", "profit", "beat", "record", "strong", "growth", "rally", "upgrade", "exceed", "boom"];

    private static readonly string[] NEGATIVE_WORDS =
        ["fall", "drop", "decline", "loss", "miss", "warn", "weak", "cut", "sell-off", "downgrade", "slump", "plunge", "concern", "risk"];

    private readonly HttpClient _http;
    private readonly ILogger<YahooFinanceNewsService> _logger;

    public YahooFinanceNewsService(HttpClient http, ILogger<YahooFinanceNewsService> logger)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
        _logger = logger;
    }

    public async Task<IReadOnlyList<NewsItem>> GetNewsForSymbolsAsync(
        IEnumerable<string> symbols,
        int maxItems = 5,
        CancellationToken ct = default)
    {
        var results = new List<NewsItem>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var symbol in symbols)
        {
            if (!TICKER_MAP.TryGetValue(symbol, out var yahooTicker)) continue;

            try
            {
                var items = await FetchNewsAsync(yahooTicker, symbol, ct);
                foreach (var item in items)
                {
                    if (seen.Add(item.Headline))
                        results.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch news for {Symbol} — skipping", symbol);
            }
        }

        return results
            .OrderByDescending(n => n.PublishedAt)
            .Take(maxItems)
            .ToList();
    }

    private async Task<IReadOnlyList<NewsItem>> FetchNewsAsync(
        string yahooTicker, string appSymbol, CancellationToken ct)
    {
        var url = $"https://query1.finance.yahoo.com/v1/finance/search"
            + $"?q={Uri.EscapeDataString(yahooTicker)}&newsCount=5"
            + "&enableNavLinks=false&enableEnhancedTrivialQuery=true";

        var response = await _http.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("news", out var newsArr)) return [];

        var items = new List<NewsItem>();
        foreach (var article in newsArr.EnumerateArray())
        {
            var title = article.TryGetProperty("title", out var t) ? t.GetString() : null;
            var publisher = article.TryGetProperty("publisher", out var p) ? p.GetString() : null;
            var publishedAt = article.TryGetProperty("providerPublishTime", out var ts)
                ? DateTimeOffset.FromUnixTimeSeconds(ts.GetInt64())
                : DateTimeOffset.UtcNow;

            if (string.IsNullOrWhiteSpace(title)) continue;

            items.Add(new NewsItem(
                Headline: title,
                Source: publisher ?? "Yahoo Finance",
                PublishedAt: publishedAt,
                RelatedSymbol: appSymbol,
                Sentiment: ClassifySentiment(title)));
        }

        return items;
    }

    private static string ClassifySentiment(string headline)
    {
        var h = headline.ToLowerInvariant();
        if (POSITIVE_WORDS.Any(h.Contains)) return "positive";
        if (NEGATIVE_WORDS.Any(h.Contains)) return "negative";
        return "neutral";
    }
}

namespace InvestSuite.Api.Infrastructure.Services;

public record NewsItem(
    string Headline,
    string Source,
    DateTimeOffset PublishedAt,
    string? RelatedSymbol,
    string Sentiment);

public interface INewsService
{
    Task<IReadOnlyList<NewsItem>> GetNewsForSymbolsAsync(
        IEnumerable<string> symbols,
        int maxItems = 5,
        CancellationToken ct = default);
}

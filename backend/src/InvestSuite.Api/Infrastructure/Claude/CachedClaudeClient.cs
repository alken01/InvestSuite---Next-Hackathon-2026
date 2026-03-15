using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace InvestSuite.Api.Infrastructure.Claude;

public sealed class CachedClaudeClient : IClaudeClient
{
    private static readonly TimeSpan CACHE_TTL = TimeSpan.FromMinutes(10);

    private readonly ClaudeClient _inner;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedClaudeClient> _logger;

    public CachedClaudeClient(
        ClaudeClient inner,
        IMemoryCache cache,
        ILogger<CachedClaudeClient> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<T> GenerateSimpleAsync<T>(
        string prompt,
        CancellationToken ct = default) where T : class
    {
        var cacheKey = BuildCacheKey<T>(prompt);

        if (_cache.TryGetValue(cacheKey, out T? cached) && cached is not null)
        {
            _logger.LogInformation("Cache hit for {Type} (key {Key})", typeof(T).Name, cacheKey[..12]);
            return cached;
        }

        _logger.LogInformation("Cache miss for {Type}, calling Claude API", typeof(T).Name);
        var result = await _inner.GenerateSimpleAsync<T>(prompt, ct);

        _cache.Set(cacheKey, result, CACHE_TTL);
        return result;
    }

    private static string BuildCacheKey<T>(string prompt)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(prompt));
        var hash = Convert.ToHexStringLower(bytes);
        return $"claude:{typeof(T).Name}:{hash}";
    }
}

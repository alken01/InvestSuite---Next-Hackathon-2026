namespace InvestSuite.Api.Infrastructure.Claude;

public interface IClaudeClient
{
    Task<T> GenerateSimpleAsync<T>(string prompt, CancellationToken ct = default) where T : class;
}

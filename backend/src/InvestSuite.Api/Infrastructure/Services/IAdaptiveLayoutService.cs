using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Services;

public interface IAdaptiveLayoutService
{
    LayoutPayload? BuildContextLayout(string accountId, string? scenario = null, DateOnly? asOfDate = null);

    Task<LayoutPayload> GenerateLayoutPayloadAsync(
        string accountId, string? userQuery = null,
        string? scenario = null, DateOnly? asOfDate = null,
        CancellationToken ct = default);

    IReadOnlyList<AccountSummary> GetAccounts();
}

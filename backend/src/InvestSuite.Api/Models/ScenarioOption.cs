using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record AccountSummary(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("experience_level")] ExperienceLevel ExperienceLevel,
    [property: JsonPropertyName("portfolio_total")] decimal PortfolioTotal,
    [property: JsonPropertyName("scenarios")] List<string> Scenarios
);

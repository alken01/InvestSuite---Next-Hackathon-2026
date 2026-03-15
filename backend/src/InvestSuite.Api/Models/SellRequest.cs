using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record SellRequest(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("shares")] decimal Shares,
    [property: JsonPropertyName("date")] string Date
);

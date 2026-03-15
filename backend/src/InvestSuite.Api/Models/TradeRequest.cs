using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record TradeRequest(
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("date")] string Date
);

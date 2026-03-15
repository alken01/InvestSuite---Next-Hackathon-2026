using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record CreateAccountRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("experience_level")] string? ExperienceLevel = null,
    [property: JsonPropertyName("risk_profile")] string? RiskProfile = null,
    [property: JsonPropertyName("personality")] string? Personality = null,
    [property: JsonPropertyName("clerk_user_id")] string? ClerkUserId = null
);

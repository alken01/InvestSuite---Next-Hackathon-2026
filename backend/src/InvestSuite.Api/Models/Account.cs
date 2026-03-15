namespace InvestSuite.Api.Models;

public record Account(
    string Id,
    string FirstName,
    string LastName,
    string Email,
    DateOnly DateOfBirth,
    DateOnly AccountOpened,
    RiskProfile RiskProfile,
    ExperienceLevel ExperienceLevel,
    string Personality,
    string CheckFrequency
);

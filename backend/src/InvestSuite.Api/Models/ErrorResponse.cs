namespace InvestSuite.Api.Models;

public sealed record ErrorResponse(string Error, string? Detail = null);

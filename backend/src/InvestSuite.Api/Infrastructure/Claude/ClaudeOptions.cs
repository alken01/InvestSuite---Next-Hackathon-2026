namespace InvestSuite.Api.Infrastructure.Claude;

public sealed class ClaudeOptions
{
    public const string SECTION_NAME = "Claude";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string CliModel { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
}

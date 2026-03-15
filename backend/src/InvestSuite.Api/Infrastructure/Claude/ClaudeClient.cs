using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;

namespace InvestSuite.Api.Infrastructure.Claude;

public sealed class ClaudeClient : IClaudeClient
{
    private const string API_KEY_HEADER = "x-api-key";
    private const string ANTHROPIC_VERSION_HEADER = "anthropic-version";
    private const string API_VERSION = "2023-06-01";
    private const string MESSAGES_ENDPOINT = "/v1/messages";
    private const string TEXT_BLOCK_TYPE = "text";

    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILogger<ClaudeClient> _logger;

    public ClaudeClient(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        IPromptBuilder promptBuilder,
        ILogger<ClaudeClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _promptBuilder = promptBuilder;
        _logger = logger;
    }

    public async Task<T> GenerateSimpleAsync<T>(
        string prompt,
        CancellationToken ct = default) where T : class
    {
        var systemPrompt = _promptBuilder.SystemPrompt;

        var messages = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = prompt
            },
            new JsonObject
            {
                ["role"] = "assistant",
                ["content"] = "{"
            }
        };

        var responseBody = await CallClaudeApiAsync(systemPrompt, messages, ct);
        var contentBlocks = responseBody["content"]?.AsArray()
            ?? throw new InvalidOperationException("Empty response from Claude");

        return ExtractJson<T>(contentBlocks);
    }

    private async Task<JsonObject> CallClaudeApiAsync(
        string systemPrompt,
        JsonArray messages,
        CancellationToken ct)
    {
        var request = BuildApiRequest(systemPrompt, messages);

        var requestBody = await request.Content!.ReadAsStringAsync(ct);
        _logger.LogInformation("Claude API request: {RequestBody}", requestBody);

        var response = await _httpClient.SendAsync(request, ct);
        var responseJson = await response.Content.ReadAsStringAsync(ct);

        _logger.LogInformation("Claude API response ({Status}): {ResponseBody}", response.StatusCode, responseJson);

        await EnsureSuccessAsync(response, ct);

        return JsonNode.Parse(responseJson)?.AsObject()
            ?? throw new InvalidOperationException("Failed to parse Claude API response");
    }

    private HttpRequestMessage BuildApiRequest(string systemPrompt, JsonArray messages)
    {
        var systemBlocks = new JsonArray
        {
            new JsonObject
            {
                ["type"] = "text",
                ["text"] = systemPrompt,
                ["cache_control"] = new JsonObject { ["type"] = "ephemeral" }
            }
        };

        var body = new JsonObject
        {
            ["model"] = _options.Model,
            ["max_tokens"] = _options.MaxTokens,
            ["system"] = systemBlocks,
            ["messages"] = JsonNode.Parse(messages.ToJsonString())
        };

        var request = new HttpRequestMessage(HttpMethod.Post, MESSAGES_ENDPOINT)
        {
            Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json")
        };
        request.Headers.Add(API_KEY_HEADER, _options.ApiKey);
        request.Headers.Add(ANTHROPIC_VERSION_HEADER, API_VERSION);
        return request;
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;
        var errorBody = await response.Content.ReadAsStringAsync(ct);
        _logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, errorBody);
        throw new HttpRequestException($"Claude API returned {response.StatusCode}: {errorBody}");
    }

    private static T ExtractJson<T>(JsonArray contentBlocks) where T : class
    {
        foreach (var block in contentBlocks)
        {
            if (block?["type"]?.GetValue<string>() != TEXT_BLOCK_TYPE) continue;

            var text = "{" + block["text"]!.GetValue<string>();
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonStr = text[jsonStart..(jsonEnd + 1)];
                return JsonSerializer.Deserialize<T>(jsonStr)
                    ?? throw new InvalidOperationException($"Failed to deserialize {typeof(T).Name}");
            }
        }

        throw new InvalidOperationException($"No JSON found in Claude response for {typeof(T).Name}");
    }
}

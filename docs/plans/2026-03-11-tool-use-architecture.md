# Tool Use Architecture — Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Replace hardcoded prompt-based widget catalog with Claude tool_use, so Claude programmatically discovers available widgets and fetches data on-demand — making the system truly dynamic and extensible.

**Architecture:** Define 5 tools (get_available_widgets, get_user_profile, get_portfolio, get_context_signals, get_market_data) in the Claude API call. The .NET backend resolves tool calls locally using existing repositories. Claude calls whichever tools it needs, gets results back, then generates the UILayout. Widget manifests live as JSON files next to each React component — adding a widget just means adding a manifest + component.

**Tech Stack:** .NET 10 (backend tool_use loop), Next.js (widget manifests + API), Claude API tool_use

---

## Task 1: Create Widget Manifest Files

**Files:**
- Create: `components/widgets/manifests/portfolio_summary.json`
- Create: `components/widgets/manifests/stock_card.json`
- Create: `components/widgets/manifests/market_pulse.json`
- Create: `components/widgets/manifests/narrative_card.json`
- Create: `components/widgets/manifests/historical_context.json`
- Create: `components/widgets/manifests/insight_card.json`
- Create: `components/widgets/manifests/welcome_back_card.json`

**Step 1: Create manifest directory and all 7 files**

Each manifest describes the widget type, purpose, and props schema so Claude knows how to use it.

`components/widgets/manifests/portfolio_summary.json`:
```json
{
  "type": "portfolio_summary",
  "description": "Overview of the investor's entire portfolio — total value, overall return, and holdings breakdown. Use this when the investor should see their full portfolio state.",
  "propsSchema": {
    "holdings": "Holding[] — array of { name, description, type, value, returnPct }",
    "totalValue": "number — total portfolio value in EUR",
    "totalChangePct": "number | null — percentage change over 7 days",
    "changeSinceLastVisit": "number | null — percentage change since last session"
  }
}
```

`components/widgets/manifests/stock_card.json`:
```json
{
  "type": "stock_card",
  "description": "Detail card for a single stock or ETF holding. Shows price, return, and for expert users: P/E ratio, analyst consensus, and health scores. Use when highlighting a specific holding.",
  "propsSchema": {
    "name": "string — stock/ETF name",
    "description": "string — what the company does",
    "type": "\"Stock\" | \"ETF\"",
    "value": "number — current holding value in EUR",
    "returnPct": "number — total return percentage",
    "price": "number (optional) — current share price",
    "changeTodayPct": "number (optional) — today's price change %",
    "peRatio": "number (optional) — price-to-earnings ratio",
    "sectorAvgPe": "number (optional) — sector average P/E for comparison",
    "dividendYieldPct": "number (optional) — annual dividend yield %",
    "analystConsensus": "string (optional) — e.g. 'Strong Buy'",
    "analystTarget": "number (optional) — analyst target price EUR",
    "scores": "{ growth: 1-5, health: 1-5, stability: 1-5 } (optional)"
  }
}
```

`components/widgets/manifests/market_pulse.json`:
```json
{
  "type": "market_pulse",
  "description": "Current market conditions at a glance — volatility level, summary, and sector performance. Use when market context matters for the investor's state.",
  "propsSchema": {
    "volatility": "\"Low\" | \"Medium\" | \"High\"",
    "summary": "string — one-paragraph market summary you write",
    "sectorHighlights": "array of { sector: string, changePct: number } (optional) — key sector movements"
  }
}
```

`components/widgets/manifests/narrative_card.json`:
```json
{
  "type": "narrative_card",
  "description": "A personalized message from the AI to the investor. Adapts tone to mood — calming when anxious, concise when focused, welcoming when detached. Use to set emotional context.",
  "propsSchema": {
    "title": "string — short heading",
    "message": "string — the personalized message you write"
  }
}
```

`components/widgets/manifests/historical_context.json`:
```json
{
  "type": "historical_context",
  "description": "Shows that similar market events have happened before and markets recovered. Use during high volatility or when the investor is anxious to provide perspective.",
  "propsSchema": {
    "event": "string — name of the historical pattern",
    "description": "string — explanation of what's happening",
    "recoveryMonths": "number (optional) — average months to recovery",
    "previousOccurrences": "array of { date: string, dropPct: number, recovery: string } (optional) — past events"
  }
}
```

`components/widgets/manifests/insight_card.json`:
```json
{
  "type": "insight_card",
  "description": "Educational content, actionable tips, or timely information. Use for beginners to learn, or for experts to surface pending actions like expired orders or tax tips.",
  "propsSchema": {
    "title": "string — insight heading",
    "content": "string — the insight text you write",
    "category": "\"education\" | \"action\" | \"tip\""
  }
}
```

`components/widgets/manifests/welcome_back_card.json`:
```json
{
  "type": "welcome_back_card",
  "description": "Catch-up summary for investors returning after extended absence. Shows what happened while they were away — portfolio change, dividends, pending actions. Use when daysSinceLastSession > 14.",
  "propsSchema": {
    "daysSinceLastVisit": "number",
    "portfolioChangePct": "number — portfolio change since last visit",
    "dividendsReceived": "number | null — dividends earned while away",
    "pendingActions": "string | null — e.g. '2 expired orders'",
    "summary": "string — narrative catch-up summary you write"
  }
}
```

**Step 2: Commit**

```bash
git add components/widgets/manifests/
git commit -m "feat: add widget manifest files for dynamic discovery"
```

---

## Task 2: Create GET /api/widgets Endpoint

**Files:**
- Create: `app/api/widgets/route.ts`

**Step 1: Create the endpoint that reads all manifest files**

`app/api/widgets/route.ts`:
```typescript
import { NextResponse } from "next/server";
import fs from "fs";
import path from "path";

export async function GET() {
  const manifestDir = path.join(process.cwd(), "components/widgets/manifests");

  try {
    const files = fs.readdirSync(manifestDir).filter(f => f.endsWith(".json"));
    const manifests = files.map(file => {
      const content = fs.readFileSync(path.join(manifestDir, file), "utf-8");
      return JSON.parse(content);
    });

    return NextResponse.json(manifests);
  } catch (error) {
    console.error("[API] Failed to load widget manifests:", error);
    return NextResponse.json(
      { error: "Failed to load widget manifests" },
      { status: 500 }
    );
  }
}
```

**Step 2: Verify**

```bash
cd C:/Users/thoma/Documents/team-25
bun run build
```

**Step 3: Commit**

```bash
git add app/api/widgets/
git commit -m "feat: add GET /api/widgets endpoint for widget manifest discovery"
```

---

## Task 3: Create WidgetRegistry + ToolDefinitions in .NET Backend

**Files:**
- Create: `backend/src/InvestSuite.Api/Infrastructure/Claude/WidgetRegistry.cs`
- Create: `backend/src/InvestSuite.Api/Infrastructure/Claude/ToolDefinitions.cs`

**Step 1: Create WidgetRegistry**

This fetches widget manifests from the Next.js frontend (or uses embedded fallback).

`backend/src/InvestSuite.Api/Infrastructure/Claude/WidgetRegistry.cs`:
```csharp
using System.Text.Json;

namespace InvestSuite.Api.Infrastructure.Claude;

public class WidgetRegistry
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<WidgetRegistry> _logger;
    private List<WidgetManifest>? _cachedManifests;

    public WidgetRegistry(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<WidgetRegistry> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<List<WidgetManifest>> GetManifestsAsync(CancellationToken ct = default)
    {
        if (_cachedManifests is not null)
            return _cachedManifests;

        var frontendUrl = _config.GetValue<string>("FrontendUrl") ?? "http://localhost:3000";

        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{frontendUrl}/api/widgets", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            _cachedManifests = JsonSerializer.Deserialize<List<WidgetManifest>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? [];

            _logger.LogInformation("Loaded {Count} widget manifests from frontend", _cachedManifests.Count);
            return _cachedManifests;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch widget manifests from frontend, using embedded fallback");
            return GetEmbeddedManifests();
        }
    }

    public void InvalidateCache() => _cachedManifests = null;

    private static List<WidgetManifest> GetEmbeddedManifests() =>
    [
        new("portfolio_summary", "Overview of the investor's entire portfolio — total value, overall return, and holdings breakdown."),
        new("stock_card", "Detail card for a single stock or ETF holding. Shows price, return, and for expert users: P/E ratio, analyst consensus, scores."),
        new("market_pulse", "Current market conditions at a glance — volatility level, summary, and sector performance."),
        new("narrative_card", "A personalized message from the AI to the investor. Adapts tone to mood."),
        new("historical_context", "Shows that similar market events have happened before and markets recovered."),
        new("insight_card", "Educational content, actionable tips, or timely information."),
        new("welcome_back_card", "Catch-up summary for investors returning after extended absence.")
    ];
}

public record WidgetManifest(string Type, string Description, Dictionary<string, string>? PropsSchema = null);
```

**Step 2: Create ToolDefinitions**

Defines the 5 tools that Claude can call, formatted for the Anthropic API.

`backend/src/InvestSuite.Api/Infrastructure/Claude/ToolDefinitions.cs`:
```csharp
using System.Text.Json;
using System.Text.Json.Nodes;

namespace InvestSuite.Api.Infrastructure.Claude;

public static class ToolDefinitions
{
    public static JsonArray BuildToolList(List<WidgetManifest> widgetManifests)
    {
        var tools = new JsonArray
        {
            BuildTool(
                "get_available_widgets",
                "Returns the list of available UI widgets that can be used to compose the dashboard. Each widget has a type, description, and props schema. Call this first to know what widgets you can use.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray()
                }
            ),
            BuildTool(
                "get_user_profile",
                "Returns the investor's profile including name, age, experience level, personality, and check frequency.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["user_id"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The investor's ID (provided in the task)"
                        }
                    },
                    ["required"] = new JsonArray { "user_id" }
                }
            ),
            BuildTool(
                "get_portfolio",
                "Returns the investor's portfolio holdings — each with name, description, type (Stock/ETF/Cash), value, and return percentage.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["user_id"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The investor's ID"
                        }
                    },
                    ["required"] = new JsonArray { "user_id" }
                }
            ),
            BuildTool(
                "get_context_signals",
                "Returns the current session's context signals — session frequency, portfolio changes, market volatility, time of day, emotional state, search queries, and pending actions.",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject
                    {
                        ["user_id"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The investor's ID"
                        },
                        ["scenario_id"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The scenario ID for this session"
                        }
                    },
                    ["required"] = new JsonArray { "user_id", "scenario_id" }
                }
            ),
            BuildTool(
                "get_market_data",
                "Returns current market data including stock details (price, P/E, analyst ratings, scores) and market overview (indices, sector performance).",
                new JsonObject
                {
                    ["type"] = "object",
                    ["properties"] = new JsonObject(),
                    ["required"] = new JsonArray()
                }
            )
        };

        return tools;
    }

    private static JsonObject BuildTool(string name, string description, JsonObject inputSchema)
    {
        return new JsonObject
        {
            ["name"] = name,
            ["description"] = description,
            ["input_schema"] = inputSchema
        };
    }
}
```

**Step 3: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet build
```

**Step 4: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/src/InvestSuite.Api/Infrastructure/Claude/WidgetRegistry.cs backend/src/InvestSuite.Api/Infrastructure/Claude/ToolDefinitions.cs
git commit -m "feat: add WidgetRegistry and ToolDefinitions for Claude tool_use"
```

---

## Task 4: Create ToolResolver

**Files:**
- Create: `backend/src/InvestSuite.Api/Infrastructure/Claude/ToolResolver.cs`

**Step 1: Create ToolResolver**

Resolves tool calls from Claude using the existing data repositories. Returns JSON results.

`backend/src/InvestSuite.Api/Infrastructure/Claude/ToolResolver.cs`:
```csharp
using System.Text.Json;
using System.Text.Json.Nodes;
using InvestSuite.Api.Infrastructure.Data;

namespace InvestSuite.Api.Infrastructure.Claude;

public class ToolResolver
{
    private readonly InvestorRepository _investors;
    private readonly ScenarioRepository _scenarios;
    private readonly StockRepository _stocks;
    private readonly WidgetRegistry _widgetRegistry;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ToolResolver(
        InvestorRepository investors,
        ScenarioRepository scenarios,
        StockRepository stocks,
        WidgetRegistry widgetRegistry)
    {
        _investors = investors;
        _scenarios = scenarios;
        _stocks = stocks;
        _widgetRegistry = widgetRegistry;
    }

    public async Task<string> ResolveAsync(string toolName, JsonElement input, CancellationToken ct = default)
    {
        return toolName switch
        {
            "get_available_widgets" => await ResolveWidgets(ct),
            "get_user_profile" => ResolveUserProfile(input),
            "get_portfolio" => ResolvePortfolio(input),
            "get_context_signals" => ResolveContextSignals(input),
            "get_market_data" => ResolveMarketData(),
            _ => JsonSerializer.Serialize(new { error = $"Unknown tool: {toolName}" })
        };
    }

    private async Task<string> ResolveWidgets(CancellationToken ct)
    {
        var manifests = await _widgetRegistry.GetManifestsAsync(ct);
        return JsonSerializer.Serialize(manifests, JsonOpts);
    }

    private string ResolveUserProfile(JsonElement input)
    {
        var userId = input.GetProperty("user_id").GetString() ?? "";
        var investor = _investors.GetById(userId);
        if (investor is null)
            return JsonSerializer.Serialize(new { error = $"Unknown investor: {userId}" });
        return JsonSerializer.Serialize(investor, JsonOpts);
    }

    private string ResolvePortfolio(JsonElement input)
    {
        var userId = input.GetProperty("user_id").GetString() ?? "";
        var holdings = _stocks.GetPortfolio(userId);
        if (holdings is null)
            return JsonSerializer.Serialize(new { error = $"No portfolio for: {userId}" });
        return JsonSerializer.Serialize(holdings, JsonOpts);
    }

    private string ResolveContextSignals(JsonElement input)
    {
        var scenarioId = input.GetProperty("scenario_id").GetString() ?? "";
        var signals = _scenarios.GetSignals(scenarioId);
        if (signals is null)
            return JsonSerializer.Serialize(new { error = $"Unknown scenario: {scenarioId}" });
        return JsonSerializer.Serialize(signals, JsonOpts);
    }

    private string ResolveMarketData()
    {
        var stocks = _stocks.GetAllStocks();
        return JsonSerializer.Serialize(new
        {
            stockDetails = stocks,
            indices = new[]
            {
                new { name = "Euro Stoxx 50", changePct = -0.3 },
                new { name = "S&P 500", changePct = 0.7 },
                new { name = "AEX", changePct = 0.4 }
            },
            sectors = new[]
            {
                new { sector = "Technology", changePct = 1.8 },
                new { sector = "Healthcare", changePct = -0.5 },
                new { sector = "Financials", changePct = 0.3 },
                new { sector = "Energy", changePct = -1.2 },
                new { sector = "Consumer", changePct = 0.1 }
            }
        }, JsonOpts);
    }
}
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet build
```

**Step 3: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/src/InvestSuite.Api/Infrastructure/Claude/ToolResolver.cs
git commit -m "feat: add ToolResolver for handling Claude tool calls"
```

---

## Task 5: Rewrite ClaudeClient for Tool Use Loop

**Files:**
- Modify: `backend/src/InvestSuite.Api/Infrastructure/Claude/ClaudeClient.cs`

**Step 1: Replace ClaudeClient.cs entirely**

The new ClaudeClient supports the tool_use conversation loop: send initial request → Claude returns tool_use blocks → resolve tools → send results back → Claude returns final text with UILayout.

`backend/src/InvestSuite.Api/Infrastructure/Claude/ClaudeClient.cs`:
```csharp
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using InvestSuite.Api.Models;
using Microsoft.Extensions.Options;

namespace InvestSuite.Api.Infrastructure.Claude;

public class ClaudeClient
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ToolResolver _toolResolver;
    private readonly ILogger<ClaudeClient> _logger;

    private static readonly JsonSerializerOptions SerializeOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions DeserializeOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public ClaudeClient(
        HttpClient httpClient,
        IOptions<ClaudeOptions> options,
        ToolResolver toolResolver,
        ILogger<ClaudeClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _toolResolver = toolResolver;
        _logger = logger;
    }

    public async Task<UILayout> GenerateLayoutAsync(
        string systemPrompt,
        string userPrompt,
        JsonArray tools,
        CancellationToken ct = default)
    {
        var messages = new JsonArray
        {
            new JsonObject
            {
                ["role"] = "user",
                ["content"] = userPrompt
            }
        };

        const int maxIterations = 5;

        for (var i = 0; i < maxIterations; i++)
        {
            _logger.LogInformation("Claude API call iteration {Iteration}", i + 1);

            var responseBody = await CallClaudeApi(systemPrompt, messages, tools, ct);

            var stopReason = responseBody["stop_reason"]?.GetValue<string>();
            var contentBlocks = responseBody["content"]?.AsArray();

            if (contentBlocks is null || contentBlocks.Count == 0)
                throw new InvalidOperationException("Empty response from Claude");

            // Add assistant response to message history
            messages.Add(new JsonObject
            {
                ["role"] = "assistant",
                ["content"] = JsonNode.Parse(contentBlocks.ToJsonString())
            });

            if (stopReason == "end_turn")
            {
                // Claude is done — extract UILayout from text blocks
                return ExtractLayout(contentBlocks);
            }

            if (stopReason == "tool_use")
            {
                // Claude wants to call tools — resolve them
                var toolResults = new JsonArray();

                foreach (var block in contentBlocks)
                {
                    if (block?["type"]?.GetValue<string>() != "tool_use") continue;

                    var toolName = block["name"]!.GetValue<string>();
                    var toolId = block["id"]!.GetValue<string>();
                    var toolInput = block["input"]!;

                    _logger.LogInformation("Resolving tool: {ToolName} (id: {ToolId})", toolName, toolId);

                    var inputElement = JsonSerializer.Deserialize<JsonElement>(toolInput.ToJsonString());
                    var result = await _toolResolver.ResolveAsync(toolName, inputElement, ct);

                    toolResults.Add(new JsonObject
                    {
                        ["type"] = "tool_result",
                        ["tool_use_id"] = toolId,
                        ["content"] = result
                    });
                }

                // Add tool results as user message
                messages.Add(new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = JsonNode.Parse(toolResults.ToJsonString())
                });

                continue;
            }

            // Unexpected stop reason
            _logger.LogWarning("Unexpected stop_reason: {StopReason}", stopReason);
            return ExtractLayout(contentBlocks);
        }

        throw new InvalidOperationException($"Claude did not finish after {maxIterations} tool_use iterations");
    }

    // Keep backward-compatible overload (no tools)
    public async Task<UILayout> GenerateLayoutAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken ct = default)
    {
        return await GenerateLayoutAsync(systemPrompt, userPrompt, [], ct);
    }

    private async Task<JsonObject> CallClaudeApi(
        string systemPrompt,
        JsonArray messages,
        JsonArray tools,
        CancellationToken ct)
    {
        var requestBody = new JsonObject
        {
            ["model"] = _options.Model,
            ["max_tokens"] = _options.MaxTokens,
            ["system"] = systemPrompt,
            ["messages"] = JsonNode.Parse(messages.ToJsonString())
        };

        if (tools.Count > 0)
        {
            requestBody["tools"] = JsonNode.Parse(tools.ToJsonString());
        }

        var json = requestBody.ToJsonString();
        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");

        var response = await _httpClient.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Claude API error {Status}: {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Claude API returned {response.StatusCode}: {errorBody}");
        }

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return JsonNode.Parse(responseJson)?.AsObject()
            ?? throw new InvalidOperationException("Failed to parse Claude API response");
    }

    private static UILayout ExtractLayout(JsonArray contentBlocks)
    {
        foreach (var block in contentBlocks)
        {
            if (block?["type"]?.GetValue<string>() != "text") continue;

            var text = block["text"]!.GetValue<string>();
            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var layoutJson = text[jsonStart..(jsonEnd + 1)];
                return JsonSerializer.Deserialize<UILayout>(layoutJson)
                    ?? throw new InvalidOperationException("Failed to deserialize UILayout");
            }
        }

        throw new InvalidOperationException("No UILayout JSON found in Claude response");
    }
}
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet build
```

**Step 3: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/src/InvestSuite.Api/Infrastructure/Claude/ClaudeClient.cs
git commit -m "feat: rewrite ClaudeClient with tool_use conversation loop"
```

---

## Task 6: Update PromptBuilder — Slim System Prompt

**Files:**
- Modify: `backend/src/InvestSuite.Api/Infrastructure/Claude/PromptBuilder.cs`

**Step 1: Replace PromptBuilder.cs**

The system prompt is now much slimmer — no hardcoded widget catalog (Claude discovers it via tools). The user prompt just tells Claude the scenario to investigate.

`backend/src/InvestSuite.Api/Infrastructure/Claude/PromptBuilder.cs`:
```csharp
namespace InvestSuite.Api.Infrastructure.Claude;

public class PromptBuilder
{
    public string BuildSystemPrompt()
    {
        return """
            You are the "brain" of an adaptive investing interface. Your job is to compose a unique, personalized UI layout by discovering available widgets and fetching relevant data using your tools.

            WORKFLOW:
            1. Call get_available_widgets to discover what UI components exist
            2. Call get_user_profile, get_portfolio, and get_context_signals to understand the investor and their current state
            3. Optionally call get_market_data if market context is relevant
            4. Compose a UILayout selecting 3-6 widgets from the available set, configured with data from your tool calls

            RULES:
            - NO investment advice. Never recommend buying or selling. The investor always decides.
            - NO gamification. No points, badges, streaks.
            - The investor stays in control. Never manipulate or create false urgency.
            - Generate a DIFFERENT layout every time. Vary widgets, order, and narrative.
            - Match tone to the investor's emotional state and experience level.

            EXPERIENCE LEVELS:
            - "beginner": Simple language, fewer metrics, explanatory tone
            - "expert": Technical metrics, data-dense, concise

            MOOD STATES:
            - "calm": Light, exploratory, educational content welcome
            - "anxious": Fewer widgets, calming reassurance, historical perspective
            - "focused": Dense data, multiple detail cards, minimal narrative
            - "detached": Storytelling flow, catch-up sequence, welcome back

            After gathering data via tools, respond with ONLY valid JSON (no markdown, no code fences):
            {
              "narrative": "A one-sentence greeting or contextual headline",
              "mood": "calm" | "anxious" | "focused" | "detached",
              "experience": "beginner" | "expert",
              "widgets": [
                { "type": "<widget_type>", "props": { ... } }
              ]
            }
            """;
    }

    public string BuildUserPrompt(string investorId, string scenarioId)
    {
        return $"""
            Generate an adaptive dashboard for investor "{investorId}" in scenario "{scenarioId}".

            Use your tools to:
            1. Discover available widgets
            2. Fetch the investor's profile and portfolio
            3. Get the context signals for this scenario
            4. Get market data if relevant

            Then compose a unique UILayout. Be creative. Make it different every time.
            """;
    }
}
```

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet build
```

**Step 3: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/src/InvestSuite.Api/Infrastructure/Claude/PromptBuilder.cs
git commit -m "feat: slim down PromptBuilder — Claude discovers widgets via tools"
```

---

## Task 7: Update BrainService + Program.cs

**Files:**
- Modify: `backend/src/InvestSuite.Api/Infrastructure/Services/BrainService.cs`
- Modify: `backend/src/InvestSuite.Api/Program.cs`

**Step 1: Replace BrainService.cs**

BrainService now builds tool definitions and passes them to ClaudeClient. It no longer pre-fetches all data — Claude fetches what it needs via tools.

`backend/src/InvestSuite.Api/Infrastructure/Services/BrainService.cs`:
```csharp
using InvestSuite.Api.Infrastructure.Claude;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Services;

public class BrainService
{
    private readonly ScenarioRepository _scenarios;
    private readonly ClaudeClient _claude;
    private readonly PromptBuilder _promptBuilder;
    private readonly WidgetRegistry _widgetRegistry;

    public BrainService(
        ScenarioRepository scenarios,
        ClaudeClient claude,
        PromptBuilder promptBuilder,
        WidgetRegistry widgetRegistry)
    {
        _scenarios = scenarios;
        _claude = claude;
        _promptBuilder = promptBuilder;
        _widgetRegistry = widgetRegistry;
    }

    public virtual async Task<UILayout> GenerateDashboardAsync(string scenarioId, CancellationToken ct = default)
    {
        var signals = _scenarios.GetSignals(scenarioId)
            ?? throw new ArgumentException($"Unknown scenario: {scenarioId}");

        // Fetch widget manifests for tool definitions
        var manifests = await _widgetRegistry.GetManifestsAsync(ct);

        // Build tools array
        var tools = ToolDefinitions.BuildToolList(manifests);

        // Build prompts — slim now, Claude fetches data via tools
        var systemPrompt = _promptBuilder.BuildSystemPrompt();
        var userPrompt = _promptBuilder.BuildUserPrompt(signals.Investor, scenarioId);

        return await _claude.GenerateLayoutAsync(systemPrompt, userPrompt, tools, ct);
    }
}
```

**Step 2: Update Program.cs — register new services**

Read `backend/src/InvestSuite.Api/Program.cs` first, then add these registrations. Find the services section and add:

After the line `builder.Services.AddSingleton<PromptBuilder>();`, add:
```csharp
builder.Services.AddSingleton<WidgetRegistry>();
builder.Services.AddScoped<ToolResolver>();
```

Also add `IHttpClientFactory` if not already registered — add after the existing `AddHttpClient<ClaudeClient>` block:
```csharp
builder.Services.AddHttpClient();
```

And add a config key for frontend URL in appsettings.json — read first, then add:
```json
"FrontendUrl": "http://localhost:3000"
```

**Step 3: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet build
```

**Step 4: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/src/InvestSuite.Api/Infrastructure/Services/BrainService.cs backend/src/InvestSuite.Api/Program.cs backend/src/InvestSuite.Api/appsettings.json
git commit -m "feat: wire up tool_use in BrainService and register new services"
```

---

## Task 8: Update Next.js Prompt for CLI Fallback

**Files:**
- Modify: `lib/ai/prompt.ts`

**Step 1: Update prompt.ts to dynamically load widget manifests**

For CLI mode (no tool_use), we still embed everything in the prompt — but now we read from manifest files instead of hardcoding.

Read `lib/ai/prompt.ts` first, then replace the `buildSystemPrompt` function:

```typescript
import { Holding, Investor, ContextSignals } from "@/lib/types";
import fs from "fs";
import path from "path";

interface WidgetManifest {
  type: string;
  description: string;
  propsSchema: Record<string, string>;
}

function loadWidgetManifests(): WidgetManifest[] {
  const manifestDir = path.join(process.cwd(), "components/widgets/manifests");
  try {
    const files = fs.readdirSync(manifestDir).filter(f => f.endsWith(".json"));
    return files.map(file => {
      const content = fs.readFileSync(path.join(manifestDir, file), "utf-8");
      return JSON.parse(content);
    });
  } catch {
    return [];
  }
}

export function buildSystemPrompt(): string {
  const manifests = loadWidgetManifests();

  const widgetCatalog = manifests.length > 0
    ? manifests.map((m, i) =>
        `${i + 1}. "${m.type}" — ${m.description}\n   Props: ${JSON.stringify(m.propsSchema)}`
      ).join("\n\n")
    : "No widget manifests found — use your best judgment.";

  return `You are the "brain" of an adaptive investing interface. Your job is to compose a unique, personalized UI layout for an investor based on their profile, portfolio, context signals, and current market conditions.

RULES:
- NO investment advice. Never recommend buying or selling. The investor always decides.
- NO gamification. No points, badges, streaks.
- The investor stays in control. Never manipulate or create false urgency.
- Generate a DIFFERENT layout composition every time. Vary which widgets you include, their order, and the narrative content.
- Match your tone to the investor's emotional state and experience level.

AVAILABLE WIDGETS (pick 3-6 per layout):

${widgetCatalog}

EXPERIENCE LEVELS:
- "beginner": Simple language, fewer metrics, explanatory tone, line charts
- "expert": Technical metrics (P/E, analyst consensus), candlestick style, concise

MOOD STATES (affects density and tone):
- "calm": Light, exploratory, educational content welcome
- "anxious": Fewer widgets, more whitespace, calming reassurance, perspective
- "focused": Dense data, multiple detail cards, minimal narrative
- "detached": Storytelling flow, catch-up sequence, welcome back

RESPONSE FORMAT — Return ONLY valid JSON, no markdown, no code fences:
{
  "narrative": "A one-sentence greeting or contextual headline",
  "mood": "calm" | "anxious" | "focused" | "detached",
  "experience": "beginner" | "expert",
  "widgets": [
    { "type": "<widget_type>", "props": { ... } }
  ]
}

Be creative with narratives. Be specific to the moment. Make every response unique.`;
}

// buildUserPrompt stays the same — keep the existing function unchanged
```

IMPORTANT: Only replace the `buildSystemPrompt` function and add the imports/helpers at the top. Keep `buildUserPrompt` exactly as it is.

**Step 2: Verify build**

```bash
cd C:/Users/thoma/Documents/team-25
bun run build
```

**Step 3: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add lib/ai/prompt.ts
git commit -m "feat: dynamic widget manifest loading in CLI fallback prompt"
```

---

## Task 9: Update Tests

**Files:**
- Modify: `backend/tests/InvestSuite.Tests/Infrastructure/PromptBuilderTests.cs`

**Step 1: Update PromptBuilder tests**

The PromptBuilder API changed — `BuildUserPrompt` now takes `(string investorId, string scenarioId)` instead of the full objects. Update the tests:

```csharp
using FluentAssertions;
using InvestSuite.Api.Infrastructure.Claude;

namespace InvestSuite.Tests.Infrastructure;

public class PromptBuilderTests
{
    private readonly PromptBuilder _builder = new();

    [Fact]
    public void BuildSystemPrompt_ContainsRules()
    {
        var prompt = _builder.BuildSystemPrompt();

        prompt.Should().Contain("NO investment advice");
        prompt.Should().Contain("NO gamification");
    }

    [Fact]
    public void BuildSystemPrompt_ContainsToolWorkflow()
    {
        var prompt = _builder.BuildSystemPrompt();

        prompt.Should().Contain("get_available_widgets");
        prompt.Should().Contain("get_user_profile");
        prompt.Should().Contain("get_portfolio");
        prompt.Should().Contain("get_context_signals");
    }

    [Fact]
    public void BuildSystemPrompt_ContainsMoodStates()
    {
        var prompt = _builder.BuildSystemPrompt();

        prompt.Should().Contain("calm");
        prompt.Should().Contain("anxious");
        prompt.Should().Contain("focused");
        prompt.Should().Contain("detached");
    }

    [Fact]
    public void BuildUserPrompt_IncludesInvestorAndScenario()
    {
        var prompt = _builder.BuildUserPrompt("sophie", "sophie-calm");

        prompt.Should().Contain("sophie");
        prompt.Should().Contain("sophie-calm");
    }
}
```

**Step 2: Run tests**

```bash
cd C:/Users/thoma/Documents/team-25/backend
dotnet test
```

Fix any compilation errors in other test files that reference the old BrainService constructor (it now takes different params). If GetDashboardHandlerTests fails to compile, update it to match the new BrainService constructor or simplify it.

**Step 3: Commit**

```bash
cd C:/Users/thoma/Documents/team-25
git add backend/tests/
git commit -m "test: update tests for tool_use architecture"
```

---

## Task 10: End-to-End Verification

**Step 1: Build everything**

```bash
cd C:/Users/thoma/Documents/team-25
bun run build
cd backend && dotnet build && dotnet test
```

**Step 2: Test CLI fallback mode**

Make sure `.env.local` has `NEXT_PUBLIC_API_URL=` (empty). Start `bun dev`, click a scenario. Check browser console for `[InvestSuite]` logs. Verify widgets render.

**Step 3: Test .NET backend mode (if API key available)**

Set API key in `backend/src/InvestSuite.Api/appsettings.json`. Start both:
```bash
# Terminal 1
cd backend && dotnet run --project src/InvestSuite.Api

# Terminal 2
# Set NEXT_PUBLIC_API_URL=http://localhost:5013 in .env.local
bun dev
```

Check the .NET backend console for tool_use logs:
```
Claude API call iteration 1
Resolving tool: get_available_widgets
Resolving tool: get_user_profile
Resolving tool: get_portfolio
Resolving tool: get_context_signals
Claude API call iteration 2
```

**Step 4: Final commit**

```bash
git add -A
git commit -m "feat: complete tool_use architecture — Claude discovers widgets programmatically"
```

---

## Dependency Graph

```
Task 1 (manifests) ──┬──► Task 2 (GET /api/widgets)
                     │         │
                     │    Task 3 (WidgetRegistry + ToolDefs)
                     │         │
                     │    Task 4 (ToolResolver)
                     │         │
                     │    Task 5 (ClaudeClient rewrite) ← depends on 4
                     │         │
                     │    Task 6 (PromptBuilder slim)
                     │         │
                     │    Task 7 (BrainService + Program.cs) ← depends on 3,4,5,6
                     │
                     └──► Task 8 (Next.js prompt update) ← depends on 1

Task 9 (tests) ← depends on 6,7
Task 10 (e2e) ← depends on all
```

Tasks 2, 3, 4, 6 can run in parallel after Task 1.
Task 5 depends on Task 4.
Task 7 depends on Tasks 3, 4, 5, 6.
Task 8 only depends on Task 1.

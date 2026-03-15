using FluentAssertions;
using InvestSuite.Api.Infrastructure.Claude;
using InvestSuite.Api.Models;

namespace InvestSuite.Tests.Infrastructure;

public class PromptBuilderTests
{
    private static readonly string PromptsPath = Path.Combine(
        AppContext.BaseDirectory, "prompts/system");
    private readonly PromptBuilder _builder = new(PromptsPath);

    private static readonly Account TestAccount = new(
        Id: "test-user",
        FirstName: "Test",
        LastName: "User",
        Email: "test@simulator.local",
        DateOfBirth: new DateOnly(1995, 1, 1),
        AccountOpened: new DateOnly(2025, 8, 1),
        RiskProfile: RiskProfile.Moderate,
        ExperienceLevel: ExperienceLevel.Beginner,
        Personality: "Anxious beginner",
        CheckFrequency: "Several times a day"
    );

    private static readonly List<Holding> TestHoldings =
    [
        new("ASML", "ASML", HoldingType.Stock,
            Shares: 2m, AverageCost: 680m, CurrentPrice: 745.20m,
            Value: 1490m, ReturnPct: 9.6m),
        new("IWDA", "World ETF", HoldingType.Etf,
            Shares: 11m, AverageCost: 112.50m, CurrentPrice: 117.45m,
            Value: 1292m, ReturnPct: 4.4m),
        new("CASH", "Cash", HoldingType.Cash,
            Shares: 0m, AverageCost: 0m, CurrentPrice: 0m,
            Value: 587m, ReturnPct: null)
    ];

    private static readonly List<Transaction> TestTransactions =
    [
        new("t-001", "test-user", TransactionType.Buy,
            "ASML", "ASML", 2m, 680m, 1360m,
            new DateTimeOffset(2025, 9, 5, 11, 0, 0, TimeSpan.FromHours(2)),
            "Read about chip shortage"),
        new("t-002", "test-user", TransactionType.Deposit,
            null, null, null, null, 500m,
            new DateTimeOffset(2025, 9, 1, 10, 0, 0, TimeSpan.FromHours(2)))
    ];

    private static readonly ContextSignals TestSignals = new(
        AccountId: "test-user",
        Scenario: "Calm Tuesday Evening",
        SessionCount30d: 12,
        DaysSinceLastSession: 2,
        PortfolioChange7d: 0.8m,
        PortfolioChangeSinceLastVisit: null,
        DividendsReceivedSinceLastVisit: null,
        PendingActions: null,
        MarketVolatility: "Low",
        TimeOfDay: "20:15",
        LastAction: null,
        SearchQuery: null,
        EmotionalStateEstimate: "Calm / curious"
    );

    private static readonly Dictionary<string, StockDetail> EmptyStocks = [];

    // ── System prompt ────────────────────────────────────────────────

    [Fact]
    public void SystemPrompt_ContainsCoreRules_NoInvestmentAdvice()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("No investment advice");
    }

    [Fact]
    public void SystemPrompt_ContainsCoreRules_NoGamification()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("No gamification");
    }

    [Fact]
    public void SystemPrompt_ContainsMoodStates()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("reassuring");
        systemPrompt.Should().Contain("neutral");
        systemPrompt.Should().Contain("focused");
        systemPrompt.Should().Contain("welcoming");
    }

    [Fact]
    public void SystemPrompt_ContainsExperienceLevels()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("beginner");
        systemPrompt.Should().Contain("experienced");
    }

    [Fact]
    public void SystemPrompt_ContainsResponseFormat()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("tone");
        systemPrompt.Should().Contain("sentiment");
        systemPrompt.Should().Contain("view_mode");
        systemPrompt.Should().Contain("ai_message");
        systemPrompt.Should().Contain("suggestions");
    }

    [Fact]
    public void SystemPrompt_ContainsViewModeRules()
    {
        var systemPrompt = _builder.SystemPrompt;

        systemPrompt.Should().Contain("ambient");
        systemPrompt.Should().Contain("expanded_card");
        systemPrompt.Should().Contain("research");
        systemPrompt.Should().Contain("buy_flow");
    }

    // ── Account profile ───────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsAccountProfile()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("Test User");
        prompt.Should().Contain("Moderate");
        prompt.Should().Contain("Beginner");
    }

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsPortfolioTotal()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("3,369");
    }

    [Fact]
    public void BuildLayoutPayloadPrompt_UsesXmlTags()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("<investor>");
        prompt.Should().Contain("</investor>");
        prompt.Should().Contain("<query>");
    }

    // ── Holdings ──────────────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsHoldings()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("ASML");
        prompt.Should().Contain("IWDA");
        prompt.Should().Contain("Cash");
    }

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsSharesAndValues()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("2sh");
        prompt.Should().Contain("1,490");
    }

    // ── Transactions ──────────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsTransactionHistory()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("Recent:");
        prompt.Should().Contain("Buy");
        prompt.Should().Contain("ASML");
    }

    [Fact]
    public void BuildLayoutPayloadPrompt_WithNoTransactions_SkipsRecentLine()
    {
        var prompt = _builder.BuildLayoutPayloadPrompt(
            TestAccount, TestHoldings, [], TestSignals, EmptyStocks);

        prompt.Should().NotContain("Recent:");
    }

    // ── Context signals ───────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_ContainsEmotionalState()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("Calm / curious");
        prompt.Should().Contain("Low");
    }

    // ── User query ────────────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_WithUserQuery_ContainsQuery()
    {
        var prompt = _builder.BuildLayoutPayloadPrompt(
            TestAccount, TestHoldings, TestTransactions, TestSignals, EmptyStocks,
            "Tell me about ASML");

        prompt.Should().Contain("<query>Tell me about ASML</query>");
    }

    [Fact]
    public void BuildLayoutPayloadPrompt_WithoutUserQuery_ContainsAmbientQuery()
    {
        var prompt = BuildPrompt();

        prompt.Should().Contain("<query>ambient home view</query>");
    }

    // ── Stock details ─────────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_WithStockDetails_IncludesStockData()
    {
        var stocks = new Dictionary<string, StockDetail>
        {
            ["ASML"] = new("ASML", 745.20m, 1.2m, "Chip machines", 301m, 35.2m, 28m, 0.7m,
                18, 4, 1, "Strong Buy", 820m, 5, 4, 3)
        };

        var prompt = _builder.BuildLayoutPayloadPrompt(
            TestAccount, TestHoldings, TestTransactions, TestSignals, stocks);

        prompt.Should().Contain("<stocks>");
        prompt.Should().Contain("745.20");
        prompt.Should().Contain("Strong Buy");
    }

    // ── Null/edge cases ───────────────────────────────────────────────

    [Fact]
    public void BuildLayoutPayloadPrompt_NullOptionalSignals_ShowsEmotionalState()
    {
        var signals = new ContextSignals(
            AccountId: "test-user",
            Scenario: "Test",
            SessionCount30d: 0,
            DaysSinceLastSession: 0,
            PortfolioChange7d: null,
            PortfolioChangeSinceLastVisit: null,
            DividendsReceivedSinceLastVisit: null,
            PendingActions: null,
            MarketVolatility: null,
            TimeOfDay: null,
            LastAction: null,
            SearchQuery: null,
            EmotionalStateEstimate: "Neutral"
        );

        var prompt = _builder.BuildLayoutPayloadPrompt(
            TestAccount, TestHoldings, TestTransactions, signals, EmptyStocks);

        prompt.Should().Contain("Neutral");
        prompt.Should().Contain("<investor>");
    }

    // ── Helper ────────────────────────────────────────────────────────

    private string BuildPrompt() =>
        _builder.BuildLayoutPayloadPrompt(
            TestAccount, TestHoldings, TestTransactions, TestSignals, EmptyStocks);
}

using InvestSuite.Api.Infrastructure.Claude;
using InvestSuite.Api.Infrastructure.Data;
using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Services;

public sealed class AdaptiveLayoutService : IAdaptiveLayoutService
{
    private const string DEFAULT_INPUT_PLACEHOLDER = "Ask anything about your portfolio\u2026";
    private const string YOUR_PORTFOLIO_LABEL = "Your portfolio";
    private const string THIS_WEEK_LABEL = "this week";
    private const string SINCE_LAST_VISIT_LABEL = "since last visit";

    private readonly IAccountRepository _accounts;
    private readonly IStockRepository _stocks;
    private readonly ITransactionRepository _transactions;
    private readonly IClaudeClient _claude;
    private readonly IPromptBuilder _promptBuilder;
    private readonly ILogger<AdaptiveLayoutService> _logger;
    private readonly HoldingsCalculator _holdingsCalc;
    private readonly EfAccountRepository _efAccounts;
    private readonly INewsService _news;
    private readonly IHistoricalPriceService _prices;

    public AdaptiveLayoutService(
        IAccountRepository accounts,
        IStockRepository stocks,
        ITransactionRepository transactions,
        IClaudeClient claude,
        IPromptBuilder promptBuilder,
        ILogger<AdaptiveLayoutService> logger,
        HoldingsCalculator holdingsCalc,
        EfAccountRepository efAccounts,
        INewsService news,
        IHistoricalPriceService prices)
    {
        _accounts = accounts;
        _stocks = stocks;
        _transactions = transactions;
        _claude = claude;
        _promptBuilder = promptBuilder;
        _logger = logger;
        _holdingsCalc = holdingsCalc;
        _efAccounts = efAccounts;
        _news = news;
        _prices = prices;
    }

    // ── Fast context (no LLM) ─────────────────────────────────────────

    public LayoutPayload? BuildContextLayout(string accountId, string? scenario = null, DateOnly? asOfDate = null)
    {
        var context = LoadContext(accountId, scenario, asOfDate);
        if (context is null) return null;

        return BuildLayoutFromData(context.Account, context.Holdings, context.Signals);
    }

    // ── LLM-powered layout ────────────────────────────────────────────

    public async Task<LayoutPayload> GenerateLayoutPayloadAsync(
        string accountId, string? userQuery = null,
        string? scenario = null, DateOnly? asOfDate = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Generating layout for {AccountId}, scenario: {Scenario}, date: {Date}, query: {Query}",
            accountId, scenario, asOfDate, userQuery);

        var ctx = LoadContext(accountId, scenario, asOfDate)
            ?? throw new ArgumentException($"Unknown account: {accountId}");

        KeyMoment? activeMoment = null;
        if (asOfDate.HasValue)
        {
            activeMoment = KeyMomentRepository.Moments
                .FirstOrDefault(m => m.Date == asOfDate.Value);
        }

        var heldSymbols = ctx.Holdings
            .Where(h => h.Type != HoldingType.Cash)
            .Select(h => h.Symbol);

        var prompt = _promptBuilder.BuildLayoutPayloadPrompt(
            ctx.Account, ctx.Holdings, ctx.RecentTransactions,
            ctx.Signals, ctx.StockDetails, userQuery, activeMoment, asOfDate);

        var claudeTask = _claude.GenerateSimpleAsync<ClaudeLayoutDecision>(prompt, ct);
        var newsTask = _news.GetNewsForSymbolsAsync(heldSymbols, maxItems: 5, ct: ct);

        await Task.WhenAll(claudeTask, newsTask);

        return BuildFromDecision(claudeTask.Result, ctx.Holdings, ctx.Signals, ctx.StockDetails, newsTask.Result, userQuery);
    }

    private static LayoutPayload BuildFromDecision(
        ClaudeLayoutDecision ai,
        IReadOnlyList<Holding> holdings,
        ContextSignals signals,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        IReadOnlyList<NewsItem> news,
        string? userQuery = null)
    {
        var portfolioTotal = holdings.Sum(h => h.Value);
        var viewMode = ai.ViewMode;
        var widgets = BuildWidgets(ai, stockDetails, holdings, viewMode == ViewMode.Ambient, news, userQuery);

        // If user said buy/sell but Claude returned ambient with no widget, force stock picker
        var queryLower = userQuery?.ToLowerInvariant() ?? "";
        var querySaysBuy = queryLower.Contains("buy");
        var querySaysSell = queryLower.Contains("sell");
        if ((querySaysBuy || querySaysSell) && viewMode == ViewMode.Ambient && (widgets is null || widgets.Count == 0))
        {
            viewMode = querySaysSell ? ViewMode.SellFlow : ViewMode.BuyFlow;
            widgets = BuildFallbackStockPicker(stockDetails, holdings, querySaysSell);
        }

        var isAmbient = viewMode == ViewMode.Ambient;
        var isExpandedCard = viewMode == ViewMode.ExpandedCard;
        var showBubbles = isAmbient || isExpandedCard;
        var showStrip = !isAmbient && !isExpandedCard;

        return new LayoutPayload(
            Tone: ai.Tone,
            Sentiment: ai.Sentiment,
            ViewMode: viewMode,
            Suggestions: ai.Suggestions.Select((s, i) =>
                new SuggestionChip($"sug-{i + 1}", s.Label, s.Query)).ToList(),
            Center: BuildCenter(portfolioTotal, signals, viewMode),
            AiMessage: new AIMessage(ai.AiMessage, ai.Footnote),
            Bubbles: showBubbles ? BuildBubbles(holdings) : null,
            BubbleStrip: showStrip ? BuildBubbleStrip(holdings) : null,
            ExpandedCard: ai.ExpandedCard,
            Widgets: widgets,
            InputPlaceholder: ai.InputPlaceholder ?? DEFAULT_INPUT_PLACEHOLDER
        );
    }

    private static List<LayoutWidgetConfig> BuildFallbackStockPicker(
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        IReadOnlyList<Holding> holdings,
        bool isSellMode)
    {
        var cash = holdings.Where(h => h.Type == HoldingType.Cash).Sum(h => h.Value);
        var data = new Dictionary<string, object?>();
        data["ai_context"] = isSellMode
            ? "Choose a position to sell."
            : "Pick a stock to buy from the available European stocks.";
        data["ai_context_accent"] = "blue";
        data["currency"] = "EUR";
        data["available_cash"] = cash;

        if (isSellMode)
        {
            var stocks = holdings
                .Where(h => h.Type != HoldingType.Cash && h.Shares > 0)
                .Select(h =>
                {
                    stockDetails.TryGetValue(h.Symbol, out var s);
                    return new Dictionary<string, object?>
                    {
                        ["symbol"] = h.Symbol,
                        ["name"] = h.Name,
                        ["price"] = s?.Price ?? h.Value / h.Shares,
                        ["delta_pct"] = s?.ChangeTodayPct ?? 0m,
                        ["shares_owned"] = h.Shares,
                        ["current_value"] = h.Value,
                        ["return_pct"] = h.ReturnPct ?? 0m,
                        ["is_held"] = true,
                        ["tap_query"] = $"Sell {h.Symbol}"
                    };
                }).ToList();
            data["stocks"] = stocks;
            data["mode"] = "sell";
            data["title"] = "Your Holdings";
            data["subtitle"] = "Choose a position to sell";
        }
        else
        {
            var stocks = stockDetails.Select(kvp =>
            {
                var s = kvp.Value;
                var holding = holdings.FirstOrDefault(h => h.Symbol == kvp.Key);
                var held = holding is not null;
                var stockData = new Dictionary<string, object?>
                {
                    ["symbol"] = kvp.Key,
                    ["name"] = s.Name,
                    ["price"] = s.Price,
                    ["delta_pct"] = s.ChangeTodayPct,
                    ["is_held"] = held,
                    ["tap_query"] = $"Buy {kvp.Key}"
                };
                if (held)
                {
                    stockData["shares_owned"] = holding!.Shares;
                    stockData["current_value"] = holding.Value;
                    stockData["return_pct"] = holding.ReturnPct ?? 0m;
                }
                return stockData;
            }).ToList();
            data["stocks"] = stocks;
            data["mode"] = "buy";
        }

        var widgetData = System.Text.Json.JsonSerializer.SerializeToElement(data);
        return
        [
            new LayoutWidgetConfig(
                Id: "w-1",
                Type: "stock_picker",
                Tier: Tier.Expanded,
                Priority: 1,
                Data: widgetData)
        ];
    }

    private static CenterContent BuildCenter(decimal portfolioTotal, ContextSignals signals, ViewMode viewMode)
    {
        var delta = ComputeDelta(signals);
        return new CenterContent(
            Value: $"\u20ac{portfolioTotal:N0}",
            Label: YOUR_PORTFOLIO_LABEL,
            Delta: delta.Delta,
            DeltaColor: delta.DeltaColor,
            DeltaLabel: delta.DeltaLabel
        );
    }

    private static List<BubbleStripItem> BuildBubbleStrip(IReadOnlyList<Holding> holdings)
    {
        return holdings
            .Where(h => h.Type != HoldingType.Cash)
            .Select(h => new BubbleStripItem(
                Id: $"strip-{h.Symbol}",
                Ticker: h.Symbol,
                TapQuery: $"Tell me about {h.Name}"))
            .ToList();
    }

    private static List<LayoutWidgetConfig>? BuildWidgetFromDecision(
        ClaudeLayoutDecision ai,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        IReadOnlyList<Holding> holdings,
        string? userQuery = null)
    {
        if (ai.WidgetType is null) return null;

        var querySaysSell = userQuery?.Contains("sell", StringComparison.OrdinalIgnoreCase) == true;

        // Override widget type: if user said "sell" + specific symbol → force sell_flow
        var widgetType = ai.WidgetType;
        if (querySaysSell && ai.ActiveSymbol is not null && widgetType is "buy_flow")
        {
            var heldPosition = holdings.FirstOrDefault(h => h.Symbol == ai.ActiveSymbol && h.Shares > 0);
            if (heldPosition is not null)
            {
                widgetType = "sell_flow";
            }
        }

        var cash = holdings.Where(h => h.Type == HoldingType.Cash).Sum(h => h.Value);
        var data = new Dictionary<string, object?>();

        data["ai_context"] = ai.AiContext;
        data["ai_context_accent"] = ai.AiContextAccent ?? "blue";
        data["currency"] = "EUR";
        data["available_cash"] = cash;

        if (ai.ActiveSymbol is not null && stockDetails.TryGetValue(ai.ActiveSymbol, out var stock))
        {
            data["symbol"] = ai.ActiveSymbol;
            data["name"] = stock.Name;
            data["price"] = stock.Price;
            data["delta_pct"] = stock.ChangeTodayPct;

            var holding = holdings.FirstOrDefault(h => h.Symbol == ai.ActiveSymbol);
            if (holding is not null)
            {
                data["shares_owned"] = holding.Shares;
                if (widgetType is "sell_flow")
                {
                    data["current_value"] = holding.Value;
                    data["average_cost"] = holding.AverageCost;
                    data["return_pct"] = holding.ReturnPct ?? 0m;
                }
            }
        }

        if (widgetType is "stock_picker")
        {
            var isSellMode = querySaysSell;

            if (isSellMode)
            {
                var stocks = holdings
                    .Where(h => h.Type != HoldingType.Cash && h.Shares > 0)
                    .Select(h =>
                    {
                        stockDetails.TryGetValue(h.Symbol, out var s);
                        return new Dictionary<string, object?>
                        {
                            ["symbol"] = h.Symbol,
                            ["name"] = h.Name,
                            ["price"] = s?.Price ?? h.Value / h.Shares,
                            ["delta_pct"] = s?.ChangeTodayPct ?? 0m,
                            ["shares_owned"] = h.Shares,
                            ["current_value"] = h.Value,
                            ["return_pct"] = h.ReturnPct ?? 0m,
                            ["is_held"] = true,
                            ["tap_query"] = $"Sell {h.Symbol}"
                        };
                    }).ToList();
                data["stocks"] = stocks;
                data["mode"] = "sell";
                data["title"] = "Your Holdings";
                data["subtitle"] = "Choose a position to sell";
            }
            else
            {
                var stocks = stockDetails.Select(kvp =>
                {
                    var s = kvp.Value;
                    var holding = holdings.FirstOrDefault(h => h.Symbol == kvp.Key);
                    var held = holding is not null;
                    var stockData = new Dictionary<string, object?>
                    {
                        ["symbol"] = kvp.Key,
                        ["name"] = s.Name,
                        ["price"] = s.Price,
                        ["delta_pct"] = s.ChangeTodayPct,
                        ["is_held"] = held,
                        ["tap_query"] = $"Buy {kvp.Key}"
                    };
                    if (held)
                    {
                        stockData["shares_owned"] = holding!.Shares;
                        stockData["current_value"] = holding.Value;
                        stockData["return_pct"] = holding.ReturnPct ?? 0m;
                    }
                    return stockData;
                }).ToList();
                data["stocks"] = stocks;
                data["mode"] = "buy";
            }
        }

        var widgetData = System.Text.Json.JsonSerializer.SerializeToElement(data);
        return
        [
            new LayoutWidgetConfig(
                Id: "w-1",
                Type: widgetType,
                Tier: Tier.Expanded,
                Priority: 1,
                Data: widgetData)
        ];
    }

    private static List<LayoutWidgetConfig>? BuildWidgets(
        ClaudeLayoutDecision ai,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        IReadOnlyList<Holding> holdings,
        bool isAmbient,
        IReadOnlyList<NewsItem> news,
        string? userQuery = null)
    {
        var widgets = BuildWidgetFromDecision(ai, stockDetails, holdings, userQuery);

        if (isAmbient && news.Count > 0)
        {
            widgets ??= [];
            widgets.Add(BuildNewsWidget(news));
        }

        return widgets;
    }

    private static LayoutWidgetConfig BuildNewsWidget(IReadOnlyList<NewsItem> news)
    {
        var items = news.Select(n => new Dictionary<string, object?>
        {
            ["headline"] = n.Headline,
            ["source"] = n.Source,
            ["time_ago"] = FormatTimeAgo(n.PublishedAt),
            ["related_symbol"] = n.RelatedSymbol,
            ["sentiment"] = n.Sentiment,
        }).ToList();

        var data = System.Text.Json.JsonSerializer.SerializeToElement(new { items });
        return new LayoutWidgetConfig(
            Id: "w-news",
            Type: "news_digest",
            Tier: Tier.Glance,
            Priority: 10,
            Data: data);
    }

    private static string FormatTimeAgo(DateTimeOffset publishedAt)
    {
        var elapsed = DateTimeOffset.UtcNow - publishedAt;
        return elapsed.TotalMinutes < 60
            ? $"{(int)elapsed.TotalMinutes}m ago"
            : elapsed.TotalHours < 24
                ? $"{(int)elapsed.TotalHours}h ago"
                : $"{(int)elapsed.TotalDays}d ago";
    }

    // ── Account listing ───────────────────────────────────────────────

    public IReadOnlyList<AccountSummary> GetAccounts()
    {
        return _accounts.GetAll().Values.Select(a =>
        {
            var holdings = _holdingsCalc.ComputeHoldings(a.Id, DateOnly.FromDateTime(DateTime.UtcNow), 1000m);
            var total = holdings.Sum(h => h.Value);
            return new AccountSummary(
                Id: a.Id,
                Name: $"{a.FirstName} {a.LastName}",
                ExperienceLevel: a.ExperienceLevel,
                PortfolioTotal: total,
                Scenarios: []);
        }).ToList();
    }

    // ── Data loading ──────────────────────────────────────────────────

    private sealed record ContextBundle(
        Account Account,
        IReadOnlyList<Holding> Holdings,
        IReadOnlyList<Transaction> RecentTransactions,
        ContextSignals Signals,
        IReadOnlyDictionary<string, StockDetail> StockDetails);

    private ContextBundle? LoadContext(string accountId, string? scenario = null, DateOnly? asOfDate = null)
    {
        var account = _accounts.GetById(accountId);
        if (account is null) return null;

        // Compute holdings from transactions + historical prices
        var effectiveDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var holdings = _holdingsCalc.ComputeHoldings(accountId, effectiveDate, 1000m);

        // Compute 7-day portfolio change, anchored to the actual latest available data date
        // so simulated/future dates correctly compare real historical prices
        var firstStock = holdings.Where(h => h.Type != HoldingType.Cash).Select(h => h.Symbol).FirstOrDefault();
        var latestDataDate = firstStock is not null
            ? (_prices.GetLatestAvailableDate(firstStock) ?? effectiveDate)
            : effectiveDate;
        var weekAgoDate = latestDataDate.AddDays(-7);
        var weekAgoHoldings = _holdingsCalc.ComputeHoldings(accountId, weekAgoDate, 1000m);
        var currentTotal = holdings.Sum(h => h.Value);
        var weekAgoTotal = weekAgoHoldings.Sum(h => h.Value);
        decimal? portfolioChange7d = weekAgoTotal > 0
            ? Math.Round((currentTotal - weekAgoTotal) / weekAgoTotal * 100m, 2)
            : null;

        // Build signals — for time-travel, generate from key moment
        ContextSignals signals;
        if (asOfDate.HasValue)
        {
            var moment = KeyMomentRepository.Moments
                .FirstOrDefault(m => m.Date == asOfDate.Value);
            signals = BuildSignals(accountId, moment, portfolioChange7d);
        }
        else
        {
            signals = BuildSignals(accountId, null, portfolioChange7d);
        }

        var transactions = _transactions.GetRecent(accountId, 15);
        return new ContextBundle(account, holdings, transactions, signals, _stocks.GetAllStocks());
    }

    private static ContextSignals BuildSignals(string accountId, KeyMoment? moment, decimal? portfolioChange7d) =>
        new(
            AccountId: accountId,
            Scenario: moment?.Title ?? "time-travel",
            SessionCount30d: 1,
            DaysSinceLastSession: 0,
            PortfolioChange7d: portfolioChange7d,
            PortfolioChangeSinceLastVisit: null,
            DividendsReceivedSinceLastVisit: null,
            PendingActions: null,
            MarketVolatility: moment?.Volatility ?? "Normal",
            TimeOfDay: "afternoon",
            LastAction: null,
            SearchQuery: null,
            EmotionalStateEstimate: moment?.Emotion ?? "curious"
        );

    // ── Context layout builder (no LLM) ──────────────────────────────

    private static LayoutPayload BuildLayoutFromData(
        Account account,
        IReadOnlyList<Holding> holdings,
        ContextSignals signals)
    {
        var portfolioTotal = holdings.Sum(h => h.Value);
        return new LayoutPayload(
            Tone: MapTone(signals.EmotionalStateEstimate),
            Sentiment: MapSentiment(signals.EmotionalStateEstimate),
            ViewMode: ViewMode.Ambient,
            Suggestions: [],
            Center: BuildCenter(portfolioTotal, signals),
            AiMessage: null,
            Bubbles: BuildBubbles(holdings),
            InputPlaceholder: DEFAULT_INPUT_PLACEHOLDER
        );
    }

    private static Tone MapTone(string emotionalState)
    {
        var s = emotionalState.ToLowerInvariant();
        if (s.Contains("anxious") || s.Contains("panic") || s.Contains("compulsiv") || s.Contains("euphori") || s.Contains("greed") || s.Contains("fomo") || s.Contains("shock") || s.Contains("fear")) return Tone.Reassuring;
        if (s.Contains("focused") || s.Contains("research")) return Tone.Focused;
        if (s.Contains("detached") || s.Contains("out of")) return Tone.Welcoming;
        return Tone.Neutral;
    }

    private static Sentiment MapSentiment(string emotionalState)
    {
        var s = emotionalState.ToLowerInvariant();
        if (s.Contains("calm") || s.Contains("curious")) return Sentiment.Calm;
        if (s.Contains("anxious") || s.Contains("panic") || s.Contains("compulsiv")) return Sentiment.Fear;
        if (s.Contains("focused") || s.Contains("research")) return Sentiment.Neutral;
        if (s.Contains("detached") || s.Contains("out of")) return Sentiment.Returning;
        if (s.Contains("greed") || s.Contains("overconfident") || s.Contains("fomo") || s.Contains("euphori")) return Sentiment.Greed;
        if (s.Contains("shock") || s.Contains("fear")) return Sentiment.Fear;
        if (s.Contains("pessimis")) return Sentiment.Fear;
        return Sentiment.Neutral;
    }

    private sealed record DeltaInfo(string? Delta, string? DeltaColor, string? DeltaLabel);

    private static CenterContent BuildCenter(decimal portfolioTotal, ContextSignals signals)
        => BuildCenter(portfolioTotal, signals, ViewMode.Ambient);

    private static DeltaInfo ComputeDelta(ContextSignals signals)
    {
        var change = signals.PortfolioChange7d ?? signals.PortfolioChangeSinceLastVisit;

        var deltaLabel = signals.PortfolioChange7d.HasValue
            ? THIS_WEEK_LABEL
            : signals.PortfolioChangeSinceLastVisit.HasValue
                ? SINCE_LAST_VISIT_LABEL : null;

        if (!change.HasValue) return new DeltaInfo(null, null, deltaLabel);

        var delta = change > 0 ? $"+{change:N2}%" : $"{change:N2}%";
        var deltaColor = change >= 0 ? "positive" : "negative";
        return new DeltaInfo(delta, deltaColor, deltaLabel);
    }

    private static List<BubbleConfig> BuildBubbles(IReadOnlyList<Holding> holdings)
    {
        var total = holdings.Sum(h => h.Value);
        if (total <= 0) return [];

        return holdings
            .OrderByDescending(h => h.Value)
            .Select((h, i) =>
            {
                var isCash = h.Type == HoldingType.Cash;
                var delta = isCash
                    ? $"\u20ac{h.Value:N0}"
                    : ((h.ReturnPct ?? 0) >= 0 ? $"+{h.ReturnPct}%" : $"{h.ReturnPct}%");
                var direction = isCash
                    ? BubbleDirection.Neutral
                    : ((h.ReturnPct ?? 0) >= 0 ? BubbleDirection.Up : BubbleDirection.Down);

                return new BubbleConfig(
                    Id: $"bubble-{i}",
                    Ticker: h.Name,
                    Delta: delta,
                    Direction: direction,
                    Weight: Math.Round(h.Value / total, 4),
                    TapQuery: isCash ? null : $"Tell me about {h.Name}"
                );
            })
            .ToList();
    }
}

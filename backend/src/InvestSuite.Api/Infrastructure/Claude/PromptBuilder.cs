using System.Globalization;
using System.Text;
using InvestSuite.Api.Models;
using static System.FormattableString;

namespace InvestSuite.Api.Infrastructure.Claude;

public sealed class PromptBuilder : IPromptBuilder
{
    private readonly string _systemPrompt;
    private readonly string _rolePrompt;
    private readonly string _rulesPrompt;
    private readonly string _toneGuide;

    // env parameter required for DI resolution even though we use AppContext.BaseDirectory
    public PromptBuilder(IWebHostEnvironment _)
    {
        var promptsPath = Path.Combine(AppContext.BaseDirectory, "prompts/system");
        _systemPrompt = LoadFile(promptsPath, "00-system.md");
        _rolePrompt = LoadFile(promptsPath, "01-role.md");
        _rulesPrompt = LoadFile(promptsPath, "02-rules.md");
        _toneGuide = LoadFile(promptsPath, "03-tone-guide.md");
    }

    public PromptBuilder(string promptsPath)
    {
        _systemPrompt = LoadFile(promptsPath, "00-system.md");
        _rolePrompt = LoadFile(promptsPath, "01-role.md");
        _rulesPrompt = LoadFile(promptsPath, "02-rules.md");
        _toneGuide = LoadFile(promptsPath, "03-tone-guide.md");
    }

    public string SystemPrompt
    {
        get
        {
            var sb = new StringBuilder();
            sb.AppendLine(_systemPrompt);
            sb.AppendLine(_rolePrompt);
            sb.AppendLine(_rulesPrompt);
            sb.AppendLine(_toneGuide);
            sb.AppendLine(RESPONSE_SCHEMA);
            return sb.ToString();
        }
    }

    private static string LoadFile(string basePath, string filename)
    {
        var path = Path.Combine(basePath, filename);
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    public string BuildLayoutPayloadPrompt(
        Account account,
        IReadOnlyList<Holding> holdings,
        IReadOnlyList<Transaction> recentTransactions,
        ContextSignals signals,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        string? userQuery = null,
        KeyMoment? activeMoment = null,
        DateOnly? asOfDate = null)
    {
        var heldSymbols = holdings
            .Where(h => h.Type != HoldingType.Cash)
            .Select(h => h.Symbol)
            .ToHashSet();
        var relevantStocks = FilterRelevantStocks(stockDetails, heldSymbols, userQuery);

        var sb = new StringBuilder();
        AppendTimeTravelContext(sb, activeMoment, asOfDate, holdings);
        AppendInvestorProfile(sb, account, holdings, recentTransactions, signals);
        AppendStockData(sb, relevantStocks, heldSymbols);
        AppendUserQuery(sb, userQuery);
        return sb.ToString();
    }

    private static void AppendTimeTravelContext(
        StringBuilder sb, KeyMoment? moment, DateOnly? asOfDate, IReadOnlyList<Holding> holdings)
    {
        if (moment is null && !asOfDate.HasValue) return;

        var portfolioTotal = holdings.Sum(h => h.Value);
        sb.AppendLine("<time_travel>");
        if (asOfDate.HasValue)
            sb.AppendLine($"Date: {asOfDate.Value:yyyy-MM-dd}");
        if (moment is not null)
        {
            sb.AppendLine($"Event: \"{moment.Title}\" — {moment.Description}");
            sb.AppendLine($"Mood: {moment.Emotion} | Volatility: {moment.Volatility}");
            sb.AppendLine($"Affected: {string.Join(", ", moment.AffectedStocks)}");
        }
        sb.AppendLine(Invariant($"Portfolio value: €{portfolioTotal:N0}"));
        sb.AppendLine("</time_travel>");
        sb.AppendLine();
    }

    private static void AppendInvestorProfile(
        StringBuilder sb,
        Account account,
        IReadOnlyList<Holding> holdings,
        IReadOnlyList<Transaction> transactions,
        ContextSignals signals)
    {
        var portfolioTotal = holdings.Sum(h => h.Value);
        var cash = holdings.Where(h => h.Type == HoldingType.Cash).Sum(h => h.Value);
        var stockHoldings = holdings.Where(h => h.Type != HoldingType.Cash).ToList();

        sb.AppendLine("<investor>");
        sb.AppendLine($"{account.FirstName} {account.LastName}, age {CalculateAge(account.DateOfBirth)}, {account.RiskProfile} risk, {account.ExperienceLevel}");
        sb.AppendLine(Invariant($"Portfolio: €{portfolioTotal:N0} (Cash: €{cash:N0})"));

        if (stockHoldings.Count > 0)
        {
            var holdingParts = stockHoldings.Select(h =>
            {
                var ret = h.ReturnPct.HasValue
                    ? string.Format(CultureInfo.InvariantCulture, " {0}{1}%", h.ReturnPct >= 0 ? "+" : "", h.ReturnPct)
                    : "";
                return Invariant($"{h.Symbol} {h.Shares}sh €{h.Value:N0}{ret}");
            });
            sb.AppendLine($"Holdings: {string.Join(", ", holdingParts)}");
        }

        if (transactions.Count > 0)
        {
            var txParts = transactions.Take(5).Select(tx =>
                Invariant($"{tx.ExecutedAt:yyyy-MM-dd} {tx.Type} {tx.Symbol ?? "—"} €{tx.Amount:N0}"));
            sb.AppendLine($"Recent: {string.Join(", ", txParts)}");
        }

        sb.AppendLine($"State: {signals.EmotionalStateEstimate} | Volatility: {signals.MarketVolatility ?? "Normal"}");
        sb.AppendLine("</investor>");
        sb.AppendLine();
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth > today.AddYears(-age)) age--;
        return age;
    }

    private static void AppendStockData(
        StringBuilder sb,
        IReadOnlyDictionary<string, StockDetail> stockDetails,
        HashSet<string> heldSymbols)
    {
        if (stockDetails.Count == 0) return;

        sb.AppendLine("<stocks>");
        foreach (var (symbol, s) in stockDetails)
        {
            var held = heldSymbols.Contains(symbol) ? " [held]" : "";
            sb.AppendLine(Invariant($"{symbol}{held}: €{s.Price:N2} {s.ChangeTodayPct:+0.0;-0.0}% P/E:{s.PeRatio} div:{s.DividendYieldPct}% {s.AnalystConsensus}(€{s.AnalystTargetEur})"));
        }
        sb.AppendLine("</stocks>");
        sb.AppendLine();
    }

    private static void AppendUserQuery(StringBuilder sb, string? userQuery)
    {
        if (!string.IsNullOrEmpty(userQuery))
        {
            sb.AppendLine($"<query>{userQuery}</query>");
        }
        else
        {
            sb.AppendLine("<query>ambient home view</query>");
        }
    }

    private static IReadOnlyDictionary<string, StockDetail> FilterRelevantStocks(
        IReadOnlyDictionary<string, StockDetail> allStocks,
        HashSet<string> heldSymbols,
        string? userQuery)
    {
        string? queriedSymbol = null;
        if (userQuery is not null)
        {
            queriedSymbol = allStocks.Keys
                .FirstOrDefault(s => userQuery.Contains(s, StringComparison.OrdinalIgnoreCase)
                    || userQuery.Contains(allStocks[s].Name, StringComparison.OrdinalIgnoreCase));
        }

        if (queriedSymbol is null && userQuery is not null && (
            userQuery.Contains("buy", StringComparison.OrdinalIgnoreCase) ||
            userQuery.Contains("sell", StringComparison.OrdinalIgnoreCase) ||
            userQuery.Contains("compare", StringComparison.OrdinalIgnoreCase) ||
            userQuery.Contains("research", StringComparison.OrdinalIgnoreCase)))
        {
            return allStocks;
        }

        var filtered = new Dictionary<string, StockDetail>();
        foreach (var (symbol, detail) in allStocks)
        {
            if (heldSymbols.Contains(symbol) || symbol == queriedSymbol)
                filtered[symbol] = detail;
        }
        return filtered;
    }

    private const string RESPONSE_SCHEMA = """
        <response_format>
        Return ONLY valid JSON. No markdown. The server handles all UI rendering, pricing, and layout.
        You decide ONLY the tone, message, and intent.

        {
          "tone": "reassuring|celebratory|neutral|focused|welcoming",
          "sentiment": "calm|cautious|fear|greed|returning|neutral",
          "view_mode": "ambient|expanded_card|research|buy_flow",
          "ai_message": "One brief contextual sentence for the investor.",
          "footnote": "Optional short helper text or null.",
          "active_symbol": "Ticker if query is about a specific stock, else null.",
          "widget_type": "buy_flow|sell_flow|stock_picker|null — only for buy_flow/research views.",
          "ai_context": "Brief contextual insight for the active widget, or null.",
          "ai_context_accent": "green|blue|amber — mood color for the widget context.",
          "suggestions": [
            { "id": "sug-1", "label": "Display text", "query": "The query to send" }
          ],
          "input_placeholder": "Ask anything about your portfolio…"
        }

        VIEW MODE RULES:
        - "ambient": Default portfolio home. Show center (large), ai_message, bubbles (all holdings), suggestions.
        - "expanded_card": Detailed card about a specific holding. Show center (small, opacity 0.4), expanded_card, minimal bubbles (just the active one, highlighted).
          "expanded_card" JSON schema:
          {
            "name": "ASML Holding NV",
            "symbol": "ASML",
            "price": "EUR745.20",
            "price_delta": "+9.6%",
            "price_color": "var(--green)",
            "subtitle": "Semiconductor Equipment",
            "sections": [
              { "type": "insight", "text": "Short insight text about this holding.", "accent": "green" },
              { "type": "education", "title": "What does ASML do?", "body": "Explanation paragraph." },
              { "type": "metrics", "items": [{ "label": "P/E Ratio", "value": "38.2" }, { "label": "Market Cap", "value": "EUR298B" }] },
              { "type": "ratings", "items": [{ "label": "Analyst Score", "score": 8.5, "max": 10 }] },
              { "type": "position", "rows": [{ "label": "Shares", "value": "2.0" }, { "label": "Avg Cost", "value": "EUR680" }] },
              { "type": "comparison", "rows": [{ "name": "ASML", "value": "+9.6%", "color": "var(--green)" }, { "name": "S&P 500", "value": "+4.1%", "color": "var(--green)" }] },
              { "type": "reframe", "message": "Reassuring or reframing message." },
              { "type": "context_card", "title": "Related Event", "body": "Historical context about a market event." }
            ],
            "actions": [
              { "label": "Buy More", "style": "primary", "query": "Buy ASML" },
              { "label": "Sell", "style": "ghost", "query": "Sell ASML" }
            ],
            "footer": "Optional footer note"
          }
          Pick 3-5 relevant section types per card. All section fields except "type" are optional — use what fits.
        - "research": Comparison/research mode. Show bubble_strip, widgets (comparison_table, sector_heatmap, ai_insight), suggestions.
        - "buy_flow": Purchase or sell flow. Show bubble_strip, widgets (buy_flow, sell_flow, or stock_picker), suggestions.

        TONE MAPPING (from emotional state):
        - Calm/curious → tone: "neutral", sentiment: "calm"
        - Anxious/panicking → tone: "reassuring", sentiment: "fear"
        - Focused/researching → tone: "focused", sentiment: "neutral"
        - Detached/returning → tone: "welcoming", sentiment: "returning"
        - Greedy/overconfident → tone: "reassuring", sentiment: "greed"

        DENSITY MAPPING:
        - beginner → "sparse"
        - expert → "moderate" or "dense"

        BUBBLE LAYOUT:
        - Create one bubble per holding (including cash)
        - Largest holding gets size "xl", next "lg", then "md", "sm", "xs"
        - Spread positions: top between 58%-76%, left between 15%-90%
        - Use float_duration between "7s" and "14s", vary float_delay
        - For negative returns: direction "down", for positive: "up", for cash: "neutral"
        - Cash bubble shows euro amount as delta (e.g. "€587"), not percentage
        - Set tap_query to a natural question about that holding

        SUGGESTIONS:
        - Always include exactly 3 suggestion chips
        - Each needs: id (unique string), label (display text), query (what to send)
        - Make them contextually relevant to the investor's state

        CENTER CONTENT:
        - value: formatted portfolio total with € symbol
        - delta: percentage change with + or - prefix
        - delta_color: "var(--green)" for positive, "var(--red)" for negative
        - In ambient mode: size "large", full opacity
        - In expanded_card mode: size "small", opacity 0.4

        WIDGET TYPES FOR RESEARCH / BUY_FLOW VIEW:
        When view_mode is "research" or "buy_flow", include a "widgets" array. Available widget types:

        "stock_picker" — browsable list of stocks for buying or selling:
        { "id": "w-picker", "type": "stock_picker", "tier": "expanded", "priority": 1,
          "data": {
            "title": "Available Stocks", "subtitle": "...", "mode": "buy" or "sell",
            "available_cash": 587, "currency": "EUR",
            "stocks": [{ "symbol": "ASML", "name": "ASML", "price": "EUR745.20", "delta": "+1.2%",
              "description": "...", "is_held": true, "tap_query": "Buy ASML" }],
            "ai_context": "...", "ai_context_accent": "green" or "blue" or "amber"
          }}

        "buy_flow" — purchase flow for a specific stock (existing, uses euro amount)

        "sell_flow" — sell flow for a specific holding (uses share count):
        { "id": "w-sell", "type": "sell_flow", "tier": "expanded", "priority": 1,
          "data": {
            "symbol": "ASML", "name": "ASML", "price": "EUR745.20", "delta": "+9.6%",
            "shares_owned": 2.0, "current_value": "EUR1,490", "average_cost": "EUR680.00",
            "return_pct": "+9.6%", "return_color": "var(--green)", "currency": "EUR",
            "ai_context": "...", "ai_context_accent": "green" or "blue" or "amber",
            "order_details": [{ "label": "...", "value": "..." }]
          }}

        "ai_insight", "sector_heatmap", "comparison_table", "dividends", "order_status", "news_digest" — existing types.
        """;
}

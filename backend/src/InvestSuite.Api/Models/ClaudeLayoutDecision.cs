using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

public record ClaudeLayoutDecision(
    [property: JsonPropertyName("tone")] Tone Tone,
    [property: JsonPropertyName("sentiment")] Sentiment Sentiment,
    [property: JsonPropertyName("view_mode")] ViewMode ViewMode,
    [property: JsonPropertyName("ai_message")] string AiMessage,
    [property: JsonPropertyName("suggestions")] List<ClaudeSuggestion> Suggestions,
    [property: JsonPropertyName("footnote")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Footnote = null,
    [property: JsonPropertyName("active_symbol")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? ActiveSymbol = null,
    [property: JsonPropertyName("widget_type")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? WidgetType = null,
    [property: JsonPropertyName("ai_context")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? AiContext = null,
    [property: JsonPropertyName("ai_context_accent")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? AiContextAccent = null,
    [property: JsonPropertyName("input_placeholder")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? InputPlaceholder = null,
    [property: JsonPropertyName("expanded_card")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    ExpandedCardConfig? ExpandedCard = null
);

public record ClaudeSuggestion(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("query")] string Query
);

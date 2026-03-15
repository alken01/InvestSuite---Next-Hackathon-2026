using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvestSuite.Api.Models;

// ── Request DTOs ──────────────────────────────────────────────────────

public record LayoutRequest(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("query")] string? Query = null,
    [property: JsonPropertyName("account_id")] string? AccountId = null,
    [property: JsonPropertyName("scenario")] string? Scenario = null,
    [property: JsonPropertyName("date")] string? Date = null
);

// ── Snake-case enum converter ─────────────────────────────────────────
// JsonStringEnumConverter ignores [JsonPropertyName] on enum members.
// This subclass applies SnakeCaseLower so ExpandedCard → "expanded_card".
// Case-insensitive on read so Claude can return "Calm" or "calm" interchangeably.

public sealed class SnakeCaseEnumConverter : JsonConverterFactory
{
    private static readonly JsonNamingPolicy Policy = JsonNamingPolicy.SnakeCaseLower;

    public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(Inner<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }

    private sealed class Inner<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString() ?? throw new JsonException($"Expected string for {typeof(T).Name}");
            foreach (var name in Enum.GetNames<T>())
            {
                if (string.Equals(Policy.ConvertName(name), value, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, value, StringComparison.OrdinalIgnoreCase))
                    return Enum.Parse<T>(name);
            }
            throw new JsonException($"Cannot convert \"{value}\" to {typeof(T).Name}");
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
            writer.WriteStringValue(Policy.ConvertName(value.ToString()));
    }
}

// ── Enums ──────────────────────────────────────────────────────────────

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Tone { Reassuring, Celebratory, Neutral, Focused, Welcoming }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Density { Sparse, Moderate, Dense }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Tier { Glance, Expanded }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Sentiment { Calm, Cautious, Fear, Greed, Returning, Neutral }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum ViewMode { Ambient, ExpandedCard, Research, BuyFlow, SellFlow }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum BreathingSpeed { Slow, Normal, Fast }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum BubbleDirection { Up, Down, Neutral }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum BubbleState { Normal, Highlighted, Dimmed }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum Accent { Green, Blue, Amber, Neutral }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum ActionStyle { Primary, Secondary, Ghost }

[JsonConverter(typeof(SnakeCaseEnumConverter))]
public enum SuggestionCategory { Suggested, Recent }


// ── Top-level LayoutPayload ────────────────────────────────────────────

public record LayoutPayload(
    [property: JsonPropertyName("tone")] Tone Tone,
    [property: JsonPropertyName("sentiment")] Sentiment Sentiment,
    [property: JsonPropertyName("view_mode")] ViewMode ViewMode,
    [property: JsonPropertyName("suggestions")] List<SuggestionChip> Suggestions,
    [property: JsonPropertyName("ambient")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AmbientConfig? Ambient = null,
    [property: JsonPropertyName("center")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    CenterContent? Center = null,
    [property: JsonPropertyName("ai_message")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    AIMessage? AiMessage = null,
    [property: JsonPropertyName("bubbles")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<BubbleConfig>? Bubbles = null,
    [property: JsonPropertyName("expanded_card")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    ExpandedCardConfig? ExpandedCard = null,
    [property: JsonPropertyName("widgets")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<LayoutWidgetConfig>? Widgets = null,
    [property: JsonPropertyName("bubble_strip")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<BubbleStripItem>? BubbleStrip = null,
    [property: JsonPropertyName("input_placeholder")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? InputPlaceholder = null
);

// ── AmbientConfig ──────────────────────────────────────────────────────

public record AmbientConfig(
    [property: JsonPropertyName("breathing_speed")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BreathingSpeed? BreathingSpeed = null,
    [property: JsonPropertyName("ring_opacity")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    double? RingOpacity = null
);

// ── CenterContent ──────────────────────────────────────────────────────

public record CenterContent(
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("label")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Label = null,
    [property: JsonPropertyName("delta")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Delta = null,
    [property: JsonPropertyName("delta_color")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? DeltaColor = null,
    [property: JsonPropertyName("delta_label")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? DeltaLabel = null
);

// ── AIMessage ──────────────────────────────────────────────────────────

public record AIMessage(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("footnote")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Footnote = null,
    [property: JsonPropertyName("accent")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    Accent? Accent = null
);

// ── BubbleConfig ───────────────────────────────────────────────────────

public record BubbleConfig(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("delta")] string Delta,
    [property: JsonPropertyName("direction")] BubbleDirection Direction,
    [property: JsonPropertyName("weight")] decimal Weight,
    [property: JsonPropertyName("state")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    BubbleState? State = null,
    [property: JsonPropertyName("has_nudge")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? HasNudge = null,
    [property: JsonPropertyName("tap_query")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? TapQuery = null
);

// ── BubbleStripItem ────────────────────────────────────────────────────

public record BubbleStripItem(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("ticker")] string Ticker,
    [property: JsonPropertyName("is_active")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsActive = null,
    [property: JsonPropertyName("tap_query")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? TapQuery = null
);

// ── ExpandedCardConfig ─────────────────────────────────────────────────

public record ExpandedCardConfig(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("sections")] List<ExpandedCardSection> Sections,
    [property: JsonPropertyName("price")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Price = null,
    [property: JsonPropertyName("price_delta")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? PriceDelta = null,
    [property: JsonPropertyName("price_color")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? PriceColor = null,
    [property: JsonPropertyName("subtitle")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Subtitle = null,
    [property: JsonPropertyName("actions")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<ExpandedCardAction>? Actions = null,
    [property: JsonPropertyName("footer")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Footer = null
);

public record ExpandedCardAction(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("style")] ActionStyle Style,
    [property: JsonPropertyName("query")] string Query
);

// ── ExpandedCardSection (flattened discriminated union) ─────────────────

public record ExpandedCardSection(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("text")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Text = null,
    [property: JsonPropertyName("accent")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Accent = null,
    [property: JsonPropertyName("title")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Title = null,
    [property: JsonPropertyName("body")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Body = null,
    [property: JsonPropertyName("items")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<SectionItem>? Items = null,
    [property: JsonPropertyName("rows")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    List<SectionRow>? Rows = null,
    [property: JsonPropertyName("paid")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Paid = null,
    [property: JsonPropertyName("current")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Current = null,
    [property: JsonPropertyName("message")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Message = null
);

// Covers both "metrics" items ({ value, label }) and "ratings" items ({ label, score, max })
public record SectionItem(
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("value")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Value = null,
    [property: JsonPropertyName("score")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    double? Score = null,
    [property: JsonPropertyName("max")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    double? Max = null
);

// Covers "position", "comparison" rows ({ label/name, value, color? })
public record SectionRow(
    [property: JsonPropertyName("label")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Label = null,
    [property: JsonPropertyName("name")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Name = null,
    [property: JsonPropertyName("value")] string Value = "",
    [property: JsonPropertyName("color")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Color = null
);

// ── LayoutWidgetConfig (new layout format) ─────────────────────────────

public record LayoutWidgetConfig(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("tier")] Tier Tier,
    [property: JsonPropertyName("priority")] int Priority,
    [property: JsonPropertyName("data")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    JsonElement? Data = null
);

// ── SuggestionChip ─────────────────────────────────────────────────────

public record SuggestionChip(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("label")] string Label,
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("icon")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? Icon = null,
    [property: JsonPropertyName("category")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    SuggestionCategory? Category = null,
    [property: JsonPropertyName("is_active")]
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    bool? IsActive = null
);

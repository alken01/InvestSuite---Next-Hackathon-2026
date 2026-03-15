using InvestSuite.Api.Models;

namespace InvestSuite.Api.Infrastructure.Data;

public static class KeyMomentRepository
{
    public static readonly IReadOnlyList<KeyMoment> Moments =
    [
        new(1, new DateOnly(2023, 3, 13),
            "SVB Bank Collapse",
            "Silicon Valley Bank collapses — biggest US bank failure since 2008. European banks sell off on contagion fears.",
            ["KBC", "ABI"],
            "Panic/Fear", "High"),

        new(2, new DateOnly(2023, 7, 19),
            "ASML Earnings Surge",
            "ASML smashes Q2 expectations — bookings up 200%. Chip equipment demand soars on AI buildout.",
            ["ASML", "SAP"],
            "Euphoria", "Moderate"),

        new(3, new DateOnly(2023, 10, 27),
            "Market Bottom",
            "European stocks hit yearly lows — bond yields peak at 5%, recession fears dominate headlines.",
            ["ASML", "ABI", "KBC", "SAP", "LVMH", "TTE", "NOVO", "UNA"],
            "Max pessimism", "High"),

        new(4, new DateOnly(2024, 6, 12),
            "Novo Nordisk Wegovy Peak",
            "Novo Nordisk hits all-time high — Wegovy weight-loss drug demand overwhelms supply chains globally.",
            ["NOVO", "ASML"],
            "FOMO/Greed", "Moderate"),

        new(5, new DateOnly(2024, 7, 17),
            "ASML Chip Restriction Crash",
            "US announces stricter chip export controls to China — ASML drops 7% as Dutch government pressured.",
            ["ASML", "SAP"],
            "Shock", "High"),

        new(6, new DateOnly(2025, 4, 2),
            "Liberation Day Tariffs",
            "Trump announces sweeping 'Liberation Day' tariffs — European markets plunge as trade war fears escalate.",
            ["ASML", "ABI", "KBC", "SAP", "LVMH", "TTE", "NOVO", "UNA"],
            "Broad panic", "High")
    ];
}

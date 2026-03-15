"use client";

import { AccentInsight } from "@/components/ui/accent-bar";
import { WidgetCard } from "@/components/ui/widget-card";
import type { StockPickerWidget } from "@/types/layout";

interface StockPickerProps {
  data: StockPickerWidget["data"];
  onQuery?: (query: string) => void;
}

export function StockPicker({ data, onQuery }: StockPickerProps) {
  const isSellMode = data.mode === "sell";

  return (
    <WidgetCard>
      {/* Header */}
      <div className="mb-[var(--space-lg)]">
        <div
          style={{
            fontSize: "var(--body-size)",
            fontWeight: 500,
            color: "var(--text-primary)",
          }}
        >
          {data.title}
        </div>
        {data.subtitle && (
          <div
            className="mt-[2px]"
            style={{
              fontSize: "var(--text-small)",
              color: "var(--text-mid)",
              lineHeight: 1.4,
            }}
          >
            {data.subtitle}
          </div>
        )}
        {!isSellMode && (
          <div
            className="mt-[var(--space-sm)]"
            style={{
              fontSize: "var(--text-xs)",
              color: "var(--text-dim)",
              letterSpacing: "0.3px",
              textTransform: "uppercase",
            }}
          >
            Available {data.currency}{" "}
            {data.available_cash.toLocaleString(undefined, {
              minimumFractionDigits: 2,
            })}
          </div>
        )}
      </div>

      {/* Stock List */}
      <div className="flex flex-col gap-[var(--space-xs)]">
        {data.stocks.map((stock) => {
          const isNegative = stock.delta_pct < 0;
          return (
            <button
              key={stock.symbol}
              onClick={() => onQuery?.(stock.tap_query)}
              className="cursor-pointer flex items-center gap-[var(--space-md)] rounded-[var(--r-md)] transition-colors text-left w-full p-[var(--space-md)] bg-[var(--surface)] border border-transparent hover:bg-[var(--surface-hover)] hover:border-[var(--border)]"
            >
              {/* Ticker column */}
              <div className="shrink-0" style={{ width: 52 }}>
                <div
                  style={{
                    fontSize: "var(--body-size)",
                    fontWeight: 500,
                    color: "var(--text-primary)",
                    letterSpacing: "-0.2px",
                  }}
                >
                  {stock.symbol}
                </div>
                {!isSellMode && stock.is_held && (
                  <span
                    style={{
                      display: "inline-block",
                      marginTop: 2,
                      fontSize: "var(--text-micro)",
                      padding: "1px 6px",
                      borderRadius: "var(--r-full)",
                      background: "var(--green-muted)",
                      color: "var(--green)",
                      fontWeight: 500,
                      letterSpacing: "0.3px",
                      textTransform: "uppercase",
                    }}
                  >
                    {stock.shares_owned != null ? `${stock.shares_owned.toFixed(stock.shares_owned % 1 === 0 ? 0 : 2)} owned` : "Held"}
                  </span>
                )}
              </div>

              {/* Name + details */}
              <div className="flex-1 min-w-0">
                <div
                  className="truncate"
                  style={{
                    fontSize: "var(--text-small)",
                    color: "var(--text-secondary)",
                  }}
                >
                  {stock.name}
                </div>
                {isSellMode && stock.shares_owned != null ? (
                  <div
                    style={{
                      fontSize: "var(--text-xs)",
                      color: "var(--text-dim)",
                      marginTop: 1,
                      lineHeight: 1.3,
                    }}
                  >
                    {stock.shares_owned} share{stock.shares_owned !== 1 ? "s" : ""} · {data.currency}{stock.current_value?.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                  </div>
                ) : (
                  stock.description && (
                    <div
                      className="line-clamp-1"
                      style={{
                        fontSize: "var(--text-xs)",
                        color: "var(--text-dim)",
                        marginTop: 1,
                        lineHeight: 1.3,
                      }}
                    >
                      {stock.description}
                    </div>
                  )
                )}
              </div>

              {/* Price + delta / Return */}
              <div className="text-right shrink-0">
                {isSellMode && stock.return_pct != null ? (
                  <>
                    <div
                      style={{
                        fontSize: "var(--text-small)",
                        color: stock.return_pct >= 0 ? "var(--green)" : "var(--red)",
                        fontWeight: 500,
                      }}
                    >
                      {stock.return_pct >= 0 ? "+" : ""}{stock.return_pct.toFixed(1)}%
                    </div>
                    <div
                      style={{
                        fontSize: "var(--text-xs)",
                        color: "var(--text-dim)",
                        fontWeight: 400,
                      }}
                    >
                      {data.currency}{stock.price.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </div>
                  </>
                ) : (
                  <>
                    <div
                      style={{
                        fontSize: "var(--text-small)",
                        color: "var(--text-primary)",
                        fontWeight: 500,
                      }}
                    >
                      {stock.price.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </div>
                    <div
                      style={{
                        fontSize: "var(--text-xs)",
                        color: isNegative ? "var(--red)" : "var(--green)",
                        fontWeight: 400,
                      }}
                    >
                      {stock.delta_pct >= 0 ? "+" : ""}{stock.delta_pct.toFixed(1)}%
                    </div>
                  </>
                )}
              </div>
            </button>
          );
        })}
      </div>

      {/* Empty state for sell mode */}
      {isSellMode && data.stocks.length === 0 && (
        <div
          className="text-center py-[var(--space-xl)]"
          style={{
            fontSize: "var(--text-small)",
            color: "var(--text-dim)",
          }}
        >
          You don&apos;t own any stocks yet
        </div>
      )}

      {/* AI Context */}
      {data.ai_context && (
        <div
          className="mt-[var(--space-lg)] pt-[var(--space-md)]"
          style={{ borderTop: "1px solid var(--border)" }}
        >
          <AccentInsight
            text={data.ai_context}
            accent={data.ai_context_accent ?? "blue"}
          />
        </div>
      )}
    </WidgetCard>
  );
}

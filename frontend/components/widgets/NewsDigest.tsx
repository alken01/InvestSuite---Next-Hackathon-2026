"use client";

import type { NewsDigestWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { StatusBadge } from "@/components/ui/status-badge";

interface NewsDigestProps {
  data: NewsDigestWidget["data"];
}

export function NewsDigest({ data }: NewsDigestProps) {
  const items = data.items ?? [];

  return (
    <WidgetCard>
      <div
        style={{
          fontSize: "var(--text-xs)",
          color: "var(--text-dim)",
          textTransform: "uppercase",
          letterSpacing: "0.5px",
          marginBottom: "var(--space-lg)",
        }}
      >
        News
      </div>

      <div className="flex flex-col">
        {items.map((item, i) => {
          const sentimentColor =
            item.sentiment === "positive" ? "var(--green)" :
            item.sentiment === "negative" ? "var(--red)" :
            "var(--text-ghost)";

          return (
            <div
              key={i}
              className="flex gap-[var(--space-md)]"
              style={{
                padding: "var(--space-md) 0",
                borderTop: i > 0 ? "1px solid var(--border)" : "none",
              }}
            >
              {/* Sentiment indicator */}
              <div
                className="shrink-0 mt-[5px]"
                style={{
                  width: 3,
                  height: 16,
                  borderRadius: 2,
                  background: sentimentColor,
                }}
              />

              <div className="flex-1 min-w-0">
                <div style={{ fontSize: "var(--text-small)", color: "var(--text-primary)", lineHeight: 1.45 }}>
                  {item.headline}
                </div>
                <div className="flex items-center gap-[var(--space-sm)] mt-[var(--space-xs)]">
                  <span style={{ fontSize: "var(--text-xs)", color: "var(--text-dim)" }}>
                    {item.source} · {item.time_ago}
                  </span>
                  {item.related_symbol && (
                    <StatusBadge
                      variant={
                        item.sentiment === "positive" ? "positive" :
                        item.sentiment === "negative" ? "negative" :
                        "neutral"
                      }
                    >
                      {item.related_symbol}
                    </StatusBadge>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </WidgetCard>
  );
}

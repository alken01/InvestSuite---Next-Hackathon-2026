"use client";

import type { DividendsWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { StatusBadge } from "@/components/ui/status-badge";

interface DividendsProps {
  data: DividendsWidget["data"];
}

export function Dividends({ data }: DividendsProps) {
  const breakdown = data.breakdown ?? [];

  return (
    <WidgetCard>
      {/* Header */}
      <div className="flex items-baseline justify-between" style={{ marginBottom: "var(--space-lg)" }}>
        <div>
          <div
            style={{
              fontSize: "var(--text-xs)",
              color: "var(--text-dim)",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
              marginBottom: "var(--space-xs)",
            }}
          >
            Dividends · {data.period}
          </div>
          <span className="font-display" style={{ fontSize: "var(--text-hero-sm)", color: "var(--green)" }}>
            {data.total}
          </span>
        </div>
      </div>

      {/* Breakdown */}
      <div className="flex flex-col">
        {breakdown.map((item, i) => (
          <div
            key={i}
            className="flex items-center justify-between"
            style={{
              padding: "var(--space-md) 0",
              borderTop: i > 0 ? "1px solid var(--border)" : "none",
            }}
          >
            <div>
              <div style={{ fontSize: "var(--body-size)", fontWeight: 500, color: "var(--text-primary)" }}>
                {item.symbol}
              </div>
              <div style={{ fontSize: "var(--text-xs)", color: "var(--text-dim)" }}>{item.date}</div>
            </div>
            <div className="flex items-center gap-[var(--space-md)]">
              <span style={{ fontSize: "var(--body-size)", fontWeight: 500, color: "var(--text-secondary)" }}>
                {item.amount}
              </span>
              <StatusBadge variant={item.status}>
                {item.status === "paid" ? "Paid" : "Upcoming"}
              </StatusBadge>
            </div>
          </div>
        ))}
      </div>
    </WidgetCard>
  );
}

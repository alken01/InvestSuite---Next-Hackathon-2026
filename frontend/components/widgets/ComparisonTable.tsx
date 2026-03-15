"use client";

import type { ComparisonTableWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { StatusBadge } from "@/components/ui/status-badge";

interface ComparisonTableProps {
  data: ComparisonTableWidget["data"];
}

export function ComparisonTable({ data }: ComparisonTableProps) {
  const columns = data.columns ?? [];
  const rows = data.rows ?? [];
  const gridCols = `1fr ${columns.slice(1).map(() => "auto").join(" ")}`;

  return (
    <WidgetCard>
      {/* Column headers */}
      <div
        style={{
          display: "grid",
          gridTemplateColumns: gridCols,
          gap: "var(--space-md)",
          paddingBottom: "var(--space-md)",
          borderBottom: "1px solid var(--border)",
        }}
      >
        {columns.map((col, i) => (
          <span
            key={i}
            style={{
              fontSize: "var(--text-xs)",
              color: "var(--text-dim)",
              textTransform: "uppercase",
              letterSpacing: "0.5px",
              textAlign: i > 0 ? "right" : "left",
            }}
          >
            {col}
          </span>
        ))}
      </div>

      {/* Rows */}
      {rows.map((row, ri) => (
        <div
          key={ri}
          style={{
            display: "grid",
            gridTemplateColumns: gridCols,
            gap: "var(--space-md)",
            alignItems: "center",
            padding: "var(--space-md) 0",
            borderBottom: ri < rows.length - 1 ? "1px solid var(--border)" : "none",
          }}
        >
          <span className="flex items-center gap-[var(--space-sm)]">
            <span style={{ fontSize: "var(--body-size)", fontWeight: 500, color: "var(--text-primary)" }}>
              {row.symbol}
            </span>
            {row.is_held && <StatusBadge variant="held">held</StatusBadge>}
          </span>
          {(row.values ?? []).map((val, vi) => (
            <span
              key={vi}
              style={{
                fontSize: "var(--text-small)",
                textAlign: "right",
                fontWeight: vi === (row.values ?? []).length - 1 ? 500 : 400,
                color:
                  vi === (row.values ?? []).length - 1 && row.last_col_color
                    ? row.last_col_color
                    : "var(--text-secondary)",
              }}
            >
              {val}
            </span>
          ))}
        </div>
      ))}
    </WidgetCard>
  );
}

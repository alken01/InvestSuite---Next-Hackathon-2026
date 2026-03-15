"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface HeatmapCell {
  label: string;
  value: number;
  formatValue?: (v: number) => string;
}

function getCellColors(value: number) {
  if (value > 0.5) return { bg: "var(--green-muted)", color: "var(--green)" };
  if (value < -0.5) return { bg: "var(--red-muted)", color: "var(--red)" };
  return { bg: "var(--surface)", color: "var(--text-dim)" };
}

interface HeatmapGridProps {
  cells: HeatmapCell[];
  columns?: number;
  className?: string;
}

export function HeatmapGrid({ cells, columns = 4, className }: HeatmapGridProps) {
  return (
    <div
      className={cn(className)}
      style={{
        display: "grid",
        gridTemplateColumns: `repeat(${columns}, 1fr)`,
        gap: "var(--space-sm)",
      }}
    >
      {cells.map((cell, i) => {
        const colors = getCellColors(cell.value);
        const formatted = cell.formatValue
          ? cell.formatValue(cell.value)
          : `${cell.value > 0 ? "+" : ""}${cell.value.toFixed(1)}%`;

        return (
          <div
            key={i}
            className="rounded-[var(--r-md)]"
            style={{
              aspectRatio: "1.4",
              background: colors.bg,
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              justifyContent: "center",
              gap: 3,
            }}
          >
            <span style={{ fontSize: "var(--text-small)", fontWeight: 500, color: colors.color, letterSpacing: "-0.2px" }}>
              {cell.label}
            </span>
            <span style={{ fontSize: "var(--text-xs)", color: colors.color, opacity: 0.8 }}>
              {formatted}
            </span>
          </div>
        );
      })}
    </div>
  );
}

"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface MetricItem {
  value: string;
  label: string;
  color?: string;
}

interface MetricGridProps {
  items: MetricItem[];
  columns?: number;
  className?: string;
}

export function MetricGrid({ items, columns = 3, className }: MetricGridProps) {
  return (
    <div
      className={cn(className)}
      style={{
        display: "grid",
        gridTemplateColumns: `repeat(${columns}, 1fr)`,
        gap: "var(--space-md)",
      }}
    >
      {items.map((item, i) => (
        <div
          key={i}
          className="rounded-[var(--r-md)]"
          style={{
            background: "var(--surface)",
            padding: "var(--space-md)",
            textAlign: "center",
          }}
        >
          <div
            className="font-display"
            style={{
              fontSize: "var(--text-metric)",
              fontWeight: 400,
              color: item.color ?? "var(--text-primary)",
            }}
          >
            {item.value}
          </div>
          <div className="micro-label" style={{ marginTop: "var(--space-xs)" }}>
            {item.label}
          </div>
        </div>
      ))}
    </div>
  );
}

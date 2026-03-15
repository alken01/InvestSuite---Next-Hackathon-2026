"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface DataListItem {
  label: string;
  value: string;
  labelColor?: string;
  valueColor?: string;
}

interface DataListProps {
  items: DataListItem[];
  className?: string;
}

export function DataList({ items, className }: DataListProps) {
  return (
    <div className={cn(className)}>
      {items.map((item, i) => (
        <div
          key={i}
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            padding: "var(--space-md) 0",
            borderTop: i > 0 ? "1px solid var(--border)" : "none",
          }}
        >
          <span style={{ fontSize: "var(--text-small)", color: item.labelColor ?? "var(--text-mid)" }}>
            {item.label}
          </span>
          <span style={{ fontSize: "var(--text-small)", color: item.valueColor ?? "var(--text-primary)" }}>
            {item.value}
          </span>
        </div>
      ))}
    </div>
  );
}

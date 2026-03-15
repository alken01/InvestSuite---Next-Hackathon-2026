"use client";

import React from "react";
import { WidgetCard } from "@/components/ui/widget-card";
import { DataList } from "@/components/ui/data-list";

interface PositionSectionProps {
  type: "position";
  rows: { label: string; value: string; color?: string }[];
}

export function PositionSection({ rows }: PositionSectionProps) {
  return (
    <WidgetCard>
      <div className="micro-label mb-[var(--space-sm)]">
        Your Position
      </div>
      <DataList
        items={(rows ?? []).map((row) => ({
          label: row.label,
          value: row.value,
          valueColor: row.color,
        }))}
      />
    </WidgetCard>
  );
}

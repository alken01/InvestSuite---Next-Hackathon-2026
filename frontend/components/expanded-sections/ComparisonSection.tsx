"use client";

import React from "react";
import { WidgetCard } from "@/components/ui/widget-card";
import { DataList } from "@/components/ui/data-list";

interface ComparisonSectionProps {
  type: "comparison";
  rows: { name: string; value: string; color?: string }[];
}

export function ComparisonSection({ rows }: ComparisonSectionProps) {
  return (
    <WidgetCard>
      <DataList
        items={(rows ?? []).map((row) => ({
          label: row.name,
          value: row.value,
          valueColor: row.color,
        }))}
      />
    </WidgetCard>
  );
}

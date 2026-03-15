"use client";

import type { SectorHeatmapWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { HeatmapGrid } from "@/components/ui/heatmap-grid";

interface SectorHeatmapProps {
  data: SectorHeatmapWidget["data"];
}

export function SectorHeatmap({ data }: SectorHeatmapProps) {
  return (
    <WidgetCard>
      <div className="flex items-baseline justify-between" style={{ marginBottom: "var(--space-lg)" }}>
        <span style={{ fontSize: "var(--body-size)", fontWeight: 500, color: "var(--text-primary)" }}>
          {data.title}
        </span>
        <span style={{ fontSize: "var(--text-xs)", color: "var(--text-dim)" }}>
          {data.period}
        </span>
      </div>
      <HeatmapGrid
        cells={(data.items ?? []).map((item) => ({
          label: item.symbol,
          value: item.delta,
        }))}
      />
    </WidgetCard>
  );
}

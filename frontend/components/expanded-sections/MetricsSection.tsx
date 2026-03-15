"use client";

import React from "react";
import { MetricGrid } from "@/components/ui/metric-grid";

interface MetricsSectionProps {
  type: "metrics";
  items: { value: string; label: string }[];
}

export function MetricsSection({ items }: MetricsSectionProps) {
  return <MetricGrid items={items} />;
}

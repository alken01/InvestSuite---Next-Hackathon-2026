"use client";

import type { AIInsightWidget } from "@/types/layout";
import { WidgetCard } from "@/components/ui/widget-card";
import { AccentInsight } from "@/components/ui/accent-bar";

interface AIInsightProps {
  data: AIInsightWidget["data"];
}

export function AIInsight({ data }: AIInsightProps) {
  return (
    <WidgetCard>
      <AccentInsight text={data.text} accent={data.accent} footnote={data.footnote} />
    </WidgetCard>
  );
}

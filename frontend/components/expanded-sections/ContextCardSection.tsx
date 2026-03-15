"use client";

import React from "react";
import { WidgetCard, WidgetLabel } from "@/components/ui/widget-card";

interface ContextCardSectionProps {
  type: "context_card";
  text: string;
}

export function ContextCardSection({ text }: ContextCardSectionProps) {
  return (
    <WidgetCard>
      <p style={{ lineHeight: 1.5, margin: 0 }}>
        <WidgetLabel size="small">{text}</WidgetLabel>
      </p>
    </WidgetCard>
  );
}

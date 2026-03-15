"use client";

import React from "react";
import { AccentInsight } from "@/components/ui/accent-bar";

interface InsightSectionProps {
  type: "insight";
  text: string;
  accent: "green" | "blue" | "amber";
}

export function InsightSection({ text, accent }: InsightSectionProps) {
  return <AccentInsight text={text} accent={accent} />;
}

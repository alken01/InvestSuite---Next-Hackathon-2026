"use client";

import React from "react";
import { cn } from "@/lib/utils";

const presets: Record<string, { bg: string; color: string }> = {
  positive: { bg: "var(--green-subtle)", color: "var(--green)" },
  negative: { bg: "var(--red-subtle)", color: "var(--red)" },
  info: { bg: "var(--blue-subtle)", color: "var(--blue)" },
  warning: { bg: "var(--amber-subtle)", color: "var(--amber)" },
  neutral: { bg: "var(--surface)", color: "var(--text-dim)" },
  held: { bg: "var(--green-subtle)", color: "var(--green)" },
  paid: { bg: "var(--green-subtle)", color: "var(--green)" },
  upcoming: { bg: "var(--blue-subtle)", color: "var(--blue)" },
  pending: { bg: "var(--blue-subtle)", color: "var(--blue)" },
  expired: { bg: "var(--amber-subtle)", color: "var(--amber)" },
  filled: { bg: "var(--green-subtle)", color: "var(--green)" },
};

interface StatusBadgeProps {
  children: React.ReactNode;
  variant?: keyof typeof presets;
  bg?: string;
  color?: string;
  className?: string;
}

export function StatusBadge({ children, variant = "neutral", bg, color, className }: StatusBadgeProps) {
  const preset = presets[variant] ?? presets.neutral;

  return (
    <span
      className={cn("badge", className)}
      style={{
        background: bg ?? preset.bg,
        color: color ?? preset.color,
      }}
    >
      {children}
    </span>
  );
}

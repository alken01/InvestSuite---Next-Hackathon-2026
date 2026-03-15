"use client";

import { cn } from "@/lib/utils";

const accentColors: Record<string, string> = {
  green: "var(--green)",
  blue: "var(--blue)",
  amber: "var(--amber)",
  red: "var(--red)",
  neutral: "var(--text-dim)",
};

interface AccentBarProps {
  accent: string;
  className?: string;
}

export function AccentBar({ accent, className }: AccentBarProps) {
  return (
    <div
      className={cn("accent-bar", className)}
      style={{ backgroundColor: accentColors[accent] ?? accentColors.neutral }}
    />
  );
}

interface AccentInsightProps {
  text: string;
  accent: string;
  footnote?: string;
  className?: string;
}

export function AccentInsight({ text, accent, footnote, className }: AccentInsightProps) {
  return (
    <div className={cn("flex", className)}>
      <AccentBar accent={accent} />
      <div style={{ paddingLeft: "var(--space-md)" }}>
        <p
          className="font-display"
          style={{
            fontStyle: "italic",
            fontSize: "var(--body-size)",
            color: "var(--text-secondary)",
            lineHeight: 1.5,
            margin: 0,
          }}
        >
          {text}
        </p>
        {footnote && (
          <p
            style={{
              fontSize: "var(--text-xs)",
              color: "var(--text-dim)",
              marginTop: "var(--space-sm)",
              marginBottom: 0,
            }}
          >
            {footnote}
          </p>
        )}
      </div>
    </div>
  );
}

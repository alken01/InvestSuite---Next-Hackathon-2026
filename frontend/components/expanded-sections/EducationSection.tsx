"use client";

import React from "react";

interface EducationSectionProps {
  type: "education";
  title: string;
  body: string;
  accent?: "green" | "blue";
}

const accentStyles: Record<string, { bg: string; border: string; color: string }> = {
  blue: {
    bg: "rgba(138,180,248,0.04)",
    border: "rgba(138,180,248,0.08)",
    color: "var(--blue)",
  },
  green: {
    bg: "rgba(126,219,168,0.04)",
    border: "rgba(126,219,168,0.08)",
    color: "var(--green)",
  },
};

export function EducationSection({ title, body, accent = "blue" }: EducationSectionProps) {
  const style = accentStyles[accent] ?? accentStyles.blue;

  return (
    <div
      style={{
        background: style.bg,
        border: `1px solid ${style.border}`,
        borderRadius: "var(--r-lg)",
        padding: "var(--card-padding)",
      }}
    >
      <div
        style={{
          fontSize: "var(--text-small)",
          fontWeight: 500,
          color: style.color,
          marginBottom: "var(--space-sm)",
        }}
      >
        {title}
      </div>
      <div
        style={{
          fontSize: "var(--text-small)",
          color: "var(--text-mid)",
          lineHeight: 1.5,
        }}
      >
        {body}
      </div>
    </div>
  );
}

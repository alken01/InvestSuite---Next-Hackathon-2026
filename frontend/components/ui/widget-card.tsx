"use client";

import { cn } from "@/lib/utils";

interface WidgetCardProps {
  children: React.ReactNode;
  className?: string;
  noPadding?: boolean;
}

export function WidgetCard({ children, className, noPadding }: WidgetCardProps) {
  return (
    <div
      className={cn("rounded-[var(--r-lg)]", className)}
      style={{
        background: "var(--surface)",
        border: "1px solid var(--border)",
        padding: noPadding ? undefined : "var(--card-padding) var(--space-2xl)",
      }}
    >
      {children}
    </div>
  );
}

interface WidgetHeaderProps {
  title: string;
  trailing?: React.ReactNode;
  className?: string;
}

export function WidgetHeader({ title, trailing, className }: WidgetHeaderProps) {
  return (
    <div
      className={cn("flex items-center justify-between", className)}
      style={{ marginBottom: "var(--space-md)" }}
    >
      <span style={{ fontSize: "var(--text-small)", fontWeight: 500, color: "var(--text-primary)" }}>
        {title}
      </span>
      {trailing && (
        <span style={{ fontSize: "var(--text-xs)", color: "var(--text-dim)" }}>
          {trailing}
        </span>
      )}
    </div>
  );
}

interface WidgetRowProps {
  children: React.ReactNode;
  showBorder?: boolean;
  className?: string;
}

export function WidgetRow({ children, showBorder, className }: WidgetRowProps) {
  return (
    <div
      className={cn("flex items-center justify-between", className)}
      style={{
        padding: "var(--space-md) 0",
        borderTop: showBorder ? "1px solid var(--border)" : "none",
      }}
    >
      {children}
    </div>
  );
}

interface WidgetLabelProps {
  children: React.ReactNode;
  color?: string;
  size?: "body" | "small" | "xs" | "label";
  weight?: number;
}

export function WidgetLabel({ children, color, size = "small", weight }: WidgetLabelProps) {
  const sizeMap = {
    body: "var(--body-size)",
    small: "var(--text-small)",
    xs: "var(--text-xs)",
    label: "var(--label-size)",
  };

  return (
    <span style={{ fontSize: sizeMap[size], color: color ?? "var(--text-mid)", fontWeight: weight }}>
      {children}
    </span>
  );
}

interface WidgetValueProps {
  children: React.ReactNode;
  color?: string;
  size?: "hero" | "hero-md" | "hero-sm" | "metric" | "body" | "small";
  display?: boolean;
}

export function WidgetValue({ children, color, size = "body", display }: WidgetValueProps) {
  const sizeMap = {
    hero: "var(--text-hero)",
    "hero-md": "var(--text-hero-md)",
    "hero-sm": "var(--text-hero-sm)",
    metric: "var(--text-metric)",
    body: "var(--body-size)",
    small: "var(--text-small)",
  };

  return (
    <span
      className={display ? "font-display" : undefined}
      style={{
        fontSize: sizeMap[size],
        color: color ?? "var(--text-primary)",
      }}
    >
      {children}
    </span>
  );
}

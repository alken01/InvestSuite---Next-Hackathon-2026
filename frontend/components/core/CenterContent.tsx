"use client";

import { motion } from "framer-motion";
import type { CenterContent } from "@/types/layout";

interface CenterContentProps {
  content: CenterContent | undefined;
  visible?: boolean;
  compact?: boolean;
}

const DELTA_COLOR_MAP = {
  positive: "var(--green)",
  negative: "var(--red)",
  neutral: "var(--text-mid)",
} as const;

export function CenterContent({
  content,
  visible = true,
  compact = false,
}: CenterContentProps) {
  if (!visible || !content) return null;

  const fontSize = compact ? "var(--text-hero-sm)" : "var(--text-hero)";
  const opacity = compact ? 0.4 : 1;
  const deltaColor = content.delta_color
    ? DELTA_COLOR_MAP[content.delta_color] ?? "var(--text-mid)"
    : "var(--text-mid)";

  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.92 }}
      animate={{ opacity: 1, scale: 1 }}
      exit={{ opacity: 0, scale: 0.92 }}
      transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
      className="flex flex-col items-center"
      style={{ opacity }}
    >
      <span
        className="font-display font-light text-white"
        style={{
          fontSize,
          fontWeight: 300,
          letterSpacing: "-1.8px",
          lineHeight: 1.1,
        }}
      >
        {content.value}
      </span>

      {content.label && (
        <span
          className="mt-2"
          style={{
            fontSize: "var(--label-size)",
            color: "var(--text-dim)",
          }}
        >
          {content.label}
        </span>
      )}

      {content.delta && (
        <div className="mt-2 flex items-center gap-1.5">
          <span
            style={{
              fontSize: "var(--body-size)",
              color: deltaColor,
            }}
          >
            {content.delta}
          </span>
          {content.delta_label && (
            <span
              style={{
                fontSize: "var(--body-size)",
                color: "var(--text-dim)",
              }}
            >
              {content.delta_label}
            </span>
          )}
        </div>
      )}
    </motion.div>
  );
}

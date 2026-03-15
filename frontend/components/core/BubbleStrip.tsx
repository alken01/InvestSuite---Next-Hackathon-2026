"use client";

import { motion } from "framer-motion";
import type { BubbleStripItem } from "@/types/layout";

interface BubbleStripProps {
  items: BubbleStripItem[];
  onTap: (query: string) => void;
}

export function BubbleStrip({ items, onTap }: BubbleStripProps) {
  return (
    <div
      className="absolute z-10 flex items-center justify-center gap-3"
      style={{
        top: "var(--bubble-strip-top)",
        left: "50%",
        transform: "translateX(-50%)",
      }}
    >
      {items.map((item) => (
        <motion.button
          key={item.id}
          whileTap={{ scale: 0.9 }}
          onClick={() => item.tap_query && onTap(item.tap_query)}
          className="flex items-center justify-center rounded-full"
          style={{
            minWidth: "var(--bubble-strip-size)",
            height: "var(--bubble-strip-size)",
            paddingInline: 10,
            background: "var(--surface)",
            border: item.is_active
              ? "1px solid rgba(138,180,248,0.35)"
              : "1px solid var(--border-light)",
            opacity: item.is_active ? 1 : 0.5,
            cursor: "pointer",
          }}
        >
          <span
            className="text-white"
            style={{
              fontSize: "var(--text-xs)",
              fontWeight: 500,
              letterSpacing: "0.4px",
            }}
          >
            {item.ticker}
          </span>
        </motion.button>
      ))}
    </div>
  );
}

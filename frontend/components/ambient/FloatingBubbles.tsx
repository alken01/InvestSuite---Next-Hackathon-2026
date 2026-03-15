"use client";

import { useMemo } from "react";
import { BubbleConfig } from "@/types/layout";
import { motion } from "framer-motion";

interface FloatingBubblesProps {
  bubbles: BubbleConfig[];
  onBubbleTap: (query: string) => void;
}

// Map portfolio weight (0–1) to pixel diameter: 44px–80px
// Minimum 44px ensures text always fits inside the bubble
function weightToPx(weight: number): number {
  const MIN = 44;
  const MAX = 80;
  return Math.round(MIN + Math.sqrt(weight) * (MAX - MIN));
}

const BG_MAP: Record<BubbleConfig["direction"], string> = {
  up: "var(--green-subtle)",
  down: "var(--red-subtle)",
  neutral: "var(--surface)",
};

// Safe zones (percentage of container)
// SAFE_BOTTOM at 55% keeps bubbles well above suggestion chips + input gradient
const SAFE_TOP = 42;
const SAFE_BOTTOM = 55;
const SAFE_LEFT = 6;
const SAFE_RIGHT = 94;
const MIN_GAP = 8;

// Spread initial positions across the safe zone deterministically
const SEED_TOPS  = [44, 52, 46, 48, 53, 50, 43, 51, 47, 49];
const SEED_LEFTS = [20, 75, 48, 85, 12, 62, 35, 80, 25, 55];

interface ResolvedItem {
  top: string;
  left: string;
  sizePx: number;
}

function resolveLayout(bubbles: BubbleConfig[]): ResolvedItem[] {
  if (bubbles.length === 0) return [];

  const cw = 400;
  const ch = 800;

  const items = bubbles.map((b, i) => {
    const r = weightToPx(b.weight) / 2;
    return {
      x: ((SEED_LEFTS[i % SEED_LEFTS.length]) / 100) * cw,
      y: ((SEED_TOPS[i % SEED_TOPS.length]) / 100) * ch,
      r,
      sizePx: weightToPx(b.weight),
    };
  });

  const safeTopPx    = (SAFE_TOP / 100) * ch;
  const safeBottomPx = (SAFE_BOTTOM / 100) * ch;
  const safeLeftPx   = (SAFE_LEFT / 100) * cw;
  const safeRightPx  = (SAFE_RIGHT / 100) * cw;

  for (const item of items) {
    item.x = Math.max(safeLeftPx + item.r, Math.min(safeRightPx - item.r, item.x));
    item.y = Math.max(safeTopPx + item.r, Math.min(safeBottomPx - item.r, item.y));
  }

  for (let pass = 0; pass < 50; pass++) {
    let moved = false;
    for (let i = 0; i < items.length; i++) {
      for (let j = i + 1; j < items.length; j++) {
        const a = items[i];
        const b = items[j];
        const dx = b.x - a.x;
        const dy = b.y - a.y;
        const dist = Math.sqrt(dx * dx + dy * dy);
        const minDist = a.r + b.r + MIN_GAP;

        if (dist < minDist && dist > 0) {
          const overlap = (minDist - dist) / 2;
          const nx = dx / dist;
          const ny = dy / dist;
          a.x -= nx * overlap;
          a.y -= ny * overlap;
          b.x += nx * overlap;
          b.y += ny * overlap;
          moved = true;
        } else if (dist === 0) {
          a.x -= 10;
          b.x += 10;
          moved = true;
        }
      }
    }

    for (const item of items) {
      item.x = Math.max(safeLeftPx + item.r, Math.min(safeRightPx - item.r, item.x));
      item.y = Math.max(safeTopPx + item.r, Math.min(safeBottomPx - item.r, item.y));
    }

    if (!moved) break;
  }

  return items.map((item) => ({
    top: `${((item.y / ch) * 100).toFixed(1)}%`,
    left: `${((item.x / cw) * 100).toFixed(1)}%`,
    sizePx: item.sizePx,
  }));
}

function NudgeDot() {
  return (
    <span
      className="absolute rounded-full"
      style={{
        width: "var(--dot-lg)",
        height: "var(--dot-lg)",
        top: "var(--space-xs)",
        right: "var(--space-xs)",
        backgroundColor: "var(--blue)",
        animationName: "nudgePulse",
        animationDuration: "2s",
        animationTimingFunction: "ease-in-out",
        animationIterationCount: "infinite",
      }}
    />
  );
}

export function FloatingBubbles({ bubbles, onBubbleTap }: FloatingBubblesProps) {
  const resolved = useMemo(() => resolveLayout(bubbles), [bubbles]);

  return (
    <div className="absolute inset-0 z-10 pointer-events-none">
      {bubbles.map((bubble, i) => {
        const pos = resolved[i];
        const sizePx = pos?.sizePx ?? 44;
        const bg = BG_MAP[bubble.direction];
        const isHighlighted = bubble.state === "highlighted";
        const isDimmed = bubble.state === "dimmed";
        const isPaused = isHighlighted || isDimmed;

        const borderColor = isHighlighted ? "rgba(138,180,248,0.3)" : "var(--border)";
        const deltaColor =
          bubble.direction === "up"
            ? "var(--green)"
            : bubble.direction === "down"
              ? "var(--red)"
              : "inherit";

        const floatX = i % 2 === 0 ? `${2 + (i % 3)}px` : `-${2 + (i % 3)}px`;
        const floatY = i % 2 === 0 ? `-${2 + (i % 3)}px` : `${2 + (i % 3)}px`;
        const floatDuration = `${8 + i}s`;
        const floatDelay = `${(i * 0.5).toFixed(1)}s`;

        return (
          <motion.div
            key={bubble.id}
            layoutId={bubble.id}
            className="absolute rounded-full flex flex-col items-center justify-center cursor-pointer pointer-events-auto"
            style={{
              width: sizePx,
              height: sizePx,
              padding: 4,
              top: pos?.top,
              left: pos?.left,
              transform: "translate(-50%, -50%)",
              background: bg,
              border: `1.5px solid ${borderColor}`,
              animationName: "bubbleFloat",
              animationDuration: floatDuration,
              animationTimingFunction: "ease-in-out",
              animationIterationCount: "infinite",
              animationDelay: floatDelay,
              animationPlayState: isPaused ? "paused" : "running",
              "--fx": floatX,
              "--fy": floatY,
            } as React.CSSProperties}
            animate={{
              scale: isHighlighted ? 1.2 : 1,
              opacity: isDimmed ? 0.25 : 1,
            }}
            whileHover={{ scale: isHighlighted ? 1.2 : 1.15 }}
            transition={{ duration: 0.3, ease: [0.16, 1, 0.3, 1] }}
            onClick={() => bubble.tap_query && onBubbleTap(bubble.tap_query)}
          >
            <span
              className="leading-none select-none truncate"
              style={{
                fontSize: sizePx < 50 ? "var(--text-2xs)" : "var(--label-size)",
                fontWeight: 500,
                color: "var(--text-secondary)",
                maxWidth: sizePx - 10,
              }}
            >
              {bubble.ticker}
            </span>
            <span
              className="leading-none select-none truncate"
              style={{
                fontSize: "var(--text-2xs)",
                color: deltaColor,
                marginTop: 2,
                maxWidth: sizePx - 10,
              }}
            >
              {bubble.delta}
            </span>

            {bubble.has_nudge && <NudgeDot />}
          </motion.div>
        );
      })}
    </div>
  );
}

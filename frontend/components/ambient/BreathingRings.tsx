"use client";

import { Sentiment } from "@/types/layout";

import { AMBIENT_THEMES } from "@/lib/ambient";
import { motion } from "framer-motion";

interface BreathingRingsProps {
  sentiment: Sentiment;
  visible?: boolean;
}

const RING_VARS = ["var(--ring-sm)", "var(--ring-md)", "var(--ring-lg)"];

export function BreathingRings({ sentiment, visible = true }: BreathingRingsProps) {
  const theme = AMBIENT_THEMES[sentiment] ?? AMBIENT_THEMES[Sentiment.Calm];

  const ringColor =
    sentiment === Sentiment.Greed ? "rgba(126,219,168,0.06)" : theme.ringColor;

  const duration =
    sentiment === Sentiment.Fear
      ? "14s"
      : sentiment === Sentiment.Greed
        ? "6s"
        : theme.breathingSpeed;

  return (
    <motion.div
      className="absolute inset-0 z-0 pointer-events-none"
      initial={{ opacity: 0 }}
      animate={{ opacity: visible ? 1 : 0 }}
      transition={{ duration: 0.8, ease: [0.16, 1, 0.3, 1] }}
    >
      {RING_VARS.map((sizeVar, i) => (
        <div
          key={i}
          className="absolute rounded-full"
          style={{
            width: sizeVar,
            height: sizeVar,
            top: "var(--breathing-center)",
            left: "50%",
            transform: "translate(-50%, -50%)",
            border: `1px solid ${ringColor}`,
            animationName: "ringPulse",
            animationDuration: duration,
            animationTimingFunction: "ease-in-out",
            animationIterationCount: "infinite",
            animationDelay: `${i * 0.8}s`,
          }}
        />
      ))}
    </motion.div>
  );
}

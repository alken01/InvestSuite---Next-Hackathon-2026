"use client";

import { Sentiment } from "@/types/layout";

import { AMBIENT_THEMES } from "@/lib/ambient";
import { motion, AnimatePresence } from "framer-motion";

interface AmbientBackgroundProps {
  sentiment: Sentiment;
}

export function AmbientBackground({ sentiment }: AmbientBackgroundProps) {
  const theme = AMBIENT_THEMES[sentiment] ?? AMBIENT_THEMES[Sentiment.Calm];
  const bg = [...theme.gradients, "var(--bg)"].join(", ");

  return (
    <AnimatePresence mode="wait">
      <motion.div
        key={sentiment}
        className="absolute inset-0 z-0"
        style={{
          background: bg,
          animationName: "ambientBreathe",
          animationDuration: theme.breathingSpeed,
          animationTimingFunction: "ease-in-out",
          animationIterationCount: "infinite",
        }}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        exit={{ opacity: 0 }}
        transition={{ duration: 1.5, ease: [0.16, 1, 0.3, 1] }}
      />
    </AnimatePresence>
  );
}

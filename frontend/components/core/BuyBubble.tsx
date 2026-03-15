"use client";

import { motion } from "framer-motion";

interface BuyBubbleProps {
  onTap: () => void;
  visible: boolean;
}

export function BuyBubble({ onTap, visible }: BuyBubbleProps) {
  if (!visible) return null;

  return (
    <motion.button
      className="buy-bubble"
      onClick={onTap}
      initial={{ scale: 0, opacity: 0 }}
      animate={{ scale: 1, opacity: 1 }}
      exit={{ scale: 0, opacity: 0 }}
      whileHover={{ scale: 1.08 }}
      whileTap={{ scale: 0.95 }}
      transition={{ type: "spring", stiffness: 400, damping: 25 }}
    >
      <svg
        width="20"
        height="20"
        viewBox="0 0 20 20"
        fill="none"
        stroke="currentColor"
        strokeWidth="2"
        strokeLinecap="round"
      >
        <line x1="10" y1="4" x2="10" y2="16" />
        <line x1="4" y1="10" x2="16" y2="10" />
      </svg>
      <span>Buy</span>
    </motion.button>
  );
}

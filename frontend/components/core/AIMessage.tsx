"use client";

import { motion } from "framer-motion";
import type { AIMessage } from "@/types/layout";

interface AIMessageProps {
  message: AIMessage | undefined;
  visible?: boolean;
}

export function AIMessage({
  message,
  visible = true,
}: AIMessageProps) {
  if (!visible || !message) return null;

  return (
    <motion.div
      initial={{ opacity: 0, y: 8 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -6 }}
      transition={{ duration: 0.5, ease: [0.16, 1, 0.3, 1] }}
      className="flex flex-col items-center"
      style={{
        maxWidth: "85%",
        textAlign: "center",
      }}
    >
      <p
        className="font-display"
        style={{
          fontSize: "var(--text-ai-message)",
          fontWeight: 400,
          fontStyle: "italic",
          color: "var(--text-secondary)",
          lineHeight: 1.5,
        }}
      >
        {message.text}
      </p>

      {message.footnote && (
        <span
          className="font-body mt-3"
          style={{
            fontSize: "var(--text-xs)",
            color: "var(--text-muted)",
          }}
        >
          {message.footnote}
        </span>
      )}
    </motion.div>
  );
}

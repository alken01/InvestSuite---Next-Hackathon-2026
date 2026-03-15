"use client";

import React from "react";
import { cn } from "@/lib/utils";
import { motion } from "framer-motion";

interface ChipProps {
  children: React.ReactNode;
  active?: boolean;
  icon?: string;
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
}

export function Chip({ children, active, icon, onClick, disabled, className }: ChipProps) {
  return (
    <motion.button
      whileTap={{ scale: 0.95 }}
      onClick={onClick}
      disabled={disabled}
      className={cn("overflow-hidden text-ellipsis whitespace-nowrap", className)}
      style={{
        background: active ? "var(--blue-subtle)" : "var(--surface)",
        border: active ? "1px solid rgba(138,180,248,0.25)" : "1px solid var(--border)",
        borderRadius: "var(--r-full)",
        padding: "var(--space-md) var(--space-lg)",
        fontSize: "var(--text-small)",
        color: active ? "var(--blue)" : "var(--text-mid)",
        cursor: disabled ? "wait" : "pointer",
        opacity: disabled ? 0.5 : 1,
      }}
    >
      {icon && <span className="mr-1">{icon}</span>}
      {children}
    </motion.button>
  );
}

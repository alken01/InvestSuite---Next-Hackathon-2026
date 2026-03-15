"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface StatusDotProps {
  color: string;
  size?: "sm" | "md" | "lg";
  className?: string;
}

export function StatusDot({ color, size = "md", className }: StatusDotProps) {
  const sizeMap = {
    sm: "var(--dot-sm)",
    md: "var(--dot-md)",
    lg: "var(--dot-lg)",
  };

  return (
    <div
      className={cn("shrink-0 rounded-full", className)}
      style={{
        width: sizeMap[size],
        height: sizeMap[size],
        backgroundColor: color,
      }}
    />
  );
}

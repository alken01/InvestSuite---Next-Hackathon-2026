"use client";

import React from "react";
import { cn } from "@/lib/utils";

interface ActionButtonProps {
  children: React.ReactNode;
  variant?: "primary" | "secondary";
  fullWidth?: boolean;
  onClick?: () => void;
  disabled?: boolean;
  className?: string;
}

export function ActionButton({
  children,
  variant = "primary",
  fullWidth,
  onClick,
  disabled,
  className,
}: ActionButtonProps) {
  const isPrimary = variant === "primary";

  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={cn(
        "text-center transition-opacity hover:opacity-80 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed",
        fullWidth && "w-full",
        className
      )}
      style={
        isPrimary
          ? {
              background: "var(--green)",
              color: "var(--bg)",
              fontSize: "var(--body-size)",
              fontWeight: 600,
              borderRadius: "var(--r-md)",
              padding: "var(--space-lg) var(--space-xl)",
              border: "none",
            }
          : {
              background: "var(--surface)",
              border: "1px solid var(--border-light)",
              color: "var(--text-secondary)",
              fontSize: "var(--body-size)",
              fontWeight: 500,
              borderRadius: "var(--r-md)",
              padding: "var(--space-lg) var(--space-xl)",
            }
      }
    >
      {children}
    </button>
  );
}

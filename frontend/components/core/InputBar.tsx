"use client";

import { useRef, useState, type FormEvent, type KeyboardEvent } from "react";
import type { SuggestionChip } from "@/types/layout";
import { Chip } from "@/components/ui/chip";

interface InputBarProps {
  placeholder?: string;
  suggestions: SuggestionChip[];
  onSubmit: (query: string) => void;
  onChipTap: (query: string) => void;
  loading?: boolean;
  aiLoading?: boolean;
  hideChips?: boolean;
}

function isBackToHome(query: string) {
  return /^(back to home|go back|home)$/i.test(query);
}

export function InputBar({
  placeholder = "Ask anything...",
  suggestions,
  onSubmit,
  onChipTap,
  loading = false,
  aiLoading = false,
  hideChips = false,
}: InputBarProps) {
  const [value, setValue] = useState("");
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const filteredChips = suggestions
    .filter((c) => !isBackToHome(c.query))
    .sort((a, b) => (a.is_active === b.is_active ? 0 : a.is_active ? 1 : -1));
  const maxRows = 4;
  const lineHeight = 22;

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    submit();
  }

  function submit() {
    const trimmed = value.trim();
    if (!trimmed || loading) return;
    onSubmit(trimmed);
    setValue("");
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
    }
  }

  function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      submit();
    }
  }

  function handleInput(newValue: string) {
    setValue(newValue);
    const el = textareaRef.current;
    if (!el) return;
    el.style.height = "auto";
    el.style.height = `${Math.min(el.scrollHeight, lineHeight * maxRows)}px`;
  }

  return (
    <div className="absolute bottom-0 left-0 right-0 z-30 flex flex-col items-center">
      {/* Gradient overlay */}
      <div
        className="pointer-events-none absolute inset-x-0 bottom-0"
        style={{
          height: "var(--input-gradient-height)",
          background: "linear-gradient(to bottom, transparent, var(--bg))",
        }}
      />

      {/* Suggestion chips — 2-column grid, max 2 rows */}
      {filteredChips.length > 0 && !hideChips && (
        <div
          className="relative z-10 grid grid-cols-2 gap-2"
          style={{ marginBottom: "var(--space-md)", paddingLeft: "var(--app-px)", paddingRight: "var(--app-px)" }}
        >
          {filteredChips.slice(0, 4).map((chip, i) => {
            const isLastOdd =
              i === filteredChips.slice(0, 4).length - 1 &&
              filteredChips.slice(0, 4).length % 2 === 1;
            return (
              <Chip
                key={chip.id}
                active={chip.is_active}
                icon={chip.icon}
                onClick={() => onChipTap(chip.query)}
                disabled={loading || aiLoading}
                className={`truncate ${isLastOdd ? "col-span-2" : ""}`}
              >
                {chip.label}
              </Chip>
            );
          })}
        </div>
      )}

      {/* Input bar */}
      <form
        onSubmit={handleSubmit}
        className="relative z-10 flex w-full items-center"
        style={{ paddingBottom: "var(--space-xl)", paddingLeft: "var(--app-px)", paddingRight: "var(--app-px)" }}
      >
        <div
          className="flex w-full items-start gap-2"
          style={{
            background: "var(--surface)",
            border: "1px solid var(--border-light)",
            borderRadius: "var(--r-lg)",
            padding: "var(--space-md) var(--space-lg)",
          }}
        >
          {(loading || aiLoading) && (
            <span
              style={{
                opacity: 0.6,
                fontSize: "var(--text-title)",
                color: "white",
                flexShrink: 0,
              }}
            >
              ···
            </span>
          )}

          <textarea
            ref={textareaRef}
            value={value}
            onChange={(e) => handleInput(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={placeholder}
            disabled={loading}
            rows={1}
            className="flex-1 resize-none text-white placeholder:text-white/20"
            style={{
              background: "transparent",
              border: "none",
              outline: "none",
              fontSize: "var(--text-title)",
              fontFamily: "inherit",
              lineHeight: `${lineHeight}px`,
              maxHeight: `${lineHeight * maxRows}px`,
              overflowY: "auto",
            }}
          />
        </div>
      </form>
    </div>
  );
}

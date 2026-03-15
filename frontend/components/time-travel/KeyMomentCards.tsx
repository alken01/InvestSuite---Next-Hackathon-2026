"use client";

import React from "react";
import type { KeyMoment } from "@/types/layout";

interface KeyMomentCardsProps {
  moments: KeyMoment[];
  activeDate: string | undefined;
  onSelect: (date: string | undefined) => void;
  disabled?: boolean;
}

function formatDate(dateStr: string): string {
  const d = new Date(dateStr + "T00:00:00");
  return d.toLocaleDateString("en-GB", { month: "short", year: "numeric" });
}

export function KeyMomentCards({ moments, activeDate, onSelect, disabled }: KeyMomentCardsProps) {
  const isNowActive = activeDate === undefined;

  return (
    <div className="key-moments-bar">
      {/* Now card */}
      <button
        onClick={() => onSelect(undefined)}
        disabled={disabled}
        className={`key-moment-card key-moment-now ${isNowActive ? "isActive" : ""} ${disabled ? "isDisabled" : ""}`}
        title="Current market view"
      >
        <span className="key-moment-indicator" />
        <span className="key-moment-date">Now</span>
        <span className="key-moment-title">Today</span>
      </button>

      <span className="key-moment-divider" />

      {/* Historical moments */}
      {moments.map((m) => {
        const isActive = activeDate === m.date;
        return (
          <button
            key={m.id}
            onClick={() => onSelect(m.date)}
            disabled={disabled}
            className={`key-moment-card ${isActive ? "isActive" : ""} ${disabled ? "isDisabled" : ""}`}
            title={m.description}
          >
            <span className="key-moment-date">{formatDate(m.date)}</span>
            <span className="key-moment-title">{m.title}</span>
          </button>
        );
      })}
    </div>
  );
}

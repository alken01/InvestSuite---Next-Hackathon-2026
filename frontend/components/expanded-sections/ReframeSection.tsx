"use client";

import React from "react";

interface ReframeSectionProps {
  type: "reframe";
  paid: string;
  current: string;
  message: string;
}

export function ReframeSection({ paid, current, message }: ReframeSectionProps) {
  return (
    <div
      style={{
        background: "var(--surface)",
        borderRadius: "var(--r-lg)",
        padding: "var(--card-padding)",
        textAlign: "center",
      }}
    >
      <p
        className="font-display"
        style={{
          fontStyle: "italic",
          fontSize: "var(--body-size)",
          color: "var(--text-mid)",
          lineHeight: 1.5,
          margin: "0 0 var(--space-md) 0",
        }}
      >
        {message}
      </p>
      <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "var(--space-lg)" }}>
        <div>
          <div className="micro-label" style={{ marginBottom: "var(--space-xs)" }}>
            Paid
          </div>
          <div
            className="font-display"
            style={{
              fontSize: "var(--text-reframe)",
              color: "var(--text-primary)",
            }}
          >
            {paid}
          </div>
        </div>
        <div>
          <div className="micro-label" style={{ marginBottom: "var(--space-xs)" }}>
            Current
          </div>
          <div
            className="font-display"
            style={{
              fontSize: "var(--text-reframe)",
              color: "var(--text-primary)",
            }}
          >
            {current}
          </div>
        </div>
      </div>
    </div>
  );
}

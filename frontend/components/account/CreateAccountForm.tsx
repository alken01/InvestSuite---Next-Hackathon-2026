"use client";

import React, { useState } from "react";

interface CreateAccountFormProps {
  onNameSubmitted: (name: string) => void;
  disabled?: boolean;
  error?: string | null;
}

export function CreateAccountForm({ onNameSubmitted, disabled, error }: CreateAccountFormProps) {
  const [name, setName] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (name.trim().length < 2) return;
    onNameSubmitted(name.trim());
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col items-center gap-[var(--space-xl)] pointer-events-auto" style={{ padding: "var(--space-xl)", maxWidth: 320 }}>
      <div className="text-center">
        <h2 className="font-display" style={{ fontSize: "var(--text-hero-sm)", color: "var(--text-primary)", marginBottom: "var(--space-sm)" }}>
          Time-Travel Investing Simulator
        </h2>
        <p style={{ fontSize: "var(--text-small)", color: "var(--text-mid)", lineHeight: 1.5 }}>
          Create your account, buy stocks at historical prices, and watch the Brain adapt to market moments.
        </p>
      </div>

      <div className="flex flex-col gap-[var(--space-md)] w-full">
        <input
          type="text"
          placeholder="Your name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          maxLength={100}
          disabled={disabled}
          className="w-full outline-none"
          style={{
            padding: "var(--space-md) var(--space-lg)",
            borderRadius: "var(--r-full)",
            border: "1px solid var(--border)",
            background: "var(--surface)",
            color: "var(--text-primary)",
            fontSize: "var(--body-size)",
            fontFamily: "var(--font-sans)",
          }}
          autoFocus
        />
        {error && (
          <p style={{ color: "var(--red)", fontSize: "var(--text-small)", margin: 0 }}>
            {error}
          </p>
        )}
        <button
          type="submit"
          disabled={name.trim().length < 2 || disabled}
          className="w-full text-center transition-opacity hover:opacity-80 cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
          style={{
            background: "var(--green)",
            color: "var(--bg)",
            fontSize: "var(--body-size)",
            fontWeight: 600,
            borderRadius: "var(--r-md)",
            padding: "var(--space-lg) var(--space-xl)",
            border: "none",
          }}
        >
          Continue
        </button>
      </div>
    </form>
  );
}

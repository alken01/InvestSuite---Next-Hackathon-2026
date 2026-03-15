"use client";

import { ActionButton } from "@/components/ui/action-button";

interface LandingScreenProps {
  onCreateAccount: () => void;
}

export function LandingScreen({ onCreateAccount }: LandingScreenProps) {
  return (
    <div className="flex flex-col items-center gap-[var(--space-xl)] pointer-events-auto" style={{ padding: "var(--space-xl)", maxWidth: 320 }}>
      <div className="text-center">
        <h1 className="font-display" style={{ fontSize: "var(--text-hero)", color: "var(--text-primary)", lineHeight: 1.1 }}>
          InvestSuite
        </h1>
        <p style={{ fontSize: "var(--text-small)", color: "var(--text-mid)", lineHeight: 1.5, marginTop: "var(--space-sm)" }}>
          An investing experience that adapts to you
        </p>
      </div>
      <div className="flex flex-col gap-[var(--space-md)] w-full" style={{ marginTop: "var(--space-md)" }}>
        <ActionButton variant="primary" fullWidth onClick={onCreateAccount}>
          Create Account
        </ActionButton>
      </div>
    </div>
  );
}

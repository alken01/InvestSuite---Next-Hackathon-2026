"use client";

import { useState, useCallback } from "react";
import { ActionButton } from "@/components/ui/action-button";

interface OnboardingProfile {
  name: string;
  experienceLevel: string;
  riskProfile: string;
  personality: string;
  clerkUserId?: string;
}

interface OnboardingFlowProps {
  name: string;
  clerkUserId?: string;
  onComplete: (profile: OnboardingProfile) => void;
}

interface OnboardingStep {
  question: string;
  options: { label: string; value: string }[];
}

const steps: OnboardingStep[] = [
  {
    question: "How much investing experience do you have?",
    options: [
      { label: "Beginner", value: "Beginner" },
      { label: "Intermediate", value: "Intermediate" },
      { label: "Expert", value: "Expert" },
    ],
  },
  {
    question: "How do you feel about risk?",
    options: [
      { label: "Conservative", value: "Conservative" },
      { label: "Moderate", value: "Moderate" },
      { label: "Aggressive", value: "Aggressive" },
    ],
  },
  {
    question: "What best describes you?",
    options: [
      { label: "Cautious learner", value: "Cautious learner who prefers safety and education" },
      { label: "Steady grower", value: "Steady grower focused on long-term balanced returns" },
      { label: "Active trader", value: "Hands-on trader who embraces volatility and opportunity" },
    ],
  },
];

export function OnboardingFlow({ name, clerkUserId, onComplete }: OnboardingFlowProps) {
  const [stepIndex, setStepIndex] = useState(0);
  const [selected, setSelected] = useState<string | null>(null);
  const [answers, setAnswers] = useState<string[]>([]);

  const handleSelect = useCallback(
    (value: string) => {
      setSelected(value);

      const newAnswers = [...answers, value];

      setTimeout(() => {
        if (stepIndex < steps.length - 1) {
          setAnswers(newAnswers);
          setStepIndex(stepIndex + 1);
          setSelected(null);
        } else {
          onComplete({
            name,
            experienceLevel: newAnswers[0],
            riskProfile: newAnswers[1],
            personality: newAnswers[2],
            clerkUserId,
          });
        }
      }, 300);
    },
    [stepIndex, answers, name, clerkUserId, onComplete]
  );

  const step = steps[stepIndex];

  return (
    <div className="flex flex-col items-center gap-[var(--space-2xl)] pointer-events-auto" style={{ padding: "var(--space-xl)", maxWidth: 320 }}>
      <h2 className="font-display text-center" style={{ fontSize: "var(--text-hero-sm)", color: "var(--text-primary)", lineHeight: 1.3 }}>
        {step.question}
      </h2>

      <div className="flex flex-col gap-[var(--space-md)] w-full">
        {step.options.map((option) => {
          const isSelected = selected === option.value;
          return (
            <ActionButton
              key={option.value}
              variant={isSelected ? "primary" : "secondary"}
              fullWidth
              onClick={() => !selected && handleSelect(option.value)}
              disabled={selected !== null && !isSelected}
            >
              {option.label}
            </ActionButton>
          );
        })}
      </div>

      <div className="flex gap-[var(--space-sm)] justify-center">
        {steps.map((_, i) => (
          <span
            key={i}
            style={{
              width: "var(--dot-lg)",
              height: "var(--dot-lg)",
              borderRadius: "50%",
              background: i < stepIndex
                ? "var(--green)"
                : i === stepIndex
                  ? "var(--text-primary)"
                  : "var(--surface-active)",
              transition: "background 0.3s ease",
            }}
          />
        ))}
      </div>
    </div>
  );
}

import { Tone, Density } from "@/types/layout";

export interface ToneTheme {
  transitionSpeed: string;
  animationEase: string;
  heroOpacity: number;
  accentIntensity: number;
}

export const TONE_THEMES: Record<Tone, ToneTheme> = {
  [Tone.Reassuring]: { transitionSpeed: "0.8s", animationEase: "cubic-bezier(0.25, 1, 0.5, 1)", heroOpacity: 0.6, accentIntensity: 0.3 },
  [Tone.Celebratory]: { transitionSpeed: "0.5s", animationEase: "cubic-bezier(0.16, 1, 0.3, 1)", heroOpacity: 1.0, accentIntensity: 0.7 },
  [Tone.Neutral]: { transitionSpeed: "0.6s", animationEase: "cubic-bezier(0.16, 1, 0.3, 1)", heroOpacity: 1.0, accentIntensity: 0.5 },
  [Tone.Focused]: { transitionSpeed: "0.2s", animationEase: "cubic-bezier(0.2, 0, 0, 1)", heroOpacity: 1.0, accentIntensity: 0.55 },
  [Tone.Welcoming]: { transitionSpeed: "0.5s", animationEase: "cubic-bezier(0.16, 1, 0.3, 1)", heroOpacity: 1.0, accentIntensity: 0.45 },
};

export interface DensityTheme {
  widgetGap: string;
  contentPadding: string;
  cardPadding: string;
  bodySize: string;
  labelSize: string;
}

export const DENSITY_THEMES: Record<Density, DensityTheme> = {
  [Density.Sparse]: { widgetGap: "16px", contentPadding: "24px", cardPadding: "18px", bodySize: "15px", labelSize: "12px" },
  [Density.Moderate]: { widgetGap: "12px", contentPadding: "20px", cardPadding: "16px", bodySize: "14px", labelSize: "11px" },
  [Density.Dense]: { widgetGap: "8px", contentPadding: "18px", cardPadding: "14px", bodySize: "13px", labelSize: "10px" },
};

export function getToneCSSVars(tone: Tone): Record<string, string> {
  const t = TONE_THEMES[tone] ?? TONE_THEMES[Tone.Neutral];
  return {
    "--transition-speed": t.transitionSpeed,
    "--animation-ease": t.animationEase,
    "--hero-opacity": String(t.heroOpacity),
    "--accent-intensity": String(t.accentIntensity),
  };
}

export function getDensityCSSVars(density: Density): Record<string, string> {
  const d = DENSITY_THEMES[density] ?? DENSITY_THEMES[Density.Sparse];
  return {
    "--widget-gap": d.widgetGap,
    "--content-padding": d.contentPadding,
    "--card-padding": d.cardPadding,
    "--body-size": d.bodySize,
    "--label-size": d.labelSize,
  };
}

import { Sentiment } from "@/types/layout";


export interface AmbientTheme {
  gradients: string[];
  breathingSpeed: string;
  ringColor: string;
}

export const AMBIENT_THEMES: Record<Sentiment, AmbientTheme> = {
  [Sentiment.Calm]: {
    gradients: [
      "radial-gradient(ellipse 60% 50% at 30% 35%, rgba(126,219,168,0.07), transparent 70%)",
      "radial-gradient(ellipse 50% 40% at 75% 65%, rgba(138,180,248,0.05), transparent 60%)",
      "radial-gradient(ellipse 80% 60% at 50% 50%, rgba(126,219,168,0.03), transparent 80%)",
    ],
    breathingSpeed: "10s",
    ringColor: "rgba(255,255,255,0.03)",
  },
  [Sentiment.Cautious]: {
    gradients: [
      "radial-gradient(ellipse 55% 45% at 40% 30%, rgba(240,198,116,0.06), transparent 65%)",
      "radial-gradient(ellipse 50% 50% at 65% 70%, rgba(232,138,138,0.04), transparent 60%)",
      "radial-gradient(ellipse 70% 50% at 50% 50%, rgba(240,198,116,0.02), transparent 80%)",
    ],
    breathingSpeed: "8s",
    ringColor: "rgba(255,255,255,0.03)",
  },
  [Sentiment.Fear]: {
    gradients: [
      "radial-gradient(ellipse 50% 40% at 50% 40%, rgba(232,138,138,0.05), transparent 65%)",
      "radial-gradient(ellipse 40% 40% at 30% 60%, rgba(180,100,100,0.03), transparent 60%)",
    ],
    breathingSpeed: "14s",
    ringColor: "rgba(255,255,255,0.02)",
  },
  [Sentiment.Greed]: {
    gradients: [
      "radial-gradient(ellipse 55% 45% at 35% 35%, rgba(126,219,168,0.09), transparent 65%)",
      "radial-gradient(ellipse 45% 40% at 70% 60%, rgba(240,198,116,0.06), transparent 60%)",
      "radial-gradient(ellipse 60% 50% at 50% 50%, rgba(126,219,168,0.04), transparent 75%)",
    ],
    breathingSpeed: "7s",
    ringColor: "rgba(126,219,168,0.04)",
  },
  [Sentiment.Returning]: {
    gradients: [
      "radial-gradient(ellipse 60% 50% at 50% 40%, rgba(138,180,248,0.06), transparent 70%)",
      "radial-gradient(ellipse 40% 35% at 30% 65%, rgba(126,219,168,0.04), transparent 60%)",
    ],
    breathingSpeed: "11s",
    ringColor: "rgba(255,255,255,0.03)",
  },
  [Sentiment.Neutral]: {
    gradients: [
      "radial-gradient(ellipse 60% 50% at 40% 40%, rgba(126,219,168,0.04), transparent 70%)",
      "radial-gradient(ellipse 50% 40% at 65% 60%, rgba(138,180,248,0.03), transparent 60%)",
    ],
    breathingSpeed: "10s",
    ringColor: "rgba(255,255,255,0.025)",
  },
};

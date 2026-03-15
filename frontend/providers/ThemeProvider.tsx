"use client";

import { createContext, useContext, useMemo } from "react";
import { Tone, Density } from "@/types/layout";
import { getToneCSSVars, getDensityCSSVars } from "@/lib/theme";

interface ThemeContextValue {
  tone: Tone;
}

const ThemeContext = createContext<ThemeContextValue>({
  tone: Tone.Neutral,
});

export function useTheme() {
  return useContext(ThemeContext);
}

interface ThemeProviderProps {
  tone: Tone;
  children: React.ReactNode;
}

export function ThemeProvider({ tone, children }: ThemeProviderProps) {
  const style = useMemo(() => {
    return {
      ...getToneCSSVars(tone),
      ...getDensityCSSVars(Density.Sparse),
    } as React.CSSProperties;
  }, [tone]);

  return (
    <ThemeContext.Provider value={{ tone }}>
      <div style={{ ...style, width: "100%", height: "100%" }}>{children}</div>
    </ThemeContext.Provider>
  );
}

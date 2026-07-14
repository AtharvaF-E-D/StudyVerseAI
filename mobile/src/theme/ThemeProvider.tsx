import React, { createContext, useContext, useMemo } from "react";
import { useColorScheme as useNativeWindColorScheme } from "nativewind";

import { colorsFor, radii, spacing, typography, type ColorScheme, type ColorTokens } from "./tokens";

export interface ThemeContextValue {
  scheme: ColorScheme;
  colors: ColorTokens;
  spacing: typeof spacing;
  radii: typeof radii;
  typography: typeof typography;
  setScheme: (scheme: ColorScheme | "system") => void;
  toggleScheme: () => void;
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined);

/**
 * Reads the active color scheme from NativeWind (which itself tracks the
 * system appearance unless overridden via `setScheme`) and exposes the
 * resolved design tokens through context. NativeWind's `darkMode: 'class'`
 * strategy means className-based dark: variants in components are already
 * driven by this same underlying colorScheme state; this provider exists so
 * screens/components that need raw token values (e.g. for native driver
 * colors, SVG icons, or the RN Animated / Reanimated APIs) can read them
 * without duplicating the palette.
 */
export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const { colorScheme, setColorScheme, toggleColorScheme } = useNativeWindColorScheme();
  const scheme: ColorScheme = colorScheme === "dark" ? "dark" : "light";

  const value = useMemo<ThemeContextValue>(
    () => ({
      scheme,
      colors: colorsFor(scheme),
      spacing,
      radii,
      typography,
      setScheme: setColorScheme,
      toggleScheme: toggleColorScheme,
    }),
    [scheme, setColorScheme, toggleColorScheme],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const ctx = useContext(ThemeContext);
  if (!ctx) {
    throw new Error("useTheme must be used within a ThemeProvider");
  }
  return ctx;
}

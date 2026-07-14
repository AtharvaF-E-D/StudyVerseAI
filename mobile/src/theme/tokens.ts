/**
 * Design tokens shared between the NativeWind (Tailwind) config and any
 * place that needs raw values in JS (e.g. native driver colors, icons,
 * chart libraries) rather than className strings. Keep these in sync with
 * `tailwind.config.js` — this file is the source of truth for the palette
 * and tailwind.config.js's `theme.extend.colors` should mirror it.
 */

export type ColorScheme = "light" | "dark";

export interface ColorTokens {
  background: string;
  surface: string;
  border: string;
  textPrimary: string;
  textSecondary: string;
  brand: string;
  brandLight: string;
  brandDark: string;
  accent: string;
  success: string;
  warning: string;
  danger: string;
  overlay: string;
}

export const lightColors: ColorTokens = {
  background: "#FFFFFF",
  surface: "#F5F6FA",
  border: "#E4E7EC",
  textPrimary: "#12141A",
  textSecondary: "#5B6272",
  brand: "#5B5BF7",
  brandLight: "#8E8CFB",
  brandDark: "#4139D9",
  accent: "#00C2A8",
  success: "#1E9E5A",
  warning: "#D98C0C",
  danger: "#E5484D",
  overlay: "rgba(18, 20, 26, 0.5)",
};

export const darkColors: ColorTokens = {
  background: "#0B0E14",
  surface: "#151923",
  border: "#262B38",
  textPrimary: "#F3F4F6",
  textSecondary: "#9AA1B1",
  brand: "#8E8CFB",
  brandLight: "#ADABFC",
  brandDark: "#5B5BF7",
  accent: "#2FE0C4",
  success: "#3BC97C",
  warning: "#E8A63A",
  danger: "#F0696D",
  overlay: "rgba(0, 0, 0, 0.6)",
};

export function colorsFor(scheme: ColorScheme): ColorTokens {
  return scheme === "dark" ? darkColors : lightColors;
}

/** 4px base spacing scale. */
export const spacing = {
  none: 0,
  xs: 4,
  sm: 8,
  md: 12,
  lg: 16,
  xl: 20,
  "2xl": 24,
  "3xl": 28,
  "4xl": 32,
  "5xl": 36,
  "6xl": 40,
  "7xl": 48,
  "8xl": 64,
} as const;

export const radii = {
  sm: 6,
  md: 10,
  lg: 16,
  xl: 24,
  full: 9999,
} as const;

export interface TypeStyle {
  fontSize: number;
  lineHeight: number;
  fontWeight: "400" | "500" | "600" | "700";
}

export const typography: {
  display: TypeStyle;
  heading: TypeStyle;
  subheading: TypeStyle;
  body: TypeStyle;
  bodyMedium: TypeStyle;
  caption: TypeStyle;
} = {
  display: { fontSize: 34, lineHeight: 40, fontWeight: "700" },
  heading: { fontSize: 24, lineHeight: 30, fontWeight: "700" },
  subheading: { fontSize: 18, lineHeight: 24, fontWeight: "600" },
  body: { fontSize: 16, lineHeight: 22, fontWeight: "400" },
  bodyMedium: { fontSize: 16, lineHeight: 22, fontWeight: "500" },
  caption: { fontSize: 13, lineHeight: 18, fontWeight: "400" },
};

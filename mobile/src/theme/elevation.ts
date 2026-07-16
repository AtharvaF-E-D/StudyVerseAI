/**
 * Elevation/shadow scale shared across `src/components`. NativeWind's
 * `shadow-*` / `boxShadow` utilities compile to CSS `box-shadow` on web but
 * have no reliable native translation, so anything that needs a shadow on
 * iOS/Android must use RN's `shadowColor`/`shadowOffset`/`shadowOpacity`/
 * `shadowRadius` props plus Android's separate `elevation` prop. This file
 * is the single source of truth for that native style object so components
 * don't hand-roll shadow values (and so light/dark shadow color stays
 * consistent with the rest of the palette).
 */
import { Platform, type ViewStyle } from "react-native";

import { colorsFor, type ColorScheme } from "./tokens";
import { useTheme } from "./ThemeProvider";

export type ElevationLevel = "none" | "sm" | "md" | "lg";

const ANDROID_ELEVATION: Record<ElevationLevel, number> = {
  none: 0,
  sm: 2,
  md: 4,
  lg: 10,
};

const IOS_SHADOW: Record<ElevationLevel, { offsetY: number; opacity: number; radius: number }> = {
  none: { offsetY: 0, opacity: 0, radius: 0 },
  sm: { offsetY: 1, opacity: 0.08, radius: 2 },
  md: { offsetY: 4, opacity: 0.12, radius: 8 },
  lg: { offsetY: 10, opacity: 0.18, radius: 20 },
};

/**
 * Returns a platform-correct shadow style object for the given elevation
 * level and color scheme. Shadows are darker/more opaque against light
 * surfaces and slightly stronger against dark ones (where a subtle glow
 * reads better than a soft drop shadow) — same shape RN expects everywhere,
 * so it can be spread directly into a `style` prop alongside a className.
 */
export function elevationStyle(level: ElevationLevel, scheme: ColorScheme = "light"): ViewStyle {
  if (level === "none") {
    return { shadowOpacity: 0, elevation: 0 };
  }

  const { offsetY, opacity, radius } = IOS_SHADOW[level];
  const shadowColor = scheme === "dark" ? "#000000" : colorsFor(scheme).textPrimary;

  if (Platform.OS === "android") {
    return { elevation: ANDROID_ELEVATION[level] };
  }

  return {
    shadowColor,
    shadowOffset: { width: 0, height: offsetY },
    shadowOpacity: opacity,
    shadowRadius: radius,
    // Harmless on iOS/web; keeps a single style object usable everywhere.
    elevation: ANDROID_ELEVATION[level],
  };
}

/**
 * Component-facing hook: reads the active color scheme from `useTheme()`
 * so call sites just write `useElevation("md")` instead of threading the
 * scheme through manually.
 */
export function useElevation(level: ElevationLevel): ViewStyle {
  const { scheme } = useTheme();
  return elevationStyle(level, scheme);
}

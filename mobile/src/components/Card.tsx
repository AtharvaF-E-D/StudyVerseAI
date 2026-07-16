import React from "react";
import { View, type ViewProps } from "react-native";
import { BlurView } from "expo-blur";
import { cssInterop } from "nativewind";

import { useTheme } from "../theme/ThemeProvider";
import { useElevation } from "../theme/elevation";

// `BlurView` is a third-party native component (not one of NativeWind's
// auto-registered `react-native` primitives), so — same rule as
// `Button.tsx`'s animated pressable — it needs an explicit `cssInterop`
// call or a `className` passed to it renders with no styles at all.
cssInterop(BlurView, { className: "style" });

export type CardElevation = "flat" | "raised" | "glass";

export interface CardProps extends ViewProps {
  elevation?: CardElevation;
  className?: string;
}

/**
 * Surface container used throughout the app for grouped content (dashboard
 * tiles, list sections, quiz/flashcard cards).
 *
 * - `flat` — bordered surface, no shadow. Default choice for content that
 *   sits directly on the screen background (most cards).
 * - `raised` — shadow only, no border. Use sparingly to draw extra
 *   attention to one card among flat siblings.
 * - `glass` — a `BlurView` glassmorphism surface. Reserved ONLY for
 *   overlay/floating surfaces (modals, floating action cards, anything
 *   presented above other content) — never everyday content cards. Text
 *   over a busy blurred background reads worse than a solid surface, and
 *   BlurView is comparatively expensive to render, so it earns its keep
 *   only where the "floating above content" effect is the point.
 */
export function Card({ elevation = "flat", className = "", style, children, ...viewProps }: CardProps) {
  const { scheme } = useTheme();
  const shadow = useElevation(elevation === "raised" ? "md" : "none");

  if (elevation === "glass") {
    return (
      <BlurView
        intensity={40}
        tint={scheme === "dark" ? "dark" : "light"}
        className={[
          "overflow-hidden rounded-xl border border-white/30 p-4 dark:border-white/10",
          className,
        ].join(" ")}
        style={style}
        {...viewProps}
      >
        {children}
      </BlurView>
    );
  }

  return (
    <View
      className={[
        "rounded-xl p-4",
        elevation === "flat"
          ? "border border-border bg-surface dark:border-border-dark dark:bg-surface-dark"
          : "bg-surface dark:bg-surface-dark",
        className,
      ].join(" ")}
      style={[shadow, style]}
      {...viewProps}
    >
      {children}
    </View>
  );
}

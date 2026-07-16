import React, { useEffect } from "react";
import { View, type ViewProps } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { durations, easings } from "../theme/motion";

// Same rule as `Button.tsx`: NativeWind auto-registers `className` support
// only for plain `react-native` components. `Animated.View` is a distinct
// exotic component created internally by react-native-reanimated, so it
// needs its own explicit `cssInterop` call or its Tailwind classes are
// silently dropped.
cssInterop(Animated.View, { className: "style" });

export interface ProgressBarProps extends Omit<ViewProps, "children"> {
  /** 0-1. Values outside that range are clamped. */
  value: number;
  className?: string;
  fillClassName?: string;
  accessibilityLabel?: string;
}

/**
 * Determinate horizontal progress bar (e.g. quiz progress, upload state).
 * Animates the fill's width with Reanimated whenever `value` changes.
 */
export function ProgressBar({
  value,
  className = "",
  fillClassName = "",
  accessibilityLabel,
  ...viewProps
}: ProgressBarProps) {
  const clamped = Math.min(1, Math.max(0, value));
  const progress = useSharedValue(clamped);

  useEffect(() => {
    progress.value = withTiming(clamped, { duration: durations.base, easing: easings.standard });
  }, [clamped, progress]);

  const fillStyle = useAnimatedStyle(() => ({
    width: `${progress.value * 100}%`,
  }));

  return (
    <View
      accessibilityRole="progressbar"
      accessibilityLabel={accessibilityLabel ?? "Progress"}
      accessibilityValue={{ min: 0, max: 100, now: Math.round(clamped * 100) }}
      className={["h-2 w-full overflow-hidden rounded-full bg-surface dark:bg-surface-dark", className].join(" ")}
      {...viewProps}
    >
      <Animated.View
        className={["h-full rounded-full bg-brand dark:bg-brand-light", fillClassName].join(" ")}
        style={fillStyle}
      />
    </View>
  );
}

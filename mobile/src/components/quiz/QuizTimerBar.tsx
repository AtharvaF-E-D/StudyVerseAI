import React, { useEffect } from "react";
import { View } from "react-native";
import Animated, { Easing, useAnimatedStyle, useSharedValue, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

cssInterop(Animated.View, { className: "style" });

export interface QuizTimerBarProps {
  remainingMs: number;
  totalMs: number;
  className?: string;
}

/**
 * Shrinking countdown bar for the current question. Deliberately uses linear
 * easing — `theme/motion.ts` otherwise bans `Easing.linear` for UI motion
 * ("reads as mechanical"), but a countdown IS a literal clock: tracking wall
 * time at a constant rate is the correct read here, not an oversight.
 * Re-targets a short `withTiming` every time `remainingMs` ticks (the play
 * screen decrements it roughly every 100ms) so the fill glides smoothly
 * instead of snapping between discrete steps, while still staying an honest
 * reflection of the client-side countdown driving it.
 */
export function QuizTimerBar({ remainingMs, totalMs, className = "" }: QuizTimerBarProps) {
  const fraction = totalMs > 0 ? Math.min(1, Math.max(0, remainingMs / totalMs)) : 0;
  const progress = useSharedValue(fraction);

  useEffect(() => {
    progress.value = withTiming(fraction, { duration: 120, easing: Easing.linear });
  }, [fraction, progress]);

  const fillStyle = useAnimatedStyle(() => ({ width: `${progress.value * 100}%` }));

  return (
    <View
      accessibilityRole="progressbar"
      accessibilityLabel="Time remaining for this question"
      accessibilityValue={{ min: 0, max: 100, now: Math.round(fraction * 100) }}
      className={["h-2.5 w-full overflow-hidden rounded-full bg-surface dark:bg-surface-dark", className].join(" ")}
    >
      <Animated.View
        style={fillStyle}
        className={["h-full rounded-full", fraction <= 0.25 ? "bg-danger" : "bg-brand dark:bg-brand-light"].join(" ")}
      />
    </View>
  );
}

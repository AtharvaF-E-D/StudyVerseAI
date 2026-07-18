import React, { useEffect } from "react";
import { Text, View } from "react-native";
import Animated, { Easing, useAnimatedStyle, useSharedValue, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";

// Same rule as `QuizTimerBar.tsx`: `Animated.View` needs its own explicit
// `cssInterop` call before a `className` passed to it has any effect.
cssInterop(Animated.View, { className: "style" });

export interface MockTestTimerBannerProps {
  remainingMs: number;
  totalMs: number;
  className?: string;
}

function formatCountdown(ms: number): string {
  const totalSeconds = Math.max(0, Math.round(ms / 1000));
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = totalSeconds % 60;
  return `${minutes}:${seconds.toString().padStart(2, "0")}`;
}

/**
 * A SINGLE overall countdown for the whole exam — unlike Rapid Fire Quiz's
 * per-question `QuizTimerBar` (reset every question), this ticks down once
 * across every question in the attempt and is rendered exactly once, pinned
 * near the top of the exam screen so it stays prominent regardless of which
 * question is currently showing. Reddens (text + fill) once a quarter or
 * less of the total time remains, mirroring `QuizTimerBar`'s own low-time
 * treatment.
 */
export function MockTestTimerBanner({ remainingMs, totalMs, className = "" }: MockTestTimerBannerProps) {
  const { colors } = useTheme();
  const fraction = totalMs > 0 ? Math.min(1, Math.max(0, remainingMs / totalMs)) : 0;
  const isLow = fraction <= 0.25;
  const progress = useSharedValue(fraction);

  useEffect(() => {
    progress.value = withTiming(fraction, { duration: 400, easing: Easing.linear });
  }, [fraction, progress]);

  const fillStyle = useAnimatedStyle(() => ({ width: `${progress.value * 100}%` }));

  return (
    <View className={className}>
      <View className="mb-2 flex-row items-center justify-center">
        <Icon name="time-outline" size={20} color={isLow ? colors.danger : colors.textPrimary} />
        <Text
          className={[
            "ml-2 text-heading",
            isLow ? "text-danger" : "text-ink-primary dark:text-ink-primary-dark",
          ].join(" ")}
        >
          {formatCountdown(remainingMs)}
        </Text>
        <Text className="ml-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">remaining</Text>
      </View>
      <View
        accessibilityRole="progressbar"
        accessibilityLabel="Time remaining for this test"
        accessibilityValue={{ min: 0, max: 100, now: Math.round(fraction * 100) }}
        className="h-2.5 w-full overflow-hidden rounded-full bg-surface dark:bg-surface-dark"
      >
        <Animated.View
          style={fillStyle}
          className={["h-full rounded-full", isLow ? "bg-danger" : "bg-brand dark:bg-brand-light"].join(" ")}
        />
      </View>
    </View>
  );
}

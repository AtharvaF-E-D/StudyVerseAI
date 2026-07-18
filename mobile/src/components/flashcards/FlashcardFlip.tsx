import React, { useEffect } from "react";
import { Pressable, StyleSheet, Text } from "react-native";
import Animated, { interpolate, useAnimatedStyle, useSharedValue, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { useReduceMotion } from "../../theme/motion";

// Same rule as `Button.tsx`/`Skeleton.tsx`: `Animated.View` needs its own
// explicit `cssInterop` call before a `className` passed to it has any effect.
cssInterop(Animated.View, { className: "style" });

const FLIP_DURATION_MS = 450;

export interface FlashcardFlipProps {
  frontText: string;
  backText: string;
  /** Controlled from the parent so it can drive both the animation and which rating buttons show. */
  flipped: boolean;
  onPress: () => void;
  className?: string;
}

/**
 * One flashcard's front/back pair with a Reanimated 3D flip (rotateY)
 * between them. Both faces are always mounted, stacked on top of each
 * other (the back absolutely positioned over the front, which drives the
 * container's height), each with `backfaceVisibility: "hidden"` so only the
 * currently-facing side is visible — paired with a plain opacity crossfade
 * at the halfway point so the handoff still reads clearly even where
 * `backfaceVisibility` support is inconsistent (this was verified visually
 * on Expo web; no fallback to a flat cross-fade proved necessary there).
 * Respects reduce-motion by snapping instantly instead of animating.
 */
export function FlashcardFlip({ frontText, backText, flipped, onPress, className = "" }: FlashcardFlipProps) {
  const reduceMotion = useReduceMotion();
  const rotation = useSharedValue(flipped ? 1 : 0);

  useEffect(() => {
    const target = flipped ? 1 : 0;
    rotation.value = reduceMotion ? target : withTiming(target, { duration: FLIP_DURATION_MS });
  }, [flipped, reduceMotion, rotation]);

  const frontStyle = useAnimatedStyle(() => ({
    transform: [{ perspective: 1200 }, { rotateY: `${interpolate(rotation.value, [0, 1], [0, 180])}deg` }],
    opacity: rotation.value < 0.5 ? 1 : 0,
  }));

  const backStyle = useAnimatedStyle(() => ({
    transform: [{ perspective: 1200 }, { rotateY: `${interpolate(rotation.value, [0, 1], [180, 360])}deg` }],
    opacity: rotation.value >= 0.5 ? 1 : 0,
  }));

  return (
    <Pressable
      onPress={onPress}
      accessibilityRole="button"
      accessibilityLabel={flipped ? "Card back. Tap to see the front again." : "Card front. Tap to reveal the answer."}
      className={["relative", className].join(" ")}
    >
      <Animated.View
        style={[{ backfaceVisibility: "hidden" }, frontStyle]}
        className="min-h-[220px] items-center justify-center rounded-2xl border border-border bg-surface p-6 dark:border-border-dark dark:bg-surface-dark"
      >
        <Text className="text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
          {frontText}
        </Text>
      </Animated.View>
      <Animated.View
        style={[StyleSheet.absoluteFill, { backfaceVisibility: "hidden" }, backStyle]}
        className="min-h-[220px] items-center justify-center rounded-2xl border border-brand bg-brand/5 p-6 dark:border-brand-light dark:bg-brand-light/10"
      >
        <Text className="text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
          {backText}
        </Text>
      </Animated.View>
    </Pressable>
  );
}

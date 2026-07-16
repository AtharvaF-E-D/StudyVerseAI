import React, { useEffect } from "react";
import type { DimensionValue } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withRepeat, withSequence, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { useReduceMotion } from "../theme/motion";

// Same rule as `Button.tsx`: `Animated.View` needs its own explicit
// `cssInterop` call before a `className` passed to it will have any effect.
cssInterop(Animated.View, { className: "style" });

export type SkeletonVariant = "text" | "circle" | "rect";

export interface SkeletonProps {
  variant?: SkeletonVariant;
  width?: DimensionValue;
  height?: DimensionValue;
  className?: string;
}

const variantClasses: Record<SkeletonVariant, string> = {
  text: "h-4 rounded",
  circle: "rounded-full",
  rect: "rounded-md",
};

/**
 * Pulsing-opacity shimmer placeholder shown while content loads. Kept to a
 * simple opacity loop (no gradient sweep) since no gradient library is a
 * dependency yet — it reads as clearly "loading" with far less complexity.
 * Respects reduce-motion by holding a static mid-opacity instead of looping.
 */
export function Skeleton({ variant = "text", width, height, className = "" }: SkeletonProps) {
  const reduceMotion = useReduceMotion();
  const opacity = useSharedValue(0.4);

  useEffect(() => {
    if (reduceMotion) {
      opacity.value = 0.5;
      return;
    }
    opacity.value = withRepeat(
      withSequence(withTiming(0.9, { duration: 600 }), withTiming(0.4, { duration: 600 })),
      -1,
      true,
    );
  }, [reduceMotion, opacity]);

  const animatedStyle = useAnimatedStyle(() => ({ opacity: opacity.value }));

  const sizeStyle: { width?: DimensionValue; height?: DimensionValue } = {};
  if (width !== undefined) sizeStyle.width = width;
  else if (variant === "circle") sizeStyle.width = 40;
  if (height !== undefined) sizeStyle.height = height;
  else if (variant === "circle") sizeStyle.height = 40;

  return (
    <Animated.View
      accessibilityElementsHidden
      importantForAccessibility="no-hide-descendants"
      className={["bg-border dark:bg-border-dark", variantClasses[variant], className].join(" ")}
      style={[sizeStyle, animatedStyle]}
    />
  );
}

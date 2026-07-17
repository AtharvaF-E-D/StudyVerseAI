import React, { useEffect, useRef } from "react";
import { View } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withSequence, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { useReduceMotion } from "../../theme/motion";

// Same rule as `Button.tsx`/`ProgressBar.tsx`: `Animated.View` needs its own
// explicit `cssInterop` call before a `className` passed to it has any effect.
cssInterop(Animated.View, { className: "style" });

export interface LivesRowProps {
  lives: number;
  maxLives?: number;
  className?: string;
}

function LifeHeart({ active }: { active: boolean }) {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();
  const scale = useSharedValue(1);
  const wasActive = useRef(active);

  useEffect(() => {
    if (wasActive.current && !active && !reduceMotion) {
      scale.value = withSequence(withTiming(1.35, { duration: 100 }), withTiming(1, { duration: 220 }));
    }
    wasActive.current = active;
  }, [active, reduceMotion, scale]);

  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  return (
    <Animated.View style={animatedStyle}>
      <Icon name={active ? "heart" : "heart-outline"} size={22} color={active ? colors.danger : colors.textSecondary} />
    </Animated.View>
  );
}

/**
 * Row of heart icons representing remaining lives. The heart just lost gets
 * a quick "pop" scale animation rather than silently disappearing, so losing
 * a life reads as an event, not a layout glitch.
 */
export function LivesRow({ lives, maxLives = 3, className = "" }: LivesRowProps) {
  return (
    <View
      accessibilityLabel={`${lives} of ${maxLives} lives remaining`}
      className={["flex-row items-center gap-1.5", className].join(" ")}
    >
      {Array.from({ length: maxLives }, (_, i) => (
        <LifeHeart key={i} active={i < lives} />
      ))}
    </View>
  );
}

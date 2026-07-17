import React, { useEffect } from "react";
import { Text } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withSequence, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { useReduceMotion } from "../../theme/motion";

cssInterop(Animated.View, { className: "style" });

export interface ComboBadgeProps {
  combo: number;
  className?: string;
}

/**
 * Small streak pill that pulses each time `combo` increases. Renders nothing
 * below a 2-streak so it doesn't clutter the play screen's header on the
 * first correct answer of a run.
 */
export function ComboBadge({ combo, className = "" }: ComboBadgeProps) {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();
  const scale = useSharedValue(1);

  useEffect(() => {
    if (combo >= 2 && !reduceMotion) {
      scale.value = withSequence(withTiming(1.25, { duration: 120 }), withTiming(1, { duration: 180 }));
    }
  }, [combo, reduceMotion, scale]);

  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  if (combo < 2) return null;

  return (
    <Animated.View
      style={animatedStyle}
      accessibilityLabel={`${combo} answer combo streak`}
      className={["flex-row items-center self-start rounded-full bg-warning/15 px-2.5 py-1", className].join(" ")}
    >
      <Icon name="flame" size={14} color={colors.warning} />
      <Text className="ml-1 text-caption font-semibold text-warning">{combo}x combo</Text>
    </Animated.View>
  );
}

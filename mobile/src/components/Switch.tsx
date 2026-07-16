import React, { useEffect } from "react";
import { Switch as RNSwitch, type SwitchProps as RNSwitchProps } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withSequence, withTiming } from "react-native-reanimated";

import { useTheme } from "../theme/ThemeProvider";
import { durations } from "../theme/motion";

export interface SwitchProps
  extends Omit<RNSwitchProps, "trackColor" | "thumbColor" | "ios_backgroundColor"> {
  accessibilityLabel: string;
}

/**
 * Themed wrapper around RN's built-in `Switch`, tinted with the brand
 * color track. RN's `Switch` renders its thumb as a native platform widget
 * with no accessible JS element to independently animate, so a true
 * "thumb-scale on press" isn't feasible without reimplementing the whole
 * control from scratch — per the "don't over-engineer" guidance, this
 * instead gives the whole switch a small Reanimated "pop" whenever `value`
 * changes, which still gives toggling some tactile motion.
 */
export function Switch({ value, accessibilityLabel, ...switchProps }: SwitchProps) {
  const { colors, scheme } = useTheme();
  const scale = useSharedValue(1);

  useEffect(() => {
    scale.value = withSequence(
      withTiming(1.12, { duration: durations.fast / 2 }),
      withTiming(1, { duration: durations.fast / 2 }),
    );
  }, [value, scale]);

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scale.value }],
  }));

  const trackOffColor = scheme === "dark" ? "#262B38" : "#E4E7EC";

  return (
    <Animated.View style={animatedStyle}>
      <RNSwitch
        value={value}
        accessibilityRole="switch"
        accessibilityLabel={accessibilityLabel}
        accessibilityState={{ checked: value }}
        trackColor={{ false: trackOffColor, true: colors.brand }}
        thumbColor="#FFFFFF"
        ios_backgroundColor={trackOffColor}
        {...switchProps}
      />
    </Animated.View>
  );
}

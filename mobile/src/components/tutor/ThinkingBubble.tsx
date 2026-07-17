import React, { useEffect } from "react";
import { View } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withDelay, withRepeat, withSequence, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { useTheme } from "../../theme/ThemeProvider";
import { useReduceMotion } from "../../theme/motion";

// Same rule as `Skeleton.tsx`/`Button.tsx`: `Animated.View` needs its own
// explicit `cssInterop` call before a `className` passed to it has any effect.
cssInterop(Animated.View, { className: "style" });

function Dot({ delay, color, reduceMotion }: { delay: number; color: string; reduceMotion: boolean }) {
  const translateY = useSharedValue(0);

  useEffect(() => {
    if (reduceMotion) {
      translateY.value = 0;
      return;
    }
    translateY.value = withDelay(
      delay,
      withRepeat(withSequence(withTiming(-4, { duration: 300 }), withTiming(0, { duration: 300 })), -1, false),
    );
  }, [reduceMotion, delay, translateY]);

  const animatedStyle = useAnimatedStyle(() => ({ transform: [{ translateY: translateY.value }] }));

  return <Animated.View className="mx-0.5 h-2 w-2 rounded-full" style={[{ backgroundColor: color }, animatedStyle]} />;
}

/**
 * A small stack of bouncing dots shown in place of the assistant's next
 * reply while `useSendMessageMutation` is pending — the tutor's real reply
 * is a normal (non-streaming) request that can take several seconds since
 * it waits on an actual OpenAI call, so this is what keeps the chat screen
 * from looking frozen in the meantime.
 */
export function ThinkingBubble() {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();

  return (
    <View
      accessibilityLabel="AI tutor is thinking"
      className="mb-3 max-w-[60%] self-start rounded-2xl rounded-bl-sm border border-border bg-surface px-4 py-3.5 dark:border-border-dark dark:bg-surface-dark"
    >
      <View className="flex-row items-center">
        <Dot delay={0} color={colors.textSecondary} reduceMotion={reduceMotion} />
        <Dot delay={150} color={colors.textSecondary} reduceMotion={reduceMotion} />
        <Dot delay={300} color={colors.textSecondary} reduceMotion={reduceMotion} />
      </View>
    </View>
  );
}

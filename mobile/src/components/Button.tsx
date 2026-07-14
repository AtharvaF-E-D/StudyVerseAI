import React from "react";
import { ActivityIndicator, Pressable, Text, type PressableProps } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

const AnimatedPressable = Animated.createAnimatedComponent(Pressable);

// NativeWind can't infer how to translate `className` into `style` for a
// component wrapped by `Animated.createAnimatedComponent` unless it's told
// to explicitly — without this the button renders with no Tailwind styles
// applied at all (invisible background, no padding/radius).
cssInterop(AnimatedPressable, { className: "style" });

export type ButtonVariant = "primary" | "secondary" | "ghost" | "danger";

export interface ButtonProps extends Omit<PressableProps, "children"> {
  title: string;
  variant?: ButtonVariant;
  loading?: boolean;
  disabled?: boolean;
  fullWidth?: boolean;
}

const containerVariantClasses: Record<ButtonVariant, string> = {
  primary: "bg-brand active:bg-brand-dark",
  secondary:
    "bg-surface dark:bg-surface-dark border border-border dark:border-border-dark active:opacity-80",
  ghost: "bg-transparent active:opacity-70",
  danger: "bg-danger active:opacity-90",
};

const labelVariantClasses: Record<ButtonVariant, string> = {
  primary: "text-white",
  secondary: "text-ink-primary dark:text-ink-primary-dark",
  ghost: "text-brand dark:text-brand-light",
  danger: "text-white",
};

/**
 * Shared button primitive used across the auth flow. Applies a small
 * Reanimated press-scale for tactile feedback and disables interaction
 * (and dims) while `loading` or `disabled` is true.
 */
export function Button({
  title,
  variant = "primary",
  loading = false,
  disabled = false,
  fullWidth = true,
  onPressIn,
  onPressOut,
  style,
  ...pressableProps
}: ButtonProps) {
  const scale = useSharedValue(1);
  const isInteractive = !loading && !disabled;

  const animatedStyle = useAnimatedStyle(() => ({
    transform: [{ scale: scale.value }],
  }));

  // Reanimated's SharedValue is intentionally mutated via `.value` — that's
  // the library's documented API, not a React-managed immutable value, so
  // the two mutations below are exempted from react-hooks/immutability.
  function handlePressIn(e: Parameters<NonNullable<PressableProps["onPressIn"]>>[0]) {
    // eslint-disable-next-line react-hooks/immutability
    scale.value = withTiming(0.97, { duration: 80 });
    onPressIn?.(e);
  }

  function handlePressOut(e: Parameters<NonNullable<PressableProps["onPressOut"]>>[0]) {
    // eslint-disable-next-line react-hooks/immutability
    scale.value = withTiming(1, { duration: 120 });
    onPressOut?.(e);
  }

  return (
    <AnimatedPressable
      accessibilityRole="button"
      accessibilityState={{ disabled: !isInteractive, busy: loading }}
      disabled={!isInteractive}
      onPressIn={handlePressIn}
      onPressOut={handlePressOut}
      className={[
        "flex-row items-center justify-center rounded-lg px-5 py-3.5",
        fullWidth ? "w-full" : "",
        containerVariantClasses[variant],
        !isInteractive ? "opacity-50" : "",
      ].join(" ")}
      style={[animatedStyle, style]}
      {...pressableProps}
    >
      {loading ? (
        <ActivityIndicator color={variant === "primary" || variant === "danger" ? "#FFFFFF" : "#5B5BF7"} />
      ) : (
        <Text className={["text-body font-semibold", labelVariantClasses[variant]].join(" ")}>
          {title}
        </Text>
      )}
    </AnimatedPressable>
  );
}

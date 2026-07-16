import React from "react";
import { Pressable, Text } from "react-native";
import Animated from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Icon, type IconName } from "./Icon";
import { fadeInUp, fadeOutDown } from "../theme/motion";

// Same rule as `Button.tsx`: `Animated.View` needs its own explicit
// `cssInterop` call before a `className` passed to it will have any effect.
cssInterop(Animated.View, { className: "style" });

export type ToastVariant = "neutral" | "brand" | "success" | "warning" | "danger";

export interface ToastProps {
  message: string;
  variant?: ToastVariant;
  onDismiss: () => void;
}

const containerVariantClasses: Record<ToastVariant, string> = {
  neutral: "bg-ink-primary dark:bg-surface-dark",
  brand: "bg-brand",
  success: "bg-success",
  warning: "bg-warning",
  danger: "bg-danger",
};

const iconByVariant: Record<ToastVariant, IconName> = {
  neutral: "information-circle",
  brand: "sparkles",
  success: "checkmark-circle",
  warning: "warning",
  danger: "alert-circle",
};

/**
 * Transient notification rendered by the `useToast()` provider (see
 * `src/lib/toast.tsx`). Presentational only — the provider owns the
 * show/auto-dismiss timer/state and mounts/unmounts this component, which
 * is what drives its `entering`/`exiting` animations.
 */
export function Toast({ message, variant = "neutral", onDismiss }: ToastProps) {
  return (
    <Animated.View
      entering={fadeInUp()}
      exiting={fadeOutDown()}
      accessibilityRole="alert"
      accessibilityLiveRegion="polite"
      className={[
        "max-w-[320px] flex-row items-center gap-2 rounded-full px-4 py-3 shadow-lg",
        containerVariantClasses[variant],
      ].join(" ")}
    >
      <Icon name={iconByVariant[variant]} size={18} color="#FFFFFF" />
      <Text className="flex-1 text-caption font-medium text-white" numberOfLines={2}>
        {message}
      </Text>
      <Pressable onPress={onDismiss} hitSlop={8} accessibilityRole="button" accessibilityLabel="Dismiss">
        <Icon name="close" size={16} color="#FFFFFF" />
      </Pressable>
    </Animated.View>
  );
}

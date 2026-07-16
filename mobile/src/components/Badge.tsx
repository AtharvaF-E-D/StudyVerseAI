import React from "react";
import { Text, View } from "react-native";

export type BadgeVariant = "neutral" | "brand" | "success" | "warning" | "danger";

export interface BadgeProps {
  label: string;
  variant?: BadgeVariant;
  className?: string;
}

const containerVariantClasses: Record<BadgeVariant, string> = {
  neutral: "bg-surface dark:bg-surface-dark border border-border dark:border-border-dark",
  brand: "bg-brand/15 dark:bg-brand-light/20",
  success: "bg-success/15",
  warning: "bg-warning/15",
  danger: "bg-danger/15",
};

const labelVariantClasses: Record<BadgeVariant, string> = {
  neutral: "text-ink-secondary dark:text-ink-secondary-dark",
  brand: "text-brand dark:text-brand-light",
  success: "text-success",
  warning: "text-warning",
  danger: "text-danger",
};

/** Small pill label used for statuses/tags (e.g. quiz difficulty, plan tier). */
export function Badge({ label, variant = "neutral", className = "" }: BadgeProps) {
  return (
    <View
      accessibilityRole="text"
      className={["self-start rounded-full px-2.5 py-1", containerVariantClasses[variant], className].join(" ")}
    >
      <Text className={["text-caption font-medium", labelVariantClasses[variant]].join(" ")}>{label}</Text>
    </View>
  );
}

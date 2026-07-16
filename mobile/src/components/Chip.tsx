import React from "react";
import { Pressable, Text, View, type PressableProps } from "react-native";

import { Icon } from "./Icon";
import { useTheme } from "../theme/ThemeProvider";

export interface ChipProps extends Omit<PressableProps, "children" | "onPress"> {
  label: string;
  selected?: boolean;
  onPress?: () => void;
  /** Renders a trailing "x" that removes the chip; omit to make it non-dismissible. */
  onDismiss?: () => void;
  className?: string;
}

/**
 * Selectable/dismissible chip used for filterable tags (quiz categories,
 * flashcard deck labels, etc). The label itself toggles `onPress`; the
 * dismiss "x" only renders when `onDismiss` is supplied and is a separate
 * pressable so removing a chip doesn't also toggle its selection.
 */
export function Chip({
  label,
  selected = false,
  onPress,
  onDismiss,
  className = "",
  ...pressableProps
}: ChipProps) {
  const { colors } = useTheme();

  return (
    <View
      className={[
        "flex-row items-center self-start rounded-full border py-2 pl-3.5",
        onDismiss ? "pr-2.5" : "pr-3.5",
        selected
          ? "border-brand bg-brand/15 dark:border-brand-light dark:bg-brand-light/20"
          : "border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
        className,
      ].join(" ")}
    >
      <Pressable
        onPress={onPress}
        disabled={!onPress}
        accessibilityRole="button"
        accessibilityLabel={label}
        accessibilityState={{ selected, disabled: !onPress }}
        className="active:opacity-70"
        {...pressableProps}
      >
        <Text
          className={[
            "text-caption font-medium",
            selected ? "text-brand dark:text-brand-light" : "text-ink-secondary dark:text-ink-secondary-dark",
          ].join(" ")}
        >
          {label}
        </Text>
      </Pressable>
      {onDismiss ? (
        <Pressable
          onPress={onDismiss}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel={`Remove ${label}`}
          className="ml-1.5 active:opacity-60"
        >
          <Icon name="close" size={14} color={selected ? colors.brand : colors.textSecondary} />
        </Pressable>
      ) : null}
    </View>
  );
}

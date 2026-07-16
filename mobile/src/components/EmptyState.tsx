import React from "react";
import { Text, View } from "react-native";

import { Icon, type IconName } from "./Icon";
import { Button } from "./Button";
import { useTheme } from "../theme/ThemeProvider";

export interface EmptyStateProps {
  icon?: IconName;
  title: string;
  description?: string;
  actionLabel?: string;
  onAction?: () => void;
  className?: string;
}

/** Centered icon + copy shown when a list/screen has no content yet. */
export function EmptyState({
  icon = "file-tray-outline",
  title,
  description,
  actionLabel,
  onAction,
  className = "",
}: EmptyStateProps) {
  const { colors } = useTheme();

  return (
    <View className={["items-center justify-center px-6 py-10", className].join(" ")}>
      <View className="mb-4 h-16 w-16 items-center justify-center rounded-full bg-surface dark:bg-surface-dark">
        <Icon name={icon} size={30} color={colors.textSecondary} />
      </View>
      <Text className="mb-1.5 text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
        {title}
      </Text>
      {description ? (
        <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
          {description}
        </Text>
      ) : null}
      {actionLabel && onAction ? (
        <View className="mt-5 self-stretch">
          <Button title={actionLabel} onPress={onAction} />
        </View>
      ) : null}
    </View>
  );
}

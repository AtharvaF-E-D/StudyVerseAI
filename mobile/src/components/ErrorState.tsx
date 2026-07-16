import React from "react";
import { Text, View } from "react-native";

import { Icon, type IconName } from "./Icon";
import { Button } from "./Button";
import { useTheme } from "../theme/ThemeProvider";

export interface ErrorStateProps {
  icon?: IconName;
  title?: string;
  description?: string;
  onRetry?: () => void;
  retryLabel?: string;
  className?: string;
}

/**
 * Same shape family as `EmptyState`, danger-toned, for failed loads —
 * shown with a "Try again" action wired to `onRetry`.
 */
export function ErrorState({
  icon = "alert-circle-outline",
  title = "Something went wrong",
  description = "Please try again.",
  onRetry,
  retryLabel = "Try again",
  className = "",
}: ErrorStateProps) {
  const { colors } = useTheme();

  return (
    <View className={["items-center justify-center px-6 py-10", className].join(" ")}>
      <View className="mb-4 h-16 w-16 items-center justify-center rounded-full bg-danger/10">
        <Icon name={icon} size={30} color={colors.danger} />
      </View>
      <Text className="mb-1.5 text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
        {title}
      </Text>
      <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
        {description}
      </Text>
      {onRetry ? (
        <View className="mt-5 self-stretch">
          <Button title={retryLabel} variant="secondary" onPress={onRetry} />
        </View>
      ) : null}
    </View>
  );
}

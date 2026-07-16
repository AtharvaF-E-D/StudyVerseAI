import React from "react";
import { Pressable, Text, View, type PressableProps } from "react-native";

export interface ListItemProps extends Omit<PressableProps, "children" | "onPress"> {
  /** Leading slot — typically an `Icon` or `Avatar`. */
  leading?: React.ReactNode;
  title: string;
  subtitle?: string;
  /** Trailing slot — typically a chevron `Icon`, `Badge`, or `Switch`. */
  trailing?: React.ReactNode;
  onPress?: () => void;
  className?: string;
}

/**
 * Row primitive for settings screens, dashboards, and quiz/flashcard
 * lists. Renders as a `Pressable` with press-state dimming when `onPress`
 * is supplied, otherwise as a plain `View` so non-interactive rows don't
 * pick up button semantics/focus.
 */
export function ListItem({
  leading,
  title,
  subtitle,
  trailing,
  onPress,
  className = "",
  ...pressableProps
}: ListItemProps) {
  const content = (
    <>
      {leading ? <View className="mr-3">{leading}</View> : null}
      <View className="flex-1">
        <Text numberOfLines={1} className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">
          {title}
        </Text>
        {subtitle ? (
          <Text numberOfLines={1} className="mt-0.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {subtitle}
          </Text>
        ) : null}
      </View>
      {trailing ? <View className="ml-3">{trailing}</View> : null}
    </>
  );

  if (onPress) {
    return (
      <Pressable
        onPress={onPress}
        accessibilityRole="button"
        accessibilityLabel={title}
        className={[
          "flex-row items-center rounded-md px-3 py-3 active:bg-surface dark:active:bg-surface-dark",
          className,
        ].join(" ")}
        {...pressableProps}
      >
        {content}
      </Pressable>
    );
  }

  return <View className={["flex-row items-center px-3 py-3", className].join(" ")}>{content}</View>;
}

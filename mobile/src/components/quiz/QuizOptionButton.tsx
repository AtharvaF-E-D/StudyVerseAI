import React from "react";
import { Pressable, Text, type PressableProps } from "react-native";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";

export type QuizOptionState = "default" | "selected" | "correct" | "incorrect" | "eliminated" | "dimmed";

export interface QuizOptionButtonProps extends Omit<PressableProps, "children" | "onPress"> {
  label: string;
  state: QuizOptionState;
  onPress?: () => void;
  className?: string;
}

const containerStateClasses: Record<QuizOptionState, string> = {
  default: "border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
  selected: "border-brand bg-brand/10 dark:border-brand-light dark:bg-brand-light/15",
  correct: "border-success bg-success/15",
  incorrect: "border-danger bg-danger/15",
  eliminated: "border-border bg-surface opacity-40 dark:border-border-dark dark:bg-surface-dark",
  dimmed: "border-border bg-surface opacity-60 dark:border-border-dark dark:bg-surface-dark",
};

const labelStateClasses: Record<QuizOptionState, string> = {
  default: "text-ink-primary dark:text-ink-primary-dark",
  selected: "text-brand dark:text-brand-light",
  correct: "text-success",
  incorrect: "text-danger",
  eliminated: "text-ink-secondary dark:text-ink-secondary-dark",
  dimmed: "text-ink-secondary dark:text-ink-secondary-dark",
};

/**
 * One tappable answer option on the quiz play screen. `state` drives the
 * post-answer feedback coloring and the 50-50 power-up's "eliminated" look;
 * it's a controlled/presentational button, not a `Button` variant, since no
 * existing `Button` variant models "correct"/"incorrect"/"eliminated".
 */
export function QuizOptionButton({
  label,
  state,
  onPress,
  className = "",
  ...pressableProps
}: QuizOptionButtonProps) {
  const { colors } = useTheme();
  const disabled = state === "eliminated" || !onPress;

  return (
    <Pressable
      accessibilityRole="button"
      accessibilityLabel={label}
      accessibilityState={{ disabled, selected: state === "selected" }}
      disabled={disabled}
      onPress={onPress}
      className={[
        "mb-3 flex-row items-center rounded-xl border px-4 py-3.5 active:opacity-80",
        containerStateClasses[state],
        className,
      ].join(" ")}
      {...pressableProps}
    >
      <Text className={["flex-1 text-body font-medium", labelStateClasses[state]].join(" ")}>{label}</Text>
      {state === "correct" ? <Icon name="checkmark-circle" size={20} color={colors.success} /> : null}
      {state === "incorrect" ? <Icon name="close-circle" size={20} color={colors.danger} /> : null}
    </Pressable>
  );
}

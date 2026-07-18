import React from "react";
import { Text, View } from "react-native";

import { Card } from "../Card";
import { Badge } from "../Badge";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";

export interface QuizReviewItemProps {
  index: number;
  questionText: string;
  options: string[];
  selectedOptionIndex: number | null;
  correctOptionIndex: number;
  explanation: string;
  /**
   * Badge label shown when `selectedOptionIndex` is null. Defaults to the
   * Rapid Fire Quiz's own wording ("Timed out" — its per-question timer is
   * the only way a question there goes unanswered). Callers whose "no
   * answer" case isn't a timeout — e.g. Mock Tests, where free navigation
   * between questions means a question can simply be skipped — should pass
   * something like "Unanswered" instead.
   */
  unansweredLabel?: string;
  className?: string;
}

/**
 * One reviewed question: the prompt, every option (the correct one and any
 * incorrectly-selected one visually distinguished), and the explanation.
 * Used by the real review screens for both Rapid Fire Quiz
 * (`app/(app)/quiz/[sessionId]/review.tsx`) and Mock Tests
 * (`app/(app)/mocktests/[attemptId]/review.tsx`), which share the exact same
 * per-question review shape, plus both features' `(dev)` fixture screens.
 */
export function QuizReviewItem({
  index,
  questionText,
  options,
  selectedOptionIndex,
  correctOptionIndex,
  explanation,
  unansweredLabel = "Timed out",
  className = "",
}: QuizReviewItemProps) {
  const { colors } = useTheme();
  const wasAnswered = selectedOptionIndex !== null;
  const isCorrect = wasAnswered && selectedOptionIndex === correctOptionIndex;

  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-start justify-between">
        <Text className="mr-3 flex-1 text-body font-medium text-ink-primary dark:text-ink-primary-dark">
          {index + 1}. {questionText}
        </Text>
        <Badge
          label={isCorrect ? "Correct" : wasAnswered ? "Incorrect" : unansweredLabel}
          variant={isCorrect ? "success" : "danger"}
        />
      </View>

      <View className="mb-3">
        {options.map((option, optionIndex) => {
          const isCorrectOption = optionIndex === correctOptionIndex;
          const isSelectedWrong = optionIndex === selectedOptionIndex && !isCorrectOption;
          return (
            <View
              key={optionIndex}
              className={[
                "mb-1.5 flex-row items-center rounded-lg border px-3 py-2",
                isCorrectOption
                  ? "border-success bg-success/10"
                  : isSelectedWrong
                    ? "border-danger bg-danger/10"
                    : "border-border dark:border-border-dark",
              ].join(" ")}
            >
              <Text
                className={[
                  "flex-1 text-caption",
                  isCorrectOption
                    ? "text-success"
                    : isSelectedWrong
                      ? "text-danger"
                      : "text-ink-secondary dark:text-ink-secondary-dark",
                ].join(" ")}
              >
                {option}
              </Text>
              {isCorrectOption ? <Icon name="checkmark-circle" size={16} color={colors.success} /> : null}
              {isSelectedWrong ? <Icon name="close-circle" size={16} color={colors.danger} /> : null}
            </View>
          );
        })}
      </View>

      {explanation ? (
        <View className="rounded-lg bg-surface px-3 py-2.5 dark:bg-surface-dark">
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{explanation}</Text>
        </View>
      ) : null}
    </Card>
  );
}

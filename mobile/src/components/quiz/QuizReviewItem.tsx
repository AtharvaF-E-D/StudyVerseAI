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
  className?: string;
}

/**
 * One reviewed question: the prompt, every option (the correct one and any
 * incorrectly-selected one visually distinguished), and the explanation.
 * Used by both the real review screen (`app/(app)/quiz/[sessionId]/review.tsx`)
 * and the `(dev)/quiz-preview` fixture screen.
 */
export function QuizReviewItem({
  index,
  questionText,
  options,
  selectedOptionIndex,
  correctOptionIndex,
  explanation,
  className = "",
}: QuizReviewItemProps) {
  const { colors } = useTheme();
  const wasAnswered = selectedOptionIndex !== null;
  const isCorrect = wasAnswered && selectedOptionIndex === correctOptionIndex;

  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-start justify-between">
        <Text className="mr-3 flex-1 text-bodyMedium text-ink-primary dark:text-ink-primary-dark">
          {index + 1}. {questionText}
        </Text>
        <Badge
          label={isCorrect ? "Correct" : wasAnswered ? "Incorrect" : "Timed out"}
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

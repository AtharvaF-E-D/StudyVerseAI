import React, { useState } from "react";
import { Text, View } from "react-native";

import { Badge } from "../Badge";
import { Card } from "../Card";
import { QuizOptionButton, type QuizOptionState } from "../quiz/QuizOptionButton";
import type { McqDto } from "../../api/notes";

export interface McqItemProps {
  index: number;
  mcq: McqDto;
  className?: string;
}

/**
 * One multiple-choice study question. Tapping an option locks it in and
 * reveals the correct answer (green), the picked option if it was wrong
 * (red), and the explanation — reusing the Rapid Fire Quiz's
 * `QuizOptionButton` for the same tap/reveal coloring rather than inventing a
 * new option control. Deliberately no scoring or session tracking: this is a
 * study aid, not the Phase 5 quiz engine.
 */
export function McqItem({ index, mcq, className = "" }: McqItemProps) {
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const revealed = selectedIndex !== null;
  const isCorrect = selectedIndex === mcq.correctOptionIndex;

  function optionState(optionIndex: number): QuizOptionState {
    if (!revealed) return "default";
    if (optionIndex === mcq.correctOptionIndex) return "correct";
    if (optionIndex === selectedIndex) return "incorrect";
    return "dimmed";
  }

  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-start justify-between">
        <Text className="mr-3 flex-1 text-bodyMedium text-ink-primary dark:text-ink-primary-dark">
          {index + 1}. {mcq.question}
        </Text>
        {revealed ? (
          <Badge label={isCorrect ? "Correct" : "Incorrect"} variant={isCorrect ? "success" : "danger"} />
        ) : null}
      </View>

      {mcq.options.map((option, optionIndex) => (
        <QuizOptionButton
          key={optionIndex}
          label={option}
          state={optionState(optionIndex)}
          onPress={revealed ? undefined : () => setSelectedIndex(optionIndex)}
        />
      ))}

      {revealed && mcq.explanation ? (
        <View className="mt-1 rounded-lg bg-surface px-3 py-2.5 dark:bg-surface-dark">
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{mcq.explanation}</Text>
        </View>
      ) : null}
    </Card>
  );
}

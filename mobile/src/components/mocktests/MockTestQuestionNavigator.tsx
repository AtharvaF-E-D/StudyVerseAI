import React from "react";
import { Pressable, ScrollView, Text } from "react-native";

export interface MockTestQuestionNavigatorProps {
  totalQuestions: number;
  currentIndex: number;
  /** `answeredFlags[i]` is true once the question at index `i` has a locally-stored answer. */
  answeredFlags: boolean[];
  onSelect: (index: number) => void;
  className?: string;
}

/**
 * Horizontal row of numbered chips for jumping directly to any question in
 * the attempt. Unlike Rapid Fire Quiz's strictly linear, one-question-at-a-
 * time flow, a mock test is a single exam with free navigation — every
 * question needs to stay reachable (and its answered/unanswered status
 * visible at a glance) at all times, not just the current one.
 */
export function MockTestQuestionNavigator({
  totalQuestions,
  currentIndex,
  answeredFlags,
  onSelect,
  className = "",
}: MockTestQuestionNavigatorProps) {
  return (
    <ScrollView
      horizontal
      showsHorizontalScrollIndicator={false}
      className={className}
      contentContainerClassName="gap-2 px-0.5 py-0.5"
    >
      {Array.from({ length: totalQuestions }, (_, index) => {
        const isCurrent = index === currentIndex;
        const answered = answeredFlags[index] ?? false;
        return (
          <Pressable
            key={index}
            onPress={() => onSelect(index)}
            accessibilityRole="button"
            accessibilityLabel={`Question ${index + 1}, ${answered ? "answered" : "unanswered"}${isCurrent ? ", current question" : ""}`}
            accessibilityState={{ selected: isCurrent }}
            className={[
              "h-9 w-9 items-center justify-center rounded-full border active:opacity-80",
              isCurrent
                ? "border-brand bg-brand dark:border-brand-light dark:bg-brand-light"
                : answered
                  ? "border-brand bg-brand/15 dark:border-brand-light dark:bg-brand-light/20"
                  : "border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
            ].join(" ")}
          >
            <Text
              className={[
                "text-caption font-semibold",
                isCurrent
                  ? "text-white"
                  : answered
                    ? "text-brand dark:text-brand-light"
                    : "text-ink-secondary dark:text-ink-secondary-dark",
              ].join(" ")}
            >
              {index + 1}
            </Text>
          </Pressable>
        );
      })}
    </ScrollView>
  );
}

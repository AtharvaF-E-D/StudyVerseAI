import React from "react";
import { Text, View } from "react-native";

import { Card } from "../Card";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import type { SubmitMockTestAttemptResponse } from "../../api/mocktests";

export interface MockTestResultSummaryCardProps {
  result: Pick<SubmitMockTestAttemptResponse, "score" | "correctCount" | "totalQuestions" | "percentileRank">;
  className?: string;
}

/**
 * Score + a prominent percentile framing shown at the top of the results
 * screen. Deliberately phrases `percentileRank` (0-100, already a
 * percentage) as a sentence — "you scored better than 72% of test-takers" —
 * rather than just printing the raw number, since that sentence is what
 * actually answers "how did I do?" at a glance.
 */
export function MockTestResultSummaryCard({ result, className = "" }: MockTestResultSummaryCardProps) {
  const { colors } = useTheme();
  const roundedPercentile = Math.round(result.percentileRank);

  return (
    <Card className={className}>
      <View className="items-center">
        <Icon name="trophy" size={32} color={colors.warning} />
        <Text className="mt-2 text-display text-ink-primary dark:text-ink-primary-dark">{result.score}</Text>
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
          {result.correctCount} of {result.totalQuestions} correct
        </Text>
      </View>
      <View className="mt-4 items-center rounded-lg bg-brand/10 px-4 py-3 dark:bg-brand-light/10">
        <Text className="text-center text-body font-semibold text-brand dark:text-brand-light">
          You scored better than {roundedPercentile}% of test-takers
        </Text>
      </View>
    </Card>
  );
}

import React from "react";
import { Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../../src/components/ScreenContainer";
import { Button } from "../../../../src/components/Button";
import { Card } from "../../../../src/components/Card";
import { ErrorState } from "../../../../src/components/ErrorState";
import { Icon } from "../../../../src/components/Icon";
import { Skeleton } from "../../../../src/components/Skeleton";
import { MockTestResultSummaryCard } from "../../../../src/components/mocktests/MockTestResultSummaryCard";
import { useTheme } from "../../../../src/theme/ThemeProvider";
import { useMockTestAttemptQuery } from "../../../../src/hooks/useMockTests";

function ResultsSkeleton() {
  return (
    <View>
      <Card className="mb-5 items-center">
        <Skeleton variant="circle" className="mb-3" />
        <Skeleton variant="text" width={80} className="mb-2" />
        <Skeleton variant="text" width={140} />
      </Card>
      <Card>
        <Skeleton variant="text" width="40%" className="mb-2" />
        <Skeleton variant="text" width="90%" className="mb-1" />
        <Skeleton variant="text" width="70%" />
      </Card>
    </View>
  );
}

/**
 * Post-submission results for one mock test attempt: score/correctCount, a
 * prominent percentile framing, the real AI-generated weakness analysis, and
 * a way into the full review screen.
 */
export default function MockTestResultsScreen() {
  const params = useLocalSearchParams<{ attemptId: string }>();
  const attemptId = params.attemptId ?? "";
  const { colors } = useTheme();

  const attemptQuery = useMockTestAttemptQuery(attemptId);
  const attempt = attemptQuery.data;

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center">
        <Pressable
          onPress={() => router.replace("/(app)/mocktests")}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back to mock tests"
          className="mr-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        <View className="flex-1">
          <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Results</Text>
          {attempt ? (
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {attempt.templateTitle}
            </Text>
          ) : null}
        </View>
      </View>

      {attemptQuery.isLoading ? (
        <ResultsSkeleton />
      ) : attemptQuery.isError || !attempt ? (
        <ErrorState
          title="Couldn't load your results"
          description="Check your connection and try again."
          onRetry={() => void attemptQuery.refetch()}
        />
      ) : (
        <>
          <MockTestResultSummaryCard result={attempt} className="mb-5" />

          <Card className="mb-6">
            <View className="mb-2 flex-row items-center">
              <Icon name="bulb-outline" size={20} color={colors.brand} />
              <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
                Where to focus next
              </Text>
            </View>
            <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
              {attempt.aiWeaknessAnalysis}
            </Text>
          </Card>

          <View className="mb-3">
            <Button title="Review answers" onPress={() => router.push(`/(app)/mocktests/${attemptId}/review`)} />
          </View>
          <Button title="Back to mock tests" variant="secondary" onPress={() => router.replace("/(app)/mocktests")} />
        </>
      )}
    </ScreenContainer>
  );
}

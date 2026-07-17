import React from "react";
import { Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../../src/components/ScreenContainer";
import { Button } from "../../../../src/components/Button";
import { Card } from "../../../../src/components/Card";
import { EmptyState } from "../../../../src/components/EmptyState";
import { ErrorState } from "../../../../src/components/ErrorState";
import { Icon } from "../../../../src/components/Icon";
import { Skeleton } from "../../../../src/components/Skeleton";
import { QuizReviewItem } from "../../../../src/components/quiz/QuizReviewItem";
import { useTheme } from "../../../../src/theme/ThemeProvider";
import { useQuizReviewQuery } from "../../../../src/hooks/useQuiz";

function ReviewSkeleton() {
  return (
    <View>
      {[0, 1, 2].map((i) => (
        <Card key={i} className="mb-4">
          <Skeleton variant="text" width="85%" className="mb-3" />
          <Skeleton variant="rect" height={36} className="mb-2 rounded-lg" />
          <Skeleton variant="rect" height={36} className="mb-2 rounded-lg" />
          <Skeleton variant="rect" height={36} className="rounded-lg" />
        </Card>
      ))}
    </View>
  );
}

/** Completed-session review: every question, the user's answer, the correct answer, and the explanation. */
export default function QuizReviewScreen() {
  const params = useLocalSearchParams<{ sessionId: string }>();
  const sessionId = params.sessionId ?? "";
  const { colors } = useTheme();

  const reviewQuery = useQuizReviewQuery(sessionId);
  const items = reviewQuery.data?.questions ?? [];
  const correctCount = items.filter((item) => item.isCorrect === true).length;

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center">
        <Pressable
          onPress={() => router.back()}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back"
          className="mr-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Review</Text>
      </View>

      {reviewQuery.isLoading ? (
        <ReviewSkeleton />
      ) : reviewQuery.isError ? (
        <ErrorState
          title="Couldn't load the review"
          description="Check your connection and try again."
          onRetry={() => void reviewQuery.refetch()}
        />
      ) : items.length === 0 ? (
        <EmptyState icon="document-text-outline" title="No answers to review" />
      ) : (
        <>
          <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {correctCount} of {items.length} correct
          </Text>
          {items.map((item, index) => (
            <QuizReviewItem
              key={item.questionId}
              index={index}
              questionText={item.questionText}
              options={item.options}
              selectedOptionIndex={item.selectedOptionIndex}
              correctOptionIndex={item.correctOptionIndex}
              explanation={item.explanation}
              className="mb-4"
            />
          ))}
          <Button title="Done" variant="secondary" onPress={() => router.replace("/(app)/quiz")} />
        </>
      )}
    </ScreenContainer>
  );
}

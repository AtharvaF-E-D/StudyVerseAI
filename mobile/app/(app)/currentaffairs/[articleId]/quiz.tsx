import React, { useState } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../../src/components/ScreenContainer";
import { Button } from "../../../../src/components/Button";
import { Card } from "../../../../src/components/Card";
import { ErrorState } from "../../../../src/components/ErrorState";
import { Icon } from "../../../../src/components/Icon";
import { QuizOptionButton, type QuizOptionState } from "../../../../src/components/quiz/QuizOptionButton";
import { useTheme } from "../../../../src/theme/ThemeProvider";
import { useArticleQuizQuery } from "../../../../src/hooks/useCurrentAffairs";

/**
 * Shown while `GET /articles/{id}/quiz` is in flight. The FIRST call for a
 * given article generates the quiz for real via OpenAI server-side (cached
 * after that per the contract), which the phase spec calls out as
 * potentially slow — this is deliberately a spinner + explanatory copy
 * rather than a `Skeleton` (which reads as "content is basically ready"),
 * since a real wait of up to ~a minute is the honest expectation to set.
 */
function QuizGeneratingState() {
  const { colors } = useTheme();
  return (
    <View className="flex-1 items-center justify-center px-6">
      <ActivityIndicator size="large" color={colors.brand} />
      <Text className="mb-2 mt-4 text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
        Generating your quiz…
      </Text>
      <Text className="text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
        The first quiz for an article is written fresh by AI from its actual content — this can take up to a
        minute. It&apos;s instant after that.
      </Text>
    </View>
  );
}

/**
 * Per-article quiz: one question at a time, tap an option to see
 * correct/incorrect + explanation immediately, then advance manually — no
 * "session"/lives/timer concept here (unlike the Rapid Fire Quiz feature
 * this reuses `QuizOptionButton` from), so grading is done client-side
 * against the questions payload the backend already returned in full.
 */
export default function ArticleQuizScreen() {
  const params = useLocalSearchParams<{ articleId: string }>();
  const articleId = params.articleId ?? "";
  const { colors } = useTheme();

  const quizQuery = useArticleQuizQuery(articleId);

  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const [revealed, setRevealed] = useState(false);
  const [correctCount, setCorrectCount] = useState(0);
  const [isComplete, setIsComplete] = useState(false);

  const questions = quizQuery.data?.questions ?? [];
  const totalQuestions = questions.length;
  const currentQuestion = questions[currentIndex];
  const isLastQuestion = currentIndex + 1 >= totalQuestions;

  function handleSelectOption(optionIndex: number) {
    if (revealed || !currentQuestion) return;
    setSelectedIndex(optionIndex);
    setRevealed(true);
    if (optionIndex === currentQuestion.correctOptionIndex) {
      setCorrectCount((prev) => prev + 1);
    }
  }

  function handleNext() {
    if (isLastQuestion) {
      setIsComplete(true);
      return;
    }
    setCurrentIndex((prev) => prev + 1);
    setSelectedIndex(null);
    setRevealed(false);
  }

  function optionState(optionIndex: number): QuizOptionState {
    if (!currentQuestion) return "default";
    if (!revealed) return optionIndex === selectedIndex ? "selected" : "default";
    if (optionIndex === currentQuestion.correctOptionIndex) return "correct";
    if (optionIndex === selectedIndex) return "incorrect";
    return "dimmed";
  }

  if (quizQuery.isLoading) {
    return (
      <ScreenContainer scrollable={false}>
        <QuizGeneratingState />
      </ScreenContainer>
    );
  }

  if (quizQuery.isError) {
    return (
      <ScreenContainer>
        <ErrorState
          title="Couldn't generate this quiz"
          description="AI generation can occasionally time out — please try again."
          onRetry={() => void quizQuery.refetch()}
        />
      </ScreenContainer>
    );
  }

  if (totalQuestions === 0 || !currentQuestion) {
    return (
      <ScreenContainer>
        <ErrorState
          icon="help-circle-outline"
          title="No quiz available"
          description="This article doesn't have a quiz yet."
          retryLabel="Back to article"
          onRetry={() => router.back()}
        />
      </ScreenContainer>
    );
  }

  if (isComplete) {
    return (
      <ScreenContainer>
        <Card className="mb-6 items-center">
          <Icon
            name={correctCount === totalQuestions ? "trophy" : "checkmark-done-circle"}
            size={40}
            color={colors.brand}
          />
          <Text className="mb-1 mt-3 text-heading text-ink-primary dark:text-ink-primary-dark">
            Quiz complete!
          </Text>
          <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            {correctCount} of {totalQuestions} correct
          </Text>
        </Card>
        <Button title="Done" onPress={() => router.back()} />
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer scrollable={false}>
      <View className="flex-1">
        <View className="mb-5 flex-row items-center justify-between">
          <Pressable
            onPress={() => router.back()}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Back"
            className="h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon name="chevron-back" size={22} color={colors.textPrimary} />
          </Pressable>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Question {currentIndex + 1} / {totalQuestions}
          </Text>
          <View className="h-9 w-9" />
        </View>

        <Text className="mb-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {currentQuestion.questionText}
        </Text>

        {currentQuestion.options.map((option, optionIndex) => (
          <QuizOptionButton
            key={optionIndex}
            label={option}
            state={optionState(optionIndex)}
            onPress={revealed ? undefined : () => handleSelectOption(optionIndex)}
          />
        ))}

        {revealed ? (
          <Card className="mb-4">
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {currentQuestion.explanation}
            </Text>
          </Card>
        ) : null}

        <View className="mt-auto">
          <Button
            title={isLastQuestion ? "See results" : "Next question"}
            disabled={!revealed}
            onPress={handleNext}
          />
        </View>
      </View>
    </ScreenContainer>
  );
}

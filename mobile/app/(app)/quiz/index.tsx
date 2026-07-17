import React, { useState } from "react";
import { Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Chip } from "../../../src/components/Chip";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { ListItem } from "../../../src/components/ListItem";
import { Skeleton } from "../../../src/components/Skeleton";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { stashStartedQuizSession } from "../../../src/lib/quizSessionCache";
import type { QuizCategoryDto, QuizDifficulty } from "../../../src/api/quiz";
import {
  useDailyChallengeStatusQuery,
  useQuizCategoriesQuery,
  useQuizStatsQuery,
  useStartQuizSessionMutation,
} from "../../../src/hooks/useQuiz";

const DIFFICULTIES: QuizDifficulty[] = ["easy", "medium", "hard"];
const DAILY_CHALLENGE_KEY = "__daily_challenge__";

/** The wire value is lowercase (`"easy"`) per the backend's camelCase enum serialization; this is purely for display. */
const DIFFICULTY_LABELS: Record<QuizDifficulty, string> = {
  easy: "Easy",
  medium: "Medium",
  hard: "Hard",
};

function countForDifficulty(category: QuizCategoryDto, difficulty: QuizDifficulty): number {
  if (difficulty === "easy") return category.easyCount;
  if (difficulty === "medium") return category.mediumCount;
  return category.hardCount;
}

function StatsStripSkeleton() {
  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        {[0, 1, 2].map((i) => (
          <View key={i} className="flex-1 items-center">
            <Skeleton variant="text" width={32} className="mb-2" />
            <Skeleton variant="text" width={56} />
          </View>
        ))}
      </View>
    </Card>
  );
}

function CategoryListSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="px-3 py-3">
          <Skeleton variant="text" width="40%" className="mb-2" />
          <Skeleton variant="text" width="65%" />
        </View>
      ))}
    </Card>
  );
}

/**
 * Category/difficulty picker — the "Rapid Fire Quiz" entry point reached from
 * the dashboard. Starting a session (regular or daily-challenge) stashes the
 * full question payload in `quizSessionCache` and navigates to the play
 * screen, which reads it back on mount (see that screen's file header for why).
 */
export default function QuizPickerScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [selectedDifficulty, setSelectedDifficulty] = useState<QuizDifficulty>("easy");
  const [startingKey, setStartingKey] = useState<string | null>(null);

  const statsQuery = useQuizStatsQuery();
  const categoriesQuery = useQuizCategoriesQuery();
  const dailyChallengeQuery = useDailyChallengeStatusQuery();
  const startSessionMutation = useStartQuizSessionMutation();

  function startSession(category: string, difficulty: QuizDifficulty, isDailyChallenge: boolean, key: string) {
    if (startSessionMutation.isPending) return;
    setStartingKey(key);
    startSessionMutation.mutate(
      { category, difficulty, isDailyChallenge },
      {
        onSuccess: (result) => {
          stashStartedQuizSession(result.sessionId, result);
          setStartingKey(null);
          router.push(`/(app)/quiz/${result.sessionId}`);
        },
        onError: () => {
          setStartingKey(null);
          show("Couldn't start the quiz. Please try again.", "danger");
        },
      },
    );
  }

  const categories = categoriesQuery.data ?? [];
  const dailyChallenge = dailyChallengeQuery.data;

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Rapid Fire Quiz</Text>

      {statsQuery.isLoading ? (
        <StatsStripSkeleton />
      ) : statsQuery.data ? (
        <Card className="mb-6">
          <View className="flex-row items-center justify-between">
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {statsQuery.data.totalSessionsPlayed}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Played</Text>
            </View>
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {Math.round(statsQuery.data.accuracyPercent)}%
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Accuracy</Text>
            </View>
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {statsQuery.data.bestComboEver}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Best combo</Text>
            </View>
          </View>
        </Card>
      ) : null}

      {dailyChallenge ? (
        <View className="mb-6">
          <Card className={dailyChallenge.completedToday ? "" : "border-brand dark:border-brand-light"}>
            <View className="mb-1 flex-row items-center justify-between">
              <View className="flex-row items-center">
                <Icon name="trophy" size={18} color={colors.warning} />
                <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
                  Today&apos;s Daily Challenge
                </Text>
              </View>
              {dailyChallenge.completedToday ? <Badge label="Completed" variant="success" /> : null}
            </View>
            <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {dailyChallenge.category} · {DIFFICULTY_LABELS[dailyChallenge.difficulty]}
            </Text>
            <Button
              title={dailyChallenge.completedToday ? "Come back tomorrow" : "Start daily challenge"}
              disabled={dailyChallenge.completedToday}
              loading={startingKey === DAILY_CHALLENGE_KEY}
              onPress={() =>
                startSession(dailyChallenge.category, dailyChallenge.difficulty, true, DAILY_CHALLENGE_KEY)
              }
            />
          </Card>
        </View>
      ) : null}

      <View className="mb-4 flex-row gap-2">
        {DIFFICULTIES.map((difficulty) => (
          <Chip
            key={difficulty}
            label={DIFFICULTY_LABELS[difficulty]}
            selected={selectedDifficulty === difficulty}
            onPress={() => setSelectedDifficulty(difficulty)}
          />
        ))}
      </View>

      {categoriesQuery.isLoading ? (
        <CategoryListSkeleton />
      ) : categoriesQuery.isError ? (
        <ErrorState
          title="Couldn't load quiz categories"
          description="Check your connection and try again."
          onRetry={() => void categoriesQuery.refetch()}
        />
      ) : categories.length === 0 ? (
        <EmptyState icon="help-circle-outline" title="No quiz categories yet" />
      ) : (
        <Card>
          {categories.map((category, index) => {
            const count = countForDifficulty(category, selectedDifficulty);
            const key = `${category.category}:${selectedDifficulty}`;
            return (
              <React.Fragment key={category.category}>
                {index > 0 ? <Divider /> : null}
                <ListItem
                  leading={<Icon name="albums-outline" size={22} color={colors.brand} />}
                  title={category.category}
                  subtitle={`${count} ${DIFFICULTY_LABELS[selectedDifficulty].toLowerCase()} question${count === 1 ? "" : "s"}`}
                  trailing={
                    <Button
                      title="Start"
                      fullWidth={false}
                      disabled={count === 0}
                      loading={startingKey === key}
                      onPress={() => startSession(category.category, selectedDifficulty, false, key)}
                    />
                  }
                />
              </React.Fragment>
            );
          })}
        </Card>
      )}
    </ScreenContainer>
  );
}

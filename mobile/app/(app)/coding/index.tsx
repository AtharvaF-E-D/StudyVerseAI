import React, { useMemo, useState } from "react";
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
import { Switch } from "../../../src/components/Switch";
import { ProblemRow } from "../../../src/components/coding/ProblemRow";
import { DIFFICULTY_LABELS } from "../../../src/components/coding/difficulty";
import { useTheme } from "../../../src/theme/ThemeProvider";
import type { CodingDifficulty, ProblemSummaryDto } from "../../../src/api/codingpractice";
import { useCodingStatsQuery, useDailyChallengeQuery, useProblemsQuery } from "../../../src/hooks/useCodingPractice";

type DifficultyFilter = "all" | CodingDifficulty;
const DIFFICULTY_FILTERS: DifficultyFilter[] = ["all", "easy", "medium", "hard"];
const ALL_CATEGORIES = "all";

function difficultyFilterLabel(filter: DifficultyFilter): string {
  return filter === "all" ? "All" : DIFFICULTY_LABELS[filter];
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

function ProblemListSkeleton() {
  return (
    <Card>
      {[0, 1, 2, 3, 4].map((i) => (
        <View key={i} className="px-3 py-3">
          <Skeleton variant="text" width="55%" className="mb-2" />
          <Skeleton variant="text" width="30%" />
        </View>
      ))}
    </Card>
  );
}

/**
 * Stats strip: solved count + daily streak (the two the brief calls out
 * explicitly) alongside total submissions, same three-column layout as the
 * Rapid Fire Quiz picker's stats strip (`app/(app)/quiz/index.tsx`).
 */
function StatsStrip() {
  const statsQuery = useCodingStatsQuery();

  if (statsQuery.isLoading) return <StatsStripSkeleton />;
  if (statsQuery.isError || !statsQuery.data) return null;

  const stats = statsQuery.data;

  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        <View className="flex-1 items-center">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{stats.totalSolved}</Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Solved</Text>
        </View>
        <View className="flex-1 items-center">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {stats.currentDailyStreak}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Day streak</Text>
        </View>
        <View className="flex-1 items-center">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {stats.totalSubmissions}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Submissions</Text>
        </View>
      </View>
    </Card>
  );
}

/** Daily-challenge teaser Card. Degrades silently on error/empty (lower priority than the core browse/solve loop — see the phase report). */
function DailyChallengeCard() {
  const { colors } = useTheme();
  const dailyChallengeQuery = useDailyChallengeQuery();

  if (dailyChallengeQuery.isLoading) {
    return (
      <Card className="mb-6">
        <Skeleton variant="text" width="50%" className="mb-2" />
        <Skeleton variant="text" width="70%" />
      </Card>
    );
  }

  if (dailyChallengeQuery.isError || !dailyChallengeQuery.data) return null;

  const challenge = dailyChallengeQuery.data;

  return (
    <Card className="mb-6 border-brand dark:border-brand-light">
      <View className="mb-1 flex-row items-center justify-between">
        <View className="flex-row items-center">
          <Icon name="trophy" size={18} color={colors.warning} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Today&apos;s Daily Challenge
          </Text>
        </View>
        <Badge label={DIFFICULTY_LABELS[challenge.difficulty]} variant="brand" />
      </View>
      <Text className="mb-3 text-body text-ink-primary dark:text-ink-primary-dark">{challenge.title}</Text>
      <Button title="Solve today's challenge" onPress={() => router.push(`/(app)/coding/${challenge.problemId}`)} />
    </Card>
  );
}

/**
 * Coding Practice problems list — stats strip, daily challenge, difficulty/
 * category/interview-only filters, then the problem bank filtered
 * server-side. Category options are derived client-side from one unfiltered
 * fetch of the problem bank since the contract has no dedicated categories
 * endpoint (unlike Current Affairs/Rapid Fire Quiz, which both do).
 */
export default function CodingPracticeScreen() {
  const { colors } = useTheme();
  const [selectedDifficulty, setSelectedDifficulty] = useState<DifficultyFilter>("all");
  const [selectedCategory, setSelectedCategory] = useState<string>(ALL_CATEGORIES);
  const [interviewOnly, setInterviewOnly] = useState(false);

  // Unfiltered fetch used only to derive the set of categories to show as
  // chips — kept independent of the current filter selections so the
  // category options themselves don't shift as the user filters.
  const categorySourceQuery = useProblemsQuery({});
  const categoryOptions = useMemo(() => {
    const categories = new Set((categorySourceQuery.data ?? []).map((p) => p.category));
    return Array.from(categories).sort((a, b) => a.localeCompare(b));
  }, [categorySourceQuery.data]);

  const problemsQuery = useProblemsQuery({
    difficulty: selectedDifficulty === "all" ? undefined : selectedDifficulty,
    category: selectedCategory === ALL_CATEGORIES ? undefined : selectedCategory,
    interviewOnly: interviewOnly ? true : undefined,
  });

  function openProblem(problem: ProblemSummaryDto) {
    router.push(`/(app)/coding/${problem.id}`);
  }

  const problems = problemsQuery.data ?? [];

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Coding Practice</Text>

      <StatsStrip />
      <DailyChallengeCard />

      <View className="mb-4 flex-row flex-wrap gap-2">
        {DIFFICULTY_FILTERS.map((filter) => (
          <Chip
            key={filter}
            label={difficultyFilterLabel(filter)}
            selected={selectedDifficulty === filter}
            onPress={() => setSelectedDifficulty(filter)}
          />
        ))}
      </View>

      {categoryOptions.length > 0 ? (
        <View className="mb-4 flex-row flex-wrap gap-2">
          <Chip
            label="All categories"
            selected={selectedCategory === ALL_CATEGORIES}
            onPress={() => setSelectedCategory(ALL_CATEGORIES)}
          />
          {categoryOptions.map((category) => (
            <Chip
              key={category}
              label={category}
              selected={selectedCategory === category}
              onPress={() => setSelectedCategory(category)}
            />
          ))}
        </View>
      ) : null}

      <Card className="mb-4">
        <ListItem
          leading={<Icon name="briefcase-outline" size={20} color={colors.textSecondary} />}
          title="Interview questions only"
          subtitle="Show only classic technical-interview staples"
          trailing={
            <Switch
              value={interviewOnly}
              onValueChange={setInterviewOnly}
              accessibilityLabel="Toggle interview questions only"
            />
          }
        />
      </Card>

      {problemsQuery.isLoading ? (
        <ProblemListSkeleton />
      ) : problemsQuery.isError ? (
        <ErrorState
          title="Couldn't load problems"
          description="Check your connection and try again."
          onRetry={() => void problemsQuery.refetch()}
        />
      ) : problems.length === 0 ? (
        <EmptyState
          icon="code-slash-outline"
          title="No problems match these filters"
          description="Try a different difficulty or category."
        />
      ) : (
        <Card>
          {problems.map((problem, index) => (
            <React.Fragment key={problem.id}>
              {index > 0 ? <Divider /> : null}
              <ProblemRow problem={problem} onPress={openProblem} />
            </React.Fragment>
          ))}
        </Card>
      )}
    </ScreenContainer>
  );
}

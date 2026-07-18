import React from "react";
import { Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Card } from "../../../src/components/Card";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useWeeklyDigestQuery } from "../../../src/hooks/useCurrentAffairs";

function DigestSkeleton() {
  return (
    <Card>
      <Skeleton variant="text" width="40%" className="mb-4" />
      <Skeleton variant="text" width="100%" className="mb-2" />
      <Skeleton variant="text" width="100%" className="mb-2" />
      <Skeleton variant="text" width="80%" />
    </Card>
  );
}

function formatWeekRange(isoDate: string): string {
  const start = new Date(isoDate);
  const end = new Date(start);
  end.setDate(end.getDate() + 6);
  const formatter = new Intl.DateTimeFormat("en-US", { month: "long", day: "numeric" });
  return `${formatter.format(start)} – ${formatter.format(end)}`;
}

/** Full weekly-digest screen, reached by tapping the teaser card on the news feed. */
export default function WeeklyDigestScreen() {
  const { colors } = useTheme();
  const digestQuery = useWeeklyDigestQuery();
  const digest = digestQuery.data;

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
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Weekly Digest</Text>
      </View>

      {digestQuery.isLoading ? (
        <DigestSkeleton />
      ) : digestQuery.isError ? (
        <ErrorState
          title="Couldn't load this week's digest"
          description="Check your connection and try again."
          onRetry={() => void digestQuery.refetch()}
        />
      ) : !digest ? (
        <EmptyState
          icon="calendar-outline"
          title="Not enough news yet"
          description="Check back once there's more coverage this week — your AI-written digest will appear here."
        />
      ) : (
        <Card>
          <View className="mb-3 flex-row items-center">
            <Icon name="sparkles" size={18} color={colors.brand} />
            <Text className="ml-2 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
              Week of {formatWeekRange(digest.weekStartDateUtc)}
            </Text>
          </View>
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{digest.summaryText}</Text>
        </Card>
      )}
    </ScreenContainer>
  );
}

import React, { useState } from "react";
import { Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { ListItem } from "../../../src/components/ListItem";
import { Skeleton } from "../../../src/components/Skeleton";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { formatRelativeTime } from "../../../src/lib/relativeTime";
import { formatPercentileRank } from "../../../src/lib/percentile";
import { stashStartedMockTestAttempt } from "../../../src/lib/mockTestAttemptCache";
import type { MockTestTemplateDto } from "../../../src/api/mocktests";
import {
  useMockTestAttemptsQuery,
  useMockTestTemplatesQuery,
  useStartMockTestAttemptMutation,
} from "../../../src/hooks/useMockTests";

function TemplatesSkeleton() {
  return (
    <View className="gap-3">
      {[0, 1, 2].map((i) => (
        <Card key={i}>
          <Skeleton variant="text" width="55%" className="mb-2" />
          <Skeleton variant="text" width="80%" className="mb-3" />
          <Skeleton variant="text" width="40%" />
        </Card>
      ))}
    </View>
  );
}

function HistorySkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="px-3 py-3">
          <Skeleton variant="text" width="50%" className="mb-2" />
          <Skeleton variant="text" width="65%" />
        </View>
      ))}
    </Card>
  );
}

/**
 * "Mock Tests" entry point: every available test template (tap to start a
 * timed attempt) plus a history of past attempts (tap one to view its
 * results). Unlike Rapid Fire Quiz's category/difficulty picker, starting an
 * attempt here needs no configuration — a template already fully specifies
 * its own question count and duration.
 */
export default function MockTestsScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [startingTemplateId, setStartingTemplateId] = useState<string | null>(null);

  const templatesQuery = useMockTestTemplatesQuery();
  const attemptsQuery = useMockTestAttemptsQuery();
  const startAttemptMutation = useStartMockTestAttemptMutation();

  function startAttempt(template: MockTestTemplateDto) {
    if (startAttemptMutation.isPending) return;
    setStartingTemplateId(template.id);
    startAttemptMutation.mutate(
      { templateId: template.id },
      {
        onSuccess: (result) => {
          stashStartedMockTestAttempt(result.attemptId, {
            templateTitle: template.title,
            durationMinutes: result.durationMinutes,
            startedAtUtc: result.startedAtUtc,
            questions: result.questions,
          });
          setStartingTemplateId(null);
          router.push(`/(app)/mocktests/${result.attemptId}`);
        },
        onError: () => {
          setStartingTemplateId(null);
          show("Couldn't start that test. Please try again.", "danger");
        },
      },
    );
  }

  const templates = templatesQuery.data ?? [];
  const attempts = attemptsQuery.data ?? [];

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Mock Tests</Text>

      <View className="mb-6">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Available tests</Text>
        {templatesQuery.isLoading ? (
          <TemplatesSkeleton />
        ) : templatesQuery.isError ? (
          <ErrorState
            title="Couldn't load mock tests"
            description="Check your connection and try again."
            onRetry={() => void templatesQuery.refetch()}
          />
        ) : templates.length === 0 ? (
          <EmptyState icon="document-text-outline" title="No mock tests available yet" />
        ) : (
          <View className="gap-3">
            {templates.map((template) => (
              <Card key={template.id}>
                <View className="mb-2 flex-row items-start justify-between">
                  <Text className="mr-3 flex-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
                    {template.title}
                  </Text>
                  <Badge label={template.category} variant="brand" />
                </View>
                {template.description ? (
                  <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                    {template.description}
                  </Text>
                ) : null}
                <View className="mb-3 flex-row items-center gap-4">
                  <View className="flex-row items-center">
                    <Icon name="list-outline" size={16} color={colors.textSecondary} />
                    <Text className="ml-1.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                      {template.questionCount} question{template.questionCount === 1 ? "" : "s"}
                    </Text>
                  </View>
                  <View className="flex-row items-center">
                    <Icon name="time-outline" size={16} color={colors.textSecondary} />
                    <Text className="ml-1.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                      {template.durationMinutes} min
                    </Text>
                  </View>
                </View>
                <Button
                  title="Start test"
                  loading={startingTemplateId === template.id}
                  disabled={startAttemptMutation.isPending && startingTemplateId !== template.id}
                  onPress={() => startAttempt(template)}
                />
              </Card>
            ))}
          </View>
        )}
      </View>

      <View>
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Past attempts</Text>
        {attemptsQuery.isLoading ? (
          <HistorySkeleton />
        ) : attemptsQuery.isError ? (
          <ErrorState
            title="Couldn't load your past attempts"
            description="Check your connection and try again."
            onRetry={() => void attemptsQuery.refetch()}
          />
        ) : attempts.length === 0 ? (
          <EmptyState
            icon="school-outline"
            title="No attempts yet"
            description="Start a mock test above to see your results and progress here."
          />
        ) : (
          <Card>
            {attempts.map((attempt, index) => (
              <React.Fragment key={attempt.attemptId}>
                {index > 0 ? <Divider /> : null}
                <ListItem
                  leading={<Icon name="document-text-outline" size={22} color={colors.brand} />}
                  title={attempt.templateTitle}
                  subtitle={`${attempt.score} pts · ${formatPercentileRank(attempt.percentileRank)} · ${formatRelativeTime(attempt.submittedAtUtc)}`}
                  trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
                  onPress={() => router.push(`/(app)/mocktests/${attempt.attemptId}/results`)}
                />
              </React.Fragment>
            ))}
          </Card>
        )}
      </View>
    </ScreenContainer>
  );
}

import React from "react";
import { Alert, Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Skeleton } from "../../../src/components/Skeleton";
import { StudyPlanSummaryCard } from "../../../src/components/studyplanner/StudyPlanSummaryCard";
import { StudyTaskRow } from "../../../src/components/studyplanner/StudyTaskRow";
import { useToast } from "../../../src/lib/toast";
import {
  useActivePlanQuery,
  useArchivePlanMutation,
  useCompleteTaskMutation,
  useTodayTasksQuery,
} from "../../../src/hooks/useStudyPlanner";

function SummarySkeleton() {
  return (
    <Card className="mb-6">
      <Skeleton variant="text" width="55%" className="mb-2" />
      <Skeleton variant="text" width="40%" className="mb-4" />
      <Skeleton variant="rect" height={8} className="mb-2" />
      <Skeleton variant="text" width="50%" />
    </Card>
  );
}

function TasksSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="px-3 py-3">
          <Skeleton variant="text" width="55%" className="mb-2" />
          <Skeleton variant="text" width="35%" />
        </View>
      ))}
    </Card>
  );
}

/**
 * Study Planner overview / "Today" screen — the entry point reached from
 * the "Study Planner" card on the dashboard. Shows an `EmptyState` prompting
 * plan creation when the signed-in user has no active plan; otherwise the
 * plan's summary (days remaining, overall progress, missed-task callout)
 * plus today's scheduled tasks, each with a tap-to-complete checkbox
 * affordance, and a link through to the weekly view.
 */
export default function StudyPlannerOverviewScreen() {
  const { show } = useToast();

  const activePlanQuery = useActivePlanQuery();
  const plan = activePlanQuery.data ?? null;

  const todayTasksQuery = useTodayTasksQuery(!!plan);
  const completeTaskMutation = useCompleteTaskMutation();
  const archivePlanMutation = useArchivePlanMutation();

  function handleComplete(taskId: string) {
    if (completeTaskMutation.isPending) return;
    completeTaskMutation.mutate(taskId, {
      onError: () => show("Couldn't mark that task complete. Please try again.", "danger"),
    });
  }

  function confirmArchive() {
    if (!plan || archivePlanMutation.isPending) return;
    Alert.alert(
      "End this plan",
      "This archives your current study plan. You can create a new one afterward.",
      [
        { text: "Cancel", style: "cancel" },
        {
          text: "End plan",
          style: "destructive",
          onPress: () =>
            archivePlanMutation.mutate(plan.planId, {
              onError: () => show("Couldn't end that plan. Please try again.", "danger"),
            }),
        },
      ],
    );
  }

  if (activePlanQuery.isLoading) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Study Planner</Text>
        <SummarySkeleton />
        <TasksSkeleton />
      </ScreenContainer>
    );
  }

  if (activePlanQuery.isError) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Study Planner</Text>
        <ErrorState
          title="Couldn't load your study plan"
          description="Check your connection and try again."
          onRetry={() => void activePlanQuery.refetch()}
        />
      </ScreenContainer>
    );
  }

  if (!plan) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Study Planner</Text>
        <EmptyState
          icon="calendar-outline"
          title="No study plan yet"
          description="Tell us your exam date and subjects, and we'll build an AI day-by-day schedule to get you ready."
          actionLabel="Create a study plan"
          onAction={() => router.push("/(app)/studyplanner/setup")}
        />
      </ScreenContainer>
    );
  }

  const tasks = todayTasksQuery.data ?? [];

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-start justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Study Planner</Text>
        <Pressable
          onPress={confirmArchive}
          disabled={archivePlanMutation.isPending}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="End plan"
        >
          <Text className="text-caption font-medium text-danger">End plan</Text>
        </Pressable>
      </View>

      <StudyPlanSummaryCard plan={plan} className="mb-6" />

      <View className="mb-3 flex-row items-center justify-between">
        <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">Today&apos;s tasks</Text>
        <Button
          title="View week"
          variant="ghost"
          fullWidth={false}
          onPress={() => router.push("/(app)/studyplanner/week")}
        />
      </View>

      {todayTasksQuery.isLoading ? (
        <TasksSkeleton />
      ) : todayTasksQuery.isError ? (
        <ErrorState
          title="Couldn't load today's tasks"
          description="Check your connection and try again."
          onRetry={() => void todayTasksQuery.refetch()}
        />
      ) : tasks.length === 0 ? (
        <Card>
          <EmptyState
            icon="checkmark-done-outline"
            title="Nothing scheduled today"
            description="Enjoy the break, or check the weekly view for what's coming up."
          />
        </Card>
      ) : (
        <Card>
          {tasks.map((task, index) => (
            <React.Fragment key={task.id}>
              {index > 0 ? <Divider /> : null}
              <StudyTaskRow
                task={task}
                onComplete={handleComplete}
                isCompleting={completeTaskMutation.isPending && completeTaskMutation.variables === task.id}
              />
            </React.Fragment>
          ))}
        </Card>
      )}
    </ScreenContainer>
  );
}

import React, { useMemo, useState } from "react";
import { Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { StudyTaskRow } from "../../../src/components/studyplanner/StudyTaskRow";
import { useTheme } from "../../../src/theme/ThemeProvider";
import {
  formatDayHeading,
  formatWeekRangeLabel,
  isToday,
  shiftYmd,
  todayYmd,
  weekDates,
} from "../../../src/lib/studyPlannerDates";
import { useActivePlanQuery, useWeeklyTasksQuery } from "../../../src/hooks/useStudyPlanner";
import type { StudyTaskDto } from "../../../src/api/studyplanner";

const WEEK_LENGTH_DAYS = 7;

function WeekSkeleton() {
  return (
    <View className="gap-3">
      {[0, 1, 2].map((i) => (
        <Card key={i}>
          <Skeleton variant="text" width="40%" className="mb-3" />
          <Skeleton variant="text" width="70%" className="mb-2" />
          <Skeleton variant="text" width="55%" />
        </Card>
      ))}
    </View>
  );
}

/**
 * Weekly view: the 7-day window starting `weekStartDate`, grouped into one
 * `Card` per day, with Previous/Next controls shifting the window a full
 * week at a time. Read-only by design — task completion lives on the
 * "Today" overview screen; `StudyTaskRow` is given no `onComplete` here, so
 * it renders a plain status `Badge` per task instead of a checkbox.
 */
export default function StudyPlannerWeekScreen() {
  const { colors } = useTheme();
  const [weekStartDate, setWeekStartDate] = useState(todayYmd());

  const activePlanQuery = useActivePlanQuery();
  const hasPlan = !!activePlanQuery.data;
  const weekQuery = useWeeklyTasksQuery(weekStartDate, hasPlan);

  const tasksByDate = useMemo(() => {
    const map = new Map<string, StudyTaskDto[]>();
    for (const task of weekQuery.data ?? []) {
      const dateKey = task.scheduledDateUtc.slice(0, 10);
      const existing = map.get(dateKey);
      if (existing) existing.push(task);
      else map.set(dateKey, [task]);
    }
    return map;
  }, [weekQuery.data]);

  if (activePlanQuery.isLoading) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">This week</Text>
        <WeekSkeleton />
      </ScreenContainer>
    );
  }

  if (activePlanQuery.isError) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">This week</Text>
        <ErrorState
          title="Couldn't load your study plan"
          description="Check your connection and try again."
          onRetry={() => void activePlanQuery.refetch()}
        />
      </ScreenContainer>
    );
  }

  if (!hasPlan) {
    return (
      <ScreenContainer>
        <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">This week</Text>
        <EmptyState
          icon="calendar-outline"
          title="No study plan yet"
          description="Create a study plan to see your weekly schedule here."
          actionLabel="Create a study plan"
          onAction={() => router.push("/(app)/studyplanner/setup")}
        />
      </ScreenContainer>
    );
  }

  const days = weekDates(weekStartDate);

  return (
    <ScreenContainer>
      <Text className="mb-1 text-heading text-ink-primary dark:text-ink-primary-dark">This week</Text>
      <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        {formatWeekRangeLabel(weekStartDate)}
      </Text>

      <View className="mb-6 flex-row items-center gap-3">
        <View className="flex-1">
          <Button
            title="Previous"
            variant="secondary"
            onPress={() => setWeekStartDate((prev) => shiftYmd(prev, -WEEK_LENGTH_DAYS))}
          />
        </View>
        <View className="flex-1">
          <Button
            title="Next"
            variant="secondary"
            onPress={() => setWeekStartDate((prev) => shiftYmd(prev, WEEK_LENGTH_DAYS))}
          />
        </View>
      </View>

      {weekQuery.isLoading ? (
        <WeekSkeleton />
      ) : weekQuery.isError ? (
        <ErrorState
          title="Couldn't load this week's tasks"
          description="Check your connection and try again."
          onRetry={() => void weekQuery.refetch()}
        />
      ) : (
        <View className="gap-3">
          {days.map((date) => {
            const dayTasks = tasksByDate.get(date) ?? [];
            return (
              <Card key={date}>
                <View className="mb-2 flex-row items-center gap-2">
                  {isToday(date) ? <Icon name="today-outline" size={16} color={colors.brand} /> : null}
                  <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                    {isToday(date) ? `Today · ${formatDayHeading(date)}` : formatDayHeading(date)}
                  </Text>
                </View>
                {dayTasks.length === 0 ? (
                  <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                    Nothing scheduled.
                  </Text>
                ) : (
                  dayTasks.map((task, index) => (
                    <React.Fragment key={task.id}>
                      {index > 0 ? <Divider /> : null}
                      <StudyTaskRow task={task} />
                    </React.Fragment>
                  ))
                )}
              </Card>
            );
          })}
        </View>
      )}
    </ScreenContainer>
  );
}

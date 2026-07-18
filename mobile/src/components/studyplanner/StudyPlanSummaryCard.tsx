import React from "react";
import { Text, View } from "react-native";

import { Badge } from "../Badge";
import { Card } from "../Card";
import { ProgressBar } from "../ProgressBar";
import { formatLongDate } from "../../lib/studyPlannerDates";
import type { ActiveStudyPlanDto } from "../../api/studyplanner";

export interface StudyPlanSummaryCardProps {
  plan: ActiveStudyPlanDto;
  className?: string;
}

/**
 * Days-remaining / overall-progress summary for the active plan, shown at
 * the top of the plan overview screen (and mirrored by the dashboard's
 * "Study Planner" entry point in a condensed form). Calls out missed tasks
 * separately whenever there are any, rather than letting them blend into
 * the overall progress bar.
 */
export function StudyPlanSummaryCard({ plan, className = "" }: StudyPlanSummaryCardProps) {
  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-start justify-between">
        <View className="flex-1 pr-3">
          <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">
            {plan.daysRemaining} {plan.daysRemaining === 1 ? "day" : "days"} until exam
          </Text>
          <Text className="mt-0.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Exam day: {formatLongDate(plan.examDate)}
          </Text>
        </View>
        {plan.missedTasks > 0 ? (
          <Badge label={`${plan.missedTasks} missed`} variant="danger" />
        ) : null}
      </View>

      <ProgressBar
        value={plan.progressPercent / 100}
        accessibilityLabel="Overall plan progress"
        className="mb-2"
      />
      <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
        {plan.completedTasks} of {plan.totalTasks} tasks completed ({Math.round(plan.progressPercent)}%)
      </Text>
    </Card>
  );
}

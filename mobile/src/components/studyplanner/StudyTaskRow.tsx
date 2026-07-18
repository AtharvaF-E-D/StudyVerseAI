import React from "react";
import { ActivityIndicator, Pressable, View } from "react-native";

import { Badge } from "../Badge";
import { Icon } from "../Icon";
import { ListItem } from "../ListItem";
import { useTheme } from "../../theme/ThemeProvider";
import { STUDY_TASK_STATUS_BADGE_VARIANTS, STUDY_TASK_STATUS_LABELS } from "../../lib/studyTaskStatus";
import type { StudyTaskDto } from "../../api/studyplanner";

export interface StudyTaskRowProps {
  task: StudyTaskDto;
  /**
   * Omit for a read-only row (the weekly view): every status renders as a
   * plain `Badge` and no complete affordance is shown. Pass it (the plan
   * overview/"Today's tasks" screen) to render a tappable checkbox-style
   * icon for `"pending"` tasks instead.
   */
  onComplete?: (taskId: string) => void;
  isCompleting?: boolean;
}

/**
 * One scheduled study task row, shared by the plan overview ("Today's
 * tasks") and weekly view screens.
 *
 * `ListItem` is deliberately given no `onPress` here — there's no task
 * detail screen to navigate to, so the row itself is never a button. That's
 * what makes the trailing checkbox `Pressable` below safe: `ListItem`
 * renders as a plain `View` (not a `Pressable`) whenever `onPress` is unset,
 * so nesting a `Pressable` inside its `trailing` slot does NOT reproduce the
 * nested-`<button>` bug documented in `app/(app)/notes/index.tsx` — that bug
 * only happens when `ListItem.onPress` is ALSO set, which it never is here.
 */
export function StudyTaskRow({ task, onComplete, isCompleting = false }: StudyTaskRowProps) {
  const { colors } = useTheme();

  const interactive = !!onComplete;
  const canComplete = interactive && task.status === "pending";
  const showCheckbox = interactive && (task.status === "pending" || task.status === "completed");
  const showStatusBadge = !interactive || task.status !== "pending";

  return (
    <ListItem
      leading={<Icon name="book-outline" size={22} color={task.isWeakTopic ? colors.warning : colors.brand} />}
      title={task.topic}
      subtitle={`${task.subject} · ${task.durationMinutes} min`}
      trailing={
        <View className="flex-row items-center gap-2">
          {task.isWeakTopic ? <Badge label="Weak topic" variant="warning" /> : null}
          {showStatusBadge ? (
            <Badge
              label={STUDY_TASK_STATUS_LABELS[task.status]}
              variant={STUDY_TASK_STATUS_BADGE_VARIANTS[task.status]}
            />
          ) : null}
          {showCheckbox ? (
            isCompleting ? (
              <ActivityIndicator size="small" color={colors.brand} />
            ) : (
              <Pressable
                onPress={canComplete ? () => onComplete?.(task.id) : undefined}
                disabled={!canComplete}
                hitSlop={8}
                accessibilityRole="button"
                accessibilityLabel={canComplete ? `Mark ${task.topic} complete` : "Completed"}
              >
                <Icon
                  name={task.status === "completed" ? "checkmark-circle" : "ellipse-outline"}
                  size={24}
                  color={task.status === "completed" ? colors.success : colors.textSecondary}
                />
              </Pressable>
            )
          ) : null}
        </View>
      }
    />
  );
}

import type { BadgeVariant } from "../components/Badge";
import type { StudyTaskStatus } from "../api/studyplanner";

/** Shared status → display label / `Badge` variant mapping, used by the plan overview (today) and weekly view screens. */
export const STUDY_TASK_STATUS_LABELS: Record<StudyTaskStatus, string> = {
  pending: "Pending",
  completed: "Completed",
  missed: "Missed",
  rescheduled: "Rescheduled",
};

export const STUDY_TASK_STATUS_BADGE_VARIANTS: Record<StudyTaskStatus, BadgeVariant> = {
  pending: "neutral",
  completed: "success",
  missed: "danger",
  rescheduled: "warning",
};

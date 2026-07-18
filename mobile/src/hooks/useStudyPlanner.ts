import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  archiveStudyPlan,
  completeTask,
  createStudyPlan,
  getActiveStudyPlan,
  getTodayTasks,
  getWeeklyTasks,
  type ActiveStudyPlanDto,
  type CreateStudyPlanRequest,
  type CreateStudyPlanResponse,
  type StudyTaskDto,
} from "../api/studyplanner";

export const activePlanQueryKey = ["studyplanner", "plans", "active"] as const;
export const todayTasksQueryKey = ["studyplanner", "tasks", "today"] as const;

export function weeklyTasksQueryKey(weekStartDate: string) {
  return ["studyplanner", "tasks", "week", weekStartDate] as const;
}

/** Fetches the signed-in user's active study plan (`data` is `null`, not an error, when they haven't created one yet). */
export function useActivePlanQuery() {
  return useQuery<ActiveStudyPlanDto | null>({
    queryKey: activePlanQueryKey,
    queryFn: getActiveStudyPlan,
  });
}

/** Fetches today's scheduled tasks for the plan overview screen. */
export function useTodayTasksQuery(enabled = true) {
  return useQuery<StudyTaskDto[]>({
    queryKey: todayTasksQueryKey,
    queryFn: getTodayTasks,
    enabled,
  });
}

/** Fetches the 7-day window of tasks starting `weekStartDate` ("yyyy-MM-dd"), for the weekly view. */
export function useWeeklyTasksQuery(weekStartDate: string, enabled = true) {
  return useQuery<StudyTaskDto[]>({
    queryKey: weeklyTasksQueryKey(weekStartDate),
    queryFn: () => getWeeklyTasks(weekStartDate),
    enabled,
  });
}

/**
 * Kicks off AI generation of a brand-new study plan. On success, invalidates
 * the active-plan snapshot and every today/week task query so the overview
 * and weekly screens pick up the freshly generated schedule immediately.
 */
export function useCreateStudyPlanMutation() {
  const queryClient = useQueryClient();

  return useMutation<CreateStudyPlanResponse, unknown, CreateStudyPlanRequest>({
    mutationFn: (request) => createStudyPlan(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: activePlanQueryKey });
      void queryClient.invalidateQueries({ queryKey: todayTasksQueryKey });
      void queryClient.invalidateQueries({ queryKey: ["studyplanner", "tasks", "week"] });
    },
  });
}

/**
 * Marks a task complete, then invalidates the active-plan snapshot (progress/
 * completed counts changed) and every today/week task query so the task's
 * status flips everywhere it's shown.
 */
export function useCompleteTaskMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (taskId) => completeTask(taskId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: activePlanQueryKey });
      void queryClient.invalidateQueries({ queryKey: todayTasksQueryKey });
      void queryClient.invalidateQueries({ queryKey: ["studyplanner", "tasks", "week"] });
    },
  });
}

/** Archives the active plan, then invalidates the active-plan query so the overview screen falls back to its "create a plan" empty state. */
export function useArchivePlanMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (planId) => archiveStudyPlan(planId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: activePlanQueryKey });
    },
  });
}

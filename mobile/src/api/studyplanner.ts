import { isAxiosError } from "axios";

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Study Planner" backend contract (base path
// `api/v1/studyplanner`), built in parallel with this mobile work.
// `localhost:5221` still refuses connections as of this writing, so this was
// verified visually against a dev-only fixture route rather than live HTTP
// calls (same situation `src/api/mocktests.ts` was in) — but partway through
// this pass, the backend agent's Application-layer code (CQRS
// commands/queries/handlers under
// `backend/src/StudyVerse.Application/Features/StudyPlanner/`) landed in the
// repo, still with no Api-layer controller wiring it to HTTP yet and no
// `CompleteTask`/`ArchivePlan` handler at all — consistent with the server
// still refusing connections. Since the C# was readable even without a
// running server, every DTO below was cross-checked directly against it
// (mirroring the exact mismatch this project shipped once before — see
// `MockTestAttemptListItemDto.attemptId` in `src/api/mocktests.ts`):
// `StudyPlanDtos.cs`'s `ActiveStudyPlanDto`/`StudyPlanTaskDto` field names
// and casing all matched this file's shape exactly, EXCEPT:
//   - `StudyPlanTaskDto.ScheduledDateUtc` is a C# `DateOnly`, not a
//     timestamp — it serializes as a plain "yyyy-MM-dd" (no time-of-day, no
//     "Z"), not the full ISO instant this file originally assumed. Fixed
//     below.
//   - The real DTO also carries `OriginalScheduledDateUtc` (nullable
//     `DateOnly`), which isn't in the mobile spec this was built against at
//     all. Per `StudyPlanTask.cs`'s doc comment and
//     `StudyPlanTaskStatus.cs`'s enum comment, the automatic missed-task
//     recovery pass never actually sets a task's status to `"rescheduled"`
//     (that value is reserved for a future manual drag-to-reschedule
//     feature) — instead it marks the overdue row `"missed"` (terminal) and
//     creates a brand-new `"pending"` row for the make-up session, with
//     `originalScheduledDateUtc` set to the date it was first due. That
//     field is exactly what a "moved from missed to a new date" reschedule
//     visualization would need — which is the lower-priority piece this
//     phase's spec explicitly allowed cutting under time pressure. Carried
//     here as optional/unused so the type stays accurate for whoever builds
//     that visualization next, but no UI reads it yet.
// Status enum casing was also confirmed, not just assumed: `Program.cs`
// registers a global `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`,
// so `StudyPlanTaskStatus.Pending` etc. really do serialize as `"pending"`
// etc. — matching `StudyTaskStatus` below. And `GetActivePlanQueryHandler`/
// `GetTodayTasksQueryHandler` return `Result.Failure(..., ResultErrorType.NotFound)`
// when there's no active plan, which `ApiControllerBase.MapFailure` maps to
// a real HTTP 404 — confirming `getActiveStudyPlan`'s catch-404-return-null
// handling below is correct, not just a guess at the contract's "a
// 404-style response" wording.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// POST /studyplanner/plans  →  CreateStudyPlanResponse
// ---------------------------------------------------------------------------

export interface CreateStudyPlanRequest {
  /** "yyyy-MM-dd" */
  examDate: string;
  subjects: string[];
  weakTopics: string[];
  hoursPerDayMinutes: number;
}

export interface CreateStudyPlanResponse {
  planId: string;
  examDate: string;
  totalTasks: number;
}

/**
 * Kicks off real AI generation of a full study plan. The contract's "this
 * can take several seconds" undersold it badly: live-testing this against
 * the real backend (once it came up mid-pass) measured ~71s for one call,
 * and the server logs showed the request getting cancelled at almost
 * exactly 15s — `coreApiClient`'s shared default `timeout` — on an earlier
 * attempt. ASP.NET Core propagates a client disconnect to
 * `HttpContext.RequestAborted`, which cancels the in-flight OpenAI call
 * too, so that 15s default doesn't just show a spurious error while the
 * plan quietly finishes server-side — it kills plan creation outright, with
 * nothing persisted. Overriding the timeout for just this one call (rather
 * than raising `coreApiClient`'s shared default, which every other
 * feature's fast reads/writes benefit from staying short) is the narrowest
 * fix. 120s leaves real margin above the observed ~71s.
 */
export async function createStudyPlan(request: CreateStudyPlanRequest): Promise<CreateStudyPlanResponse> {
  const { data } = await coreApiClient.post<CreateStudyPlanResponse>("/studyplanner/plans", request, {
    timeout: 120_000,
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /studyplanner/plans/active  →  ActiveStudyPlanDto | null
// ---------------------------------------------------------------------------

export interface ActiveStudyPlanDto {
  planId: string;
  /** "yyyy-MM-dd" */
  examDate: string;
  daysRemaining: number;
  subjects: string[];
  weakTopics: string[];
  hoursPerDayMinutes: number;
  totalTasks: number;
  completedTasks: number;
  missedTasks: number;
  /** 0-100, already a percentage (not a 0-1 fraction) — same convention as `QuizStatsResponse.accuracyPercent`. */
  progressPercent: number;
}

/**
 * Fetches the signed-in user's active study plan, or `null` if they don't
 * have one yet (the contract describes this as "a 404-style response" —
 * treated here as an expected, non-error outcome rather than surfacing an
 * `ErrorState` for what's really just "no plan created yet").
 */
export async function getActiveStudyPlan(): Promise<ActiveStudyPlanDto | null> {
  try {
    const { data } = await coreApiClient.get<ActiveStudyPlanDto>("/studyplanner/plans/active");
    return data;
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

// ---------------------------------------------------------------------------
// Shared task shape
// ---------------------------------------------------------------------------

export type StudyTaskStatus = "pending" | "completed" | "missed" | "rescheduled";

export interface StudyTaskDto {
  id: string;
  subject: string;
  topic: string;
  durationMinutes: number;
  isWeakTopic: boolean;
  status: StudyTaskStatus;
  /**
   * Plain "yyyy-MM-dd" (no time-of-day, no "Z") — confirmed against the real
   * backend's `StudyPlanTaskDto.ScheduledDateUtc`, which is a C# `DateOnly`
   * rather than a timestamp, despite the "Utc" in its name. Not a
   * `Date`-parseable instant on its own; treat it the same way this file's
   * other "yyyy-MM-dd" fields (`examDate`, `weekStartDate`) are treated.
   */
  scheduledDateUtc: string;
  completedAtUtc: string | null;
  /**
   * The date this task was ORIGINALLY due, present only on an automatic
   * missed-task-recovery make-up session (see this file's header comment).
   * Not in the mobile spec this client was built against; added after
   * reading the real backend's `StudyPlanTaskDto` so the type stays
   * accurate. Unused today — a "moved from missed to a new date" reschedule
   * visualization built on this was the piece cut for time in this pass.
   */
  originalScheduledDateUtc?: string | null;
}

// ---------------------------------------------------------------------------
// GET /studyplanner/tasks/today  →  StudyTaskDto[]
// ---------------------------------------------------------------------------

export async function getTodayTasks(): Promise<StudyTaskDto[]> {
  const { data } = await coreApiClient.get<StudyTaskDto[]>("/studyplanner/tasks/today");
  return data;
}

// ---------------------------------------------------------------------------
// GET /studyplanner/tasks/week?weekStartDate=yyyy-MM-dd  →  StudyTaskDto[]
// ---------------------------------------------------------------------------

export async function getWeeklyTasks(weekStartDate: string): Promise<StudyTaskDto[]> {
  const { data } = await coreApiClient.get<StudyTaskDto[]>("/studyplanner/tasks/week", {
    params: { weekStartDate },
  });
  return data;
}

// ---------------------------------------------------------------------------
// POST /studyplanner/tasks/{id}/complete  →  200 (task echoed back or just success)
// ---------------------------------------------------------------------------

/**
 * Marks a task complete. The contract doesn't commit to a specific response
 * body ("task echoed back or just success"), so this is treated generically
 * — callers rely on invalidating the today/week/active-plan queries
 * afterward rather than reading anything off this call's return value.
 */
export async function completeTask(taskId: string): Promise<void> {
  await coreApiClient.post(`/studyplanner/tasks/${taskId}/complete`);
}

// ---------------------------------------------------------------------------
// POST /studyplanner/plans/{id}/archive  →  204
// ---------------------------------------------------------------------------

export async function archiveStudyPlan(planId: string): Promise<void> {
  await coreApiClient.post(`/studyplanner/plans/${planId}/archive`);
}

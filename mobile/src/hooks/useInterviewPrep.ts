import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  completeInterviewSession,
  createInterviewSession,
  getInterviewCategories,
  getInterviewSession,
  getInterviewSessions,
  getInterviewStats,
  getResumeHistory,
  submitInterviewAnswer,
  uploadResume,
  type CompleteInterviewSessionResponse,
  type CreateInterviewSessionRequest,
  type CreateInterviewSessionResponse,
  type InterviewCategory,
  type InterviewSessionDetailDto,
  type InterviewSessionHistoryItemDto,
  type InterviewStatsDto,
  type ResumeAnalysisDto,
  type SubmitInterviewAnswerRequest,
  type SubmitInterviewAnswerResponse,
  type UploadResumeFile,
} from "../api/interviewprep";

export const interviewCategoriesQueryKey = ["interview", "categories"] as const;
export const interviewStatsQueryKey = ["interview", "stats"] as const;
export const interviewSessionsQueryKey = ["interview", "sessions"] as const;
export const resumeHistoryQueryKey = ["interview", "resume", "history"] as const;

export function interviewSessionQueryKey(sessionId: string) {
  return ["interview", "sessions", sessionId] as const;
}

/** Fetches the set of interview categories the server currently offers. The picker screen falls back to the local `INTERVIEW_CATEGORIES` constant on error/empty, the same "don't leave the user with nothing" resilience `CodingProblemScreen` uses for its language list. */
export function useInterviewCategoriesQuery() {
  return useQuery<InterviewCategory[]>({
    queryKey: interviewCategoriesQueryKey,
    queryFn: getInterviewCategories,
  });
}

/** Fetches the signed-in user's aggregate interview-prep stats for the stats strip. */
export function useInterviewStatsQuery() {
  return useQuery<InterviewStatsDto>({
    queryKey: interviewStatsQueryKey,
    queryFn: getInterviewStats,
  });
}

/** Starts a new practice session and returns its full question set — see `src/lib/interviewSessionCache.ts` for how the play screen consumes this without an extra round trip. */
export function useCreateInterviewSessionMutation() {
  return useMutation<CreateInterviewSessionResponse, unknown, CreateInterviewSessionRequest>({
    mutationFn: (request) => createInterviewSession(request),
  });
}

/**
 * Fetches a session's full detail — questions, any already-graded answers,
 * and the overall score/plan once completed. Used by the practice screen
 * only as a cold-resume fallback when it mounts without the locally-cached
 * payload from `POST /interview/sessions` (see `src/lib/interviewSessionCache.ts`),
 * and also used directly by the history list to resume/review a past
 * session by id.
 */
export function useInterviewSessionQuery(sessionId: string, options?: { enabled?: boolean }) {
  return useQuery<InterviewSessionDetailDto>({
    queryKey: interviewSessionQueryKey(sessionId),
    queryFn: () => getInterviewSession(sessionId),
    enabled: (options?.enabled ?? true) && sessionId.length > 0,
  });
}

/**
 * Grades one answer via a real AI call. Deliberately has NO shared
 * `onSuccess` here that mutates a cache — the practice screen calls
 * `.mutate(vars, { onSuccess, onError })` itself so the graded score/feedback
 * lands in ITS OWN local per-question state (that's what actually drives the
 * inline reveal). Calling out explicitly why this matters: a submit mutation
 * with only an `onError` handler and no `onSuccess` wired to local state is
 * exactly the bug class found and fixed in the previous phase — every call
 * site below (and in the practice screen) is written to avoid it.
 */
export function useSubmitInterviewAnswerMutation(sessionId: string) {
  return useMutation<SubmitInterviewAnswerResponse, unknown, SubmitInterviewAnswerRequest>({
    mutationFn: (request) => submitInterviewAnswer(sessionId, request),
  });
}

/**
 * Finalizes a session with a real (slower) AI call. On success, invalidates
 * the session history and stats — both now have a newly-completed session
 * and a new overall score baked into the averages — so every screen reflects
 * the server's authoritative post-completion state. The practice screen
 * ALSO passes its own `onSuccess` at the call site to push the returned
 * `overallScore`/`improvementPlan` into local state so the finalized summary
 * actually renders (same reasoning as the answer mutation above).
 */
export function useCompleteInterviewSessionMutation(sessionId: string) {
  const queryClient = useQueryClient();

  return useMutation<CompleteInterviewSessionResponse, unknown, void>({
    mutationFn: () => completeInterviewSession(sessionId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: interviewSessionsQueryKey });
      void queryClient.invalidateQueries({ queryKey: interviewStatsQueryKey });
    },
  });
}

/** Fetches the signed-in user's past practice sessions for the history list. */
export function useInterviewSessionsQuery() {
  return useQuery<InterviewSessionHistoryItemDto[]>({
    queryKey: interviewSessionsQueryKey,
    queryFn: getInterviewSessions,
  });
}

/**
 * Uploads a resume for real AI analysis. On success, seeds the freshly
 * analyzed record straight into the resume-history list's cache (rather than
 * just invalidating and waiting on a refetch) so the very next screen — the
 * analysis result — can read it back instantly (see `src/api/interviewprep.ts`'s
 * header for why that list IS the detail source), and also invalidates stats
 * so the strip's `resumeAnalysesCount` reflects the new total.
 */
export function useUploadResumeMutation() {
  const queryClient = useQueryClient();

  return useMutation<ResumeAnalysisDto, unknown, UploadResumeFile>({
    mutationFn: (file) => uploadResume(file),
    onSuccess: (result) => {
      queryClient.setQueryData<ResumeAnalysisDto[]>(resumeHistoryQueryKey, (prev) =>
        prev ? [result, ...prev.filter((analysis) => analysis.id !== result.id)] : [result],
      );
      void queryClient.invalidateQueries({ queryKey: interviewStatsQueryKey });
    },
  });
}

/** Fetches every past resume analysis in full — also the source a specific analysis's detail screen reads from (see `src/api/interviewprep.ts`'s header). */
export function useResumeHistoryQuery() {
  return useQuery<ResumeAnalysisDto[]>({
    queryKey: resumeHistoryQueryKey,
    queryFn: getResumeHistory,
  });
}

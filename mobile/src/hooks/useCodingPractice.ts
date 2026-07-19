import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  getCodingStats,
  getDailyChallenge,
  getHint,
  getLanguages,
  getProblem,
  getProblems,
  getSubmissions,
  submitSolution,
  type CodingStatsDto,
  type DailyChallengeDto,
  type GetHintRequest,
  type GetProblemsParams,
  type HintResponse,
  type LanguageDto,
  type ProblemDetailDto,
  type ProblemSummaryDto,
  type SubmissionHistoryItemDto,
  type SubmitSolutionRequest,
  type SubmitSolutionResponse,
} from "../api/codingpractice";

// The list query key is nested under its own "list" segment (rather than
// just `["coding", "problems", params]`) specifically so
// `useSubmitSolutionMutation` can invalidate every cached list filter
// combination via the `["coding", "problems", "list"]` prefix WITHOUT that
// fuzzy prefix match also catching `problemDetailQueryKey` below (which
// shares the `["coding", "problems", ...]` prefix otherwise) — invalidating
// the detail query on submit would refetch `ProblemDetailDto` and, via the
// editor screen's "reset code when starter code changes" effect, wipe out
// whatever the user had just typed/submitted.
export function problemsQueryKey(params: GetProblemsParams) {
  return ["coding", "problems", "list", params] as const;
}

export const problemsListQueryKeyPrefix = ["coding", "problems", "list"] as const;

export function problemDetailQueryKey(problemId: string, languageId: number) {
  return ["coding", "problems", "detail", problemId, languageId] as const;
}

export const languagesQueryKey = ["coding", "languages"] as const;
export const dailyChallengeQueryKey = ["coding", "dailyChallenge"] as const;
export const codingStatsQueryKey = ["coding", "stats"] as const;

export function submissionsQueryKey(problemId?: string) {
  return ["coding", "submissions", problemId ?? "__all__"] as const;
}

/** Fetches the problem bank for the list screen, filtered by difficulty/category/interview-only. */
export function useProblemsQuery(params: GetProblemsParams) {
  return useQuery<ProblemSummaryDto[]>({
    queryKey: problemsQueryKey(params),
    queryFn: () => getProblems(params),
  });
}

/** Fetches one problem's full detail (description, sample tests, starter code) for the chosen language. */
export function useProblemQuery(problemId: string, languageId: number) {
  return useQuery<ProblemDetailDto>({
    queryKey: problemDetailQueryKey(problemId, languageId),
    queryFn: () => getProblem(problemId, languageId),
    enabled: problemId.length > 0 && languageId > 0,
  });
}

/** Fetches every language Judge0 (and this backend) supports, for the language picker. */
export function useLanguagesQuery() {
  return useQuery<LanguageDto[]>({
    queryKey: languagesQueryKey,
    queryFn: getLanguages,
  });
}

/** Fetches today's featured daily-challenge problem, or `null` if there isn't one right now. */
export function useDailyChallengeQuery() {
  return useQuery<DailyChallengeDto | null>({
    queryKey: dailyChallengeQueryKey,
    queryFn: getDailyChallenge,
  });
}

/** Fetches the signed-in user's aggregate coding stats (solved count, streak) for the stats strip. */
export function useCodingStatsQuery() {
  return useQuery<CodingStatsDto>({
    queryKey: codingStatsQueryKey,
    queryFn: getCodingStats,
  });
}

/** Fetches past submissions for one problem (or every problem, when omitted). */
export function useSubmissionsQuery(problemId?: string) {
  return useQuery<SubmissionHistoryItemDto[]>({
    queryKey: submissionsQueryKey(problemId),
    queryFn: () => getSubmissions(problemId),
  });
}

/**
 * Grades a real submission. On success, invalidates the problems LIST (every
 * cached filter combination — solved state affects the checkmark everywhere
 * a row for this problem could appear), this problem's submission history,
 * and the aggregate stats strip (solved count/streak), so every screen
 * reflects the server's authoritative post-submission state rather than
 * hand-patching each cache entry in place.
 *
 * Deliberately does NOT invalidate `problemDetailQueryKey` — `ProblemDetailDto`
 * has no `isSolved` field to go stale in the first place (only
 * `ProblemSummaryDto` does), and invalidating it would refetch the starter
 * code and, via the editor screen's "reset code when starter code changes"
 * effect, wipe out the very code the user just submitted.
 */
export function useSubmitSolutionMutation(problemId: string) {
  const queryClient = useQueryClient();

  return useMutation<SubmitSolutionResponse, unknown, SubmitSolutionRequest>({
    mutationFn: (request) => submitSolution(problemId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: problemsListQueryKeyPrefix });
      void queryClient.invalidateQueries({ queryKey: submissionsQueryKey(problemId) });
      void queryClient.invalidateQueries({ queryKey: submissionsQueryKey() });
      void queryClient.invalidateQueries({ queryKey: codingStatsQueryKey });
    },
  });
}

/** Requests a real AI hint for the user's current in-progress code. Not cached via `useQuery` — each tap is a fresh, deliberate request, same shape as the tutor's `sendMessage`. */
export function useHintMutation(problemId: string) {
  return useMutation<HintResponse, unknown, GetHintRequest>({
    mutationFn: (request) => getHint(problemId, request),
  });
}

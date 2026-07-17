import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  abandonQuizSession,
  activateExtraTimePowerUp,
  activateFiftyFiftyPowerUp,
  getDailyChallengeStatus,
  getQuizCategories,
  getQuizReview,
  getQuizSession,
  getQuizStats,
  startQuizSession,
  submitQuizAnswer,
  type DailyChallengeStatusDto,
  type ExtraTimeResponse,
  type FiftyFiftyResponse,
  type QuizCategoryDto,
  type QuizReviewResponse,
  type QuizSessionStateDto,
  type QuizStatsResponse,
  type StartQuizSessionRequest,
  type StartQuizSessionResponse,
  type SubmitQuizAnswerRequest,
  type SubmitQuizAnswerResponse,
} from "../api/quiz";

export const quizCategoriesQueryKey = ["quiz", "categories"] as const;
export const quizDailyChallengeQueryKey = ["quiz", "daily-challenge"] as const;
export const quizStatsQueryKey = ["quiz", "stats"] as const;

export function quizSessionQueryKey(sessionId: string) {
  return ["quiz", "sessions", sessionId] as const;
}

export function quizReviewQueryKey(sessionId: string) {
  return ["quiz", "sessions", sessionId, "review"] as const;
}

/** Fetches every quiz category with its per-difficulty question counts, for the category picker. */
export function useQuizCategoriesQuery() {
  return useQuery<QuizCategoryDto[]>({
    queryKey: quizCategoriesQueryKey,
    queryFn: getQuizCategories,
  });
}

/** Fetches today's daily-challenge category/difficulty and whether it's already been completed. */
export function useDailyChallengeStatusQuery() {
  return useQuery<DailyChallengeStatusDto>({
    queryKey: quizDailyChallengeQueryKey,
    queryFn: getDailyChallengeStatus,
  });
}

/** Fetches the signed-in user's aggregate quiz stats (total played, accuracy, best combo, per-category breakdown). */
export function useQuizStatsQuery() {
  return useQuery<QuizStatsResponse>({
    queryKey: quizStatsQueryKey,
    queryFn: getQuizStats,
  });
}

/** Starts a new quiz session (regular or daily-challenge) and returns its full question set. */
export function useStartQuizSessionMutation() {
  return useMutation<StartQuizSessionResponse, unknown, StartQuizSessionRequest>({
    mutationFn: (request) => startQuizSession(request),
  });
}

/**
 * Submits an answer for `sessionId`'s current question. On the answer that
 * completes the session, invalidates stats and the daily-challenge status
 * (which may have just flipped to `completedToday: true`) so both reflect
 * the server's authoritative result next time they're read.
 */
export function useSubmitAnswerMutation(sessionId: string) {
  const queryClient = useQueryClient();

  return useMutation<SubmitQuizAnswerResponse, unknown, SubmitQuizAnswerRequest>({
    mutationFn: (request) => submitQuizAnswer(sessionId, request),
    onSuccess: (result) => {
      if (result.isSessionComplete) {
        void queryClient.invalidateQueries({ queryKey: quizStatsQueryKey });
        void queryClient.invalidateQueries({ queryKey: quizDailyChallengeQueryKey });
      }
    },
  });
}

/** Uses the 50-50 power-up on the session's current question, returning the two option indexes to visually eliminate. */
export function useFiftyFiftyMutation(sessionId: string) {
  return useMutation<FiftyFiftyResponse, unknown, void>({
    mutationFn: () => activateFiftyFiftyPowerUp(sessionId),
  });
}

/** Uses the extra-time power-up on the session's current question. */
export function useExtraTimeMutation(sessionId: string) {
  return useMutation<ExtraTimeResponse, unknown, void>({
    mutationFn: () => activateExtraTimePowerUp(sessionId),
  });
}

/** Abandons an in-progress session (used by the play screen's pause-overlay "Quit" action). */
export function useAbandonSessionMutation(sessionId: string) {
  return useMutation<void, unknown, void>({
    mutationFn: () => abandonQuizSession(sessionId),
  });
}

/**
 * Fetches a session's current authoritative state (question index, lives,
 * combo, power-ups used, current question without its answer) — used by the
 * play screen only as a cold-resume fallback, when it mounts without the
 * locally-cached full question list from `POST /quiz/sessions` (see
 * `src/lib/quizSessionCache.ts`). Pass `enabled: false` once that local cache
 * hit so this never fires on the normal start -> play navigation path.
 */
export function useQuizSessionQuery(sessionId: string, options?: { enabled?: boolean }) {
  return useQuery<QuizSessionStateDto>({
    queryKey: quizSessionQueryKey(sessionId),
    queryFn: () => getQuizSession(sessionId),
    enabled: (options?.enabled ?? true) && sessionId.length > 0,
  });
}

/** Fetches a completed session's full review (every question, the user's answer, the correct answer, the explanation). */
export function useQuizReviewQuery(sessionId: string) {
  return useQuery<QuizReviewResponse>({
    queryKey: quizReviewQueryKey(sessionId),
    queryFn: () => getQuizReview(sessionId),
    enabled: sessionId.length > 0,
  });
}

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  getMockTestAttempt,
  getMockTestAttempts,
  getMockTestReview,
  getMockTestTemplates,
  startMockTestAttempt,
  submitMockTestAttempt,
  type MockTestAttemptListItemDto,
  type MockTestAttemptSummaryDto,
  type MockTestReviewItemDto,
  type MockTestTemplateDto,
  type StartMockTestAttemptRequest,
  type StartMockTestAttemptResponse,
  type SubmitMockTestAttemptRequest,
  type SubmitMockTestAttemptResponse,
} from "../api/mocktests";

export const mockTestTemplatesQueryKey = ["mocktests", "templates"] as const;
export const mockTestAttemptsQueryKey = ["mocktests", "attempts"] as const;

export function mockTestAttemptQueryKey(attemptId: string) {
  return ["mocktests", "attempts", attemptId] as const;
}

export function mockTestReviewQueryKey(attemptId: string) {
  return ["mocktests", "attempts", attemptId, "review"] as const;
}

/** Fetches every available mock test template, for the templates/history screen. */
export function useMockTestTemplatesQuery() {
  return useQuery<MockTestTemplateDto[]>({
    queryKey: mockTestTemplatesQueryKey,
    queryFn: getMockTestTemplates,
  });
}

/** Starts a new mock test attempt and returns its full question set (the only endpoint that does). */
export function useStartMockTestAttemptMutation() {
  return useMutation<StartMockTestAttemptResponse, unknown, StartMockTestAttemptRequest>({
    mutationFn: (request) => startMockTestAttempt(request),
  });
}

/**
 * Submits every locally-tracked answer for `attemptId` in one request. On
 * success, invalidates the attempts list and this attempt's own query so the
 * history screen and results screen both reflect the server's authoritative
 * score/percentile/AI analysis next time they're read.
 */
export function useSubmitMockTestAttemptMutation(attemptId: string) {
  const queryClient = useQueryClient();

  return useMutation<SubmitMockTestAttemptResponse, unknown, SubmitMockTestAttemptRequest>({
    mutationFn: (request) => submitMockTestAttempt(attemptId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: mockTestAttemptsQueryKey });
      void queryClient.invalidateQueries({ queryKey: mockTestAttemptQueryKey(attemptId) });
    },
  });
}

/** Fetches one attempt's post-submission summary (score, percentile, AI weakness analysis) for the results screen. */
export function useMockTestAttemptQuery(attemptId: string) {
  return useQuery<MockTestAttemptSummaryDto>({
    queryKey: mockTestAttemptQueryKey(attemptId),
    queryFn: () => getMockTestAttempt(attemptId),
    enabled: attemptId.length > 0,
  });
}

/** Fetches the signed-in user's past mock test attempts, for the history list. */
export function useMockTestAttemptsQuery() {
  return useQuery<MockTestAttemptListItemDto[]>({
    queryKey: mockTestAttemptsQueryKey,
    queryFn: getMockTestAttempts,
  });
}

/** Fetches a completed attempt's full review (every question, the user's answer, the correct answer, the explanation). */
export function useMockTestReviewQuery(attemptId: string) {
  return useQuery<MockTestReviewItemDto[]>({
    queryKey: mockTestReviewQueryKey(attemptId),
    queryFn: () => getMockTestReview(attemptId),
    enabled: attemptId.length > 0,
  });
}

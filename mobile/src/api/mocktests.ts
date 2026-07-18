import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Mock Tests" backend contract (base path
// `api/v1/mocktests`), built in parallel with this mobile work. The
// controller wasn't reachable over HTTP yet at the time this was written, so
// it's coded directly to the documented shape below and verified visually
// against `app/(dev)/mocktests-preview.tsx`'s fixtures instead (see that
// file's header) — re-verify against the real backend once it's up.
//
// One behavior worth flagging since it isn't obvious from field names alone:
// `POST /attempts` is the ONLY endpoint that returns the full question list
// for an attempt (mirrors the Rapid Fire Quiz contract's
// `POST /quiz/sessions` — see that file's header comment). `GET /attempts/{id}`
// instead returns a POST-SUBMISSION results summary (score/correctCount/
// percentileRank/aiWeaknessAnalysis), so it's only useful for the results
// screen, never for resuming an in-progress attempt. See
// `src/lib/mockTestAttemptCache.ts` for how the exam screen carries the
// question payload across the start -> exam navigation instead.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// GET /mocktests/templates  →  MockTestTemplateDto[]
// ---------------------------------------------------------------------------

export interface MockTestTemplateDto {
  id: string;
  title: string;
  description: string;
  category: string;
  questionCount: number;
  durationMinutes: number;
}

export async function getMockTestTemplates(): Promise<MockTestTemplateDto[]> {
  const { data } = await coreApiClient.get<MockTestTemplateDto[]>("/mocktests/templates");
  return data;
}

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

/** Mirrors `QuizQuestionDto` — never includes the correct answer. */
export interface MockTestQuestionDto {
  id: string;
  questionText: string;
  options: string[];
}

// ---------------------------------------------------------------------------
// POST /mocktests/attempts  →  StartMockTestAttemptResponse
// ---------------------------------------------------------------------------

export interface StartMockTestAttemptRequest {
  templateId: string;
}

export interface StartMockTestAttemptResponse {
  attemptId: string;
  durationMinutes: number;
  startedAtUtc: string;
  questions: MockTestQuestionDto[];
}

export async function startMockTestAttempt(
  request: StartMockTestAttemptRequest,
): Promise<StartMockTestAttemptResponse> {
  const { data } = await coreApiClient.post<StartMockTestAttemptResponse>("/mocktests/attempts", request);
  return data;
}

// ---------------------------------------------------------------------------
// POST /mocktests/attempts/{id}/submit  →  SubmitMockTestAttemptResponse
// ---------------------------------------------------------------------------

export interface SubmitMockTestAnswerDto {
  questionId: string;
  selectedOptionIndex: number;
}

export interface SubmitMockTestAttemptRequest {
  /**
   * Only the questions the test-taker actually answered — free navigation
   * between questions (and the "submit with unanswered questions" flow)
   * means this can be a strict subset of the attempt's full question list.
   */
  answers: SubmitMockTestAnswerDto[];
}

export interface SubmitMockTestAttemptResponse {
  score: number;
  correctCount: number;
  totalQuestions: number;
  /** 0-100, already a percentage (not a 0-1 fraction) — same convention as `QuizStatsResponse.accuracyPercent`. */
  percentileRank: number;
  aiWeaknessAnalysis: string;
}

export async function submitMockTestAttempt(
  attemptId: string,
  request: SubmitMockTestAttemptRequest,
): Promise<SubmitMockTestAttemptResponse> {
  const { data } = await coreApiClient.post<SubmitMockTestAttemptResponse>(
    `/mocktests/attempts/${attemptId}/submit`,
    request,
  );
  return data;
}

// ---------------------------------------------------------------------------
// GET /mocktests/attempts/{id}  →  MockTestAttemptSummaryDto
// ---------------------------------------------------------------------------

export interface MockTestAttemptSummaryDto extends SubmitMockTestAttemptResponse {
  templateTitle: string;
  submittedAtUtc: string;
}

export async function getMockTestAttempt(attemptId: string): Promise<MockTestAttemptSummaryDto> {
  const { data } = await coreApiClient.get<MockTestAttemptSummaryDto>(`/mocktests/attempts/${attemptId}`);
  return data;
}

// ---------------------------------------------------------------------------
// GET /mocktests/attempts  →  MockTestAttemptListItemDto[]
// ---------------------------------------------------------------------------

export interface MockTestAttemptListItemDto {
  /** Named `attemptId`, not `id` — matches the real backend's `MockTestAttemptListItemDto.AttemptId` field exactly (verified against a live response; this mismatch shipped broken once before being caught by a live UI test). */
  attemptId: string;
  templateTitle: string;
  score: number;
  correctCount: number;
  totalQuestions: number;
  percentileRank: number;
  submittedAtUtc: string;
}

export async function getMockTestAttempts(): Promise<MockTestAttemptListItemDto[]> {
  const { data } = await coreApiClient.get<MockTestAttemptListItemDto[]>("/mocktests/attempts");
  return data;
}

// ---------------------------------------------------------------------------
// GET /mocktests/attempts/{id}/review  →  MockTestReviewItemDto[]
// ---------------------------------------------------------------------------

export interface MockTestReviewItemDto {
  questionId: string;
  questionText: string;
  options: string[];
  /** Null if this question was left unanswered — free navigation means not every question is guaranteed a submitted answer, unlike the quiz's linear flow. */
  selectedOptionIndex: number | null;
  correctOptionIndex: number;
  explanation: string;
}

export async function getMockTestReview(attemptId: string): Promise<MockTestReviewItemDto[]> {
  const { data } = await coreApiClient.get<MockTestReviewItemDto[]>(`/mocktests/attempts/${attemptId}/review`);
  return data;
}

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the real "Rapid Fire Quiz" backend contract (base path
// `api/v1/quiz`) — verified directly against the backend's actual C# request/
// response records once its source became available in this repo
// (`backend/src/StudyVerse.Application/Features/Quiz/**`,
// `backend/src/StudyVerse.Api/Controllers/QuizController.cs`), even though the
// server itself wasn't running yet to hit over HTTP. Two behaviors worth
// flagging explicitly since they aren't obvious from field names alone:
//
// 1. Enums serialize as camelCase strings (`Program.cs` registers
//    `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`), so
//    `QuizDifficulty.Easy` is the wire value `"easy"`, not `"Easy"`.
// 2. `SubmitAnswerCommandValidator` requires `selectedOptionIndex` to be
//    `InclusiveBetween(0, 3)` — there is no "no answer" sentinel the server
//    accepts. See the `NO_ANSWER_FALLBACK_OPTION_INDEX` comment in the play
//    screen for how a timeout is submitted anyway.
// ---------------------------------------------------------------------------

export type QuizDifficulty = "easy" | "medium" | "hard";

/** Mirrors the backend's `QuizSessionStatus` enum (also camelCase on the wire). */
export type QuizSessionStatus = "inProgress" | "completed" | "abandoned";

// ---------------------------------------------------------------------------
// GET /quiz/categories  →  QuizCategorySummaryDto[]
// ---------------------------------------------------------------------------

export interface QuizCategoryDto {
  category: string;
  easyCount: number;
  mediumCount: number;
  hardCount: number;
  totalCount: number;
}

export async function getQuizCategories(): Promise<QuizCategoryDto[]> {
  const { data } = await coreApiClient.get<QuizCategoryDto[]>("/quiz/categories");
  return data;
}

// ---------------------------------------------------------------------------
// GET /quiz/daily-challenge/status  →  DailyQuizChallengeStatusDto
// ---------------------------------------------------------------------------

export interface DailyChallengeStatusDto {
  category: string;
  difficulty: QuizDifficulty;
  completedToday: boolean;
}

export async function getDailyChallengeStatus(): Promise<DailyChallengeStatusDto> {
  const { data } = await coreApiClient.get<DailyChallengeStatusDto>("/quiz/daily-challenge/status");
  return data;
}

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

/** Mirrors `QuizQuestionOptionsDto` — never includes the correct answer. */
export interface QuizQuestionDto {
  id: string;
  questionText: string;
  options: string[];
}

export interface QuizPowerUpsAvailableDto {
  fiftyFifty: boolean;
  extraTime: boolean;
}

/**
 * Mirrors `SubmitAnswer.QuizSessionSummaryDto`. Only present on the answer
 * that completes the session. Notably has no `accuracy` field — callers
 * compute it as `correctAnswers / totalQuestions` if they need a percentage.
 */
export interface QuizSessionSummaryDto {
  totalQuestions: number;
  correctAnswers: number;
  score: number;
  xpEarned: number;
  coinsEarned: number;
  bestCombo: number;
  completedAllQuestions: boolean;
  ranOutOfLives: boolean;
  dailyChallengeBonusXp: number;
  dailyChallengeBonusCoins: number;
}

// ---------------------------------------------------------------------------
// POST /quiz/sessions  →  StartQuizSessionResultDto
// ---------------------------------------------------------------------------

export interface StartQuizSessionRequest {
  category: string;
  difficulty: QuizDifficulty;
  isDailyChallenge: boolean;
}

export interface StartQuizSessionResponse {
  sessionId: string;
  questions: QuizQuestionDto[];
  livesRemaining: number;
  powerUpsAvailable: QuizPowerUpsAvailableDto;
  totalQuestions: number;
}

export async function startQuizSession(
  request: StartQuizSessionRequest,
): Promise<StartQuizSessionResponse> {
  const { data } = await coreApiClient.post<StartQuizSessionResponse>("/quiz/sessions", request);
  return data;
}

// ---------------------------------------------------------------------------
// POST /quiz/sessions/{id}/answers  →  SubmitAnswerResultDto
// ---------------------------------------------------------------------------

export interface SubmitQuizAnswerRequest {
  questionId: string;
  /** Must be 0-3 — the backend validator rejects anything else, including a "no answer" sentinel. */
  selectedOptionIndex: number;
  timeTakenMs: number;
}

export interface SubmitQuizAnswerResponse {
  isCorrect: boolean;
  correctOptionIndex: number;
  explanation: string;
  xpEarnedThisAnswer: number;
  comboCount: number;
  livesRemaining: number;
  isSessionComplete: boolean;
  sessionSummary?: QuizSessionSummaryDto;
}

export async function submitQuizAnswer(
  sessionId: string,
  request: SubmitQuizAnswerRequest,
): Promise<SubmitQuizAnswerResponse> {
  const { data } = await coreApiClient.post<SubmitQuizAnswerResponse>(
    `/quiz/sessions/${sessionId}/answers`,
    request,
  );
  return data;
}

// ---------------------------------------------------------------------------
// POST /quiz/sessions/{id}/power-ups/fifty-fifty  →  FiftyFiftyResultDto
// POST /quiz/sessions/{id}/power-ups/extra-time   →  UseExtraTimeResultDto
// ---------------------------------------------------------------------------

export interface FiftyFiftyResponse {
  hiddenOptionIndexes: number[];
}

/** Named to avoid a `use*` prefix — this is a plain API call, not a hook, and `react-hooks/rules-of-hooks` lint would otherwise treat it as one. */
export async function activateFiftyFiftyPowerUp(sessionId: string): Promise<FiftyFiftyResponse> {
  const { data } = await coreApiClient.post<FiftyFiftyResponse>(
    `/quiz/sessions/${sessionId}/power-ups/fifty-fifty`,
  );
  return data;
}

export interface ExtraTimeResponse {
  extraTimeActivated: boolean;
}

export async function activateExtraTimePowerUp(sessionId: string): Promise<ExtraTimeResponse> {
  const { data } = await coreApiClient.post<ExtraTimeResponse>(
    `/quiz/sessions/${sessionId}/power-ups/extra-time`,
  );
  return data;
}

// ---------------------------------------------------------------------------
// POST /quiz/sessions/{id}/abandon  →  204 No Content
// ---------------------------------------------------------------------------

export async function abandonQuizSession(sessionId: string): Promise<void> {
  await coreApiClient.post<void>(`/quiz/sessions/${sessionId}/abandon`);
}

// ---------------------------------------------------------------------------
// GET /quiz/sessions/{id}  (resume)  →  QuizSessionStateDto
// ---------------------------------------------------------------------------

export interface QuizSessionStateDto {
  sessionId: string;
  category: string;
  difficulty: QuizDifficulty;
  status: QuizSessionStatus;
  isDailyChallenge: boolean;
  currentQuestionIndex: number;
  totalQuestions: number;
  livesRemaining: number;
  comboCount: number;
  bestCombo: number;
  score: number;
  powerUpsAvailable: QuizPowerUpsAvailableDto;
  /** Null once the session is Completed/Abandoned — nothing left to resume. */
  currentQuestion: QuizQuestionDto | null;
}

export async function getQuizSession(sessionId: string): Promise<QuizSessionStateDto> {
  const { data } = await coreApiClient.get<QuizSessionStateDto>(`/quiz/sessions/${sessionId}`);
  return data;
}

// ---------------------------------------------------------------------------
// GET /quiz/sessions/{id}/review  →  QuizReviewDto
// ---------------------------------------------------------------------------

export interface QuizReviewQuestionDto {
  questionId: string;
  orderIndex: number;
  questionText: string;
  options: string[];
  correctOptionIndex: number;
  /** Null only if this question in the session was never reached/answered (e.g. an abandoned session). */
  selectedOptionIndex: number | null;
  isCorrect: boolean | null;
  explanation: string;
  timeTakenMs: number | null;
}

export interface QuizReviewResponse {
  sessionId: string;
  category: string;
  difficulty: QuizDifficulty;
  isDailyChallenge: boolean;
  score: number;
  xpEarned: number;
  coinsEarned: number;
  bestCombo: number;
  questions: QuizReviewQuestionDto[];
}

export async function getQuizReview(sessionId: string): Promise<QuizReviewResponse> {
  const { data } = await coreApiClient.get<QuizReviewResponse>(`/quiz/sessions/${sessionId}/review`);
  return data;
}

// ---------------------------------------------------------------------------
// GET /quiz/stats  →  QuizStatsDto
// ---------------------------------------------------------------------------

export interface QuizCategoryStatDto {
  category: string;
  questionsAnswered: number;
  questionsCorrect: number;
}

export interface QuizStatsResponse {
  totalSessionsPlayed: number;
  totalQuestionsAnswered: number;
  totalCorrectAnswers: number;
  /** 0-100, already a percentage (not a 0-1 fraction). */
  accuracyPercent: number;
  bestComboEver: number;
  categoryBreakdown: QuizCategoryStatDto[];
}

export async function getQuizStats(): Promise<QuizStatsResponse> {
  const { data } = await coreApiClient.get<QuizStatsResponse>("/quiz/stats");
  return data;
}

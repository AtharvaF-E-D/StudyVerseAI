import { isAxiosError } from "axios";

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Coding Practice" backend contract (base path
// `api/v1/coding`), built in parallel with this mobile work. Unlike
// Current Affairs (whose full `Application`/`Api` layers had already landed
// by the time that client was written), Coding Practice's backend is only
// PARTWAY built in this checkout: `StudyVerse.Domain/Entities/CodingProblem.cs`,
// `CodingProblemTestCase.cs`, `CodeSubmission.cs`, `Enums/CodingDifficulty.cs`,
// `Enums/CodeSubmissionStatus.cs`, and two pure Domain helpers
// (`CodingScoring`, `DailyCodingChallengeSelector`) all exist — but there is
// NO `StudyVerse.Application/Features/CodingPractice/**` (no query/command
// handlers, no DTOs, no seed data), NO `CodingPracticeController`, no
// `IJudge0Provider` implementation, and no EF configuration/migration/DbSet
// for either entity anywhere in `StudyVerse.Infrastructure` (confirmed by
// grepping the whole backend source tree, not just the one feature folder).
// `http://localhost:5221` also isn't accepting connections in this
// environment. So this feature genuinely cannot be exercised end-to-end yet
// — every function below is coded to the phase brief's contract shorthand,
// cross-checked against what Domain code *does* exist, and verified visually
// via `app/(dev)/coding-preview.tsx`'s fixtures instead. Re-verify against the
// real controller once `Features/CodingPractice` and the API controller land.
//
// One real deviation the shorthand contract almost certainly gets wrong,
// found by reading the actual enums and `Program.cs`: EVERY controller in
// this app registers `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`
// globally, and `Enums/CodingDifficulty.cs` / `Enums/CodeSubmissionStatus.cs`
// both use plain C# `PascalCase` member names (`Easy`, `WrongAnswer`, ...) —
// exactly like `QuizDifficulty`, which the already-shipped Rapid Fire Quiz
// client confirms serializes as lowercase `"easy"`/`"medium"`/`"hard"`, NOT
// the shorthand's `"Easy"`/`"Medium"`/`"Hard"`. So `difficulty` below is
// typed as lowercase, and `status` as camelCase (`"wrongAnswer"`,
// `"compileError"`, `"runtimeError"`, not `"WrongAnswer"`/`"CompileError"`/
// `"RuntimeError"`) — mirroring the CamelCase policy's actual output for a
// multi-word PascalCase name, not the brief's shorthand casing.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

/** Mirrors `Domain.Enums.CodingDifficulty` serialized via the app-wide camelCase enum converter. */
export type CodingDifficulty = "easy" | "medium" | "hard";

/** Mirrors `Domain.Enums.CodeSubmissionStatus` serialized via the app-wide camelCase enum converter. */
export type CodeSubmissionStatus = "accepted" | "wrongAnswer" | "compileError" | "runtimeError" | "error";

// ---------------------------------------------------------------------------
// GET /coding/problems?difficulty=&category=&interviewOnly=  →  ProblemSummaryDto[]
// ---------------------------------------------------------------------------

export interface ProblemSummaryDto {
  id: string;
  title: string;
  difficulty: CodingDifficulty;
  category: string;
  isInterviewQuestion: boolean;
  isSolved: boolean;
}

export interface GetProblemsParams {
  difficulty?: CodingDifficulty;
  category?: string;
  interviewOnly?: boolean;
}

export async function getProblems(params?: GetProblemsParams): Promise<ProblemSummaryDto[]> {
  const { data } = await coreApiClient.get<ProblemSummaryDto[]>("/coding/problems", {
    params: {
      difficulty: params?.difficulty,
      category: params?.category || undefined,
      interviewOnly: params?.interviewOnly || undefined,
    },
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /coding/problems/{id}?languageId=  →  ProblemDetailDto
// ---------------------------------------------------------------------------

/** One worked example shown on the problem detail screen — `CodingProblemTestCase` rows with `IsSample = true`. */
export interface SampleTestCaseDto {
  input: string;
  expectedOutput: string;
}

export interface ProblemDetailDto {
  id: string;
  title: string;
  description: string;
  difficulty: CodingDifficulty;
  category: string;
  isInterviewQuestion: boolean;
  sampleTestCases: SampleTestCaseDto[];
  /** The starter snippet for the requested `languageId` only (server resolves `CodingProblem.StarterCodeJson` down to one string). */
  starterCode: string;
}

export async function getProblem(problemId: string, languageId: number): Promise<ProblemDetailDto> {
  const { data } = await coreApiClient.get<ProblemDetailDto>(`/coding/problems/${problemId}`, {
    params: { languageId },
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /coding/languages  →  LanguageDto[]
// ---------------------------------------------------------------------------

/** Judge0 language id + display name (e.g. `{ languageId: 109, name: "Python (3.13.2)" }`). */
export interface LanguageDto {
  languageId: number;
  name: string;
}

export async function getLanguages(): Promise<LanguageDto[]> {
  const { data } = await coreApiClient.get<LanguageDto[]>("/coding/languages");
  return data;
}

// ---------------------------------------------------------------------------
// POST /coding/problems/{id}/submit  →  SubmitSolutionResponse
// ---------------------------------------------------------------------------

export interface SubmitSolutionRequest {
  languageId: number;
  sourceCode: string;
}

/**
 * One graded test case result. Non-sample (`isSample: false`) cases are
 * StudyVerse's hidden-test anti-cheat design, same reasoning as quiz answers
 * never including the correct index up front — the UI must never render
 * `input`/`expectedOutput`/`actualOutput` for a non-sample result even if a
 * future backend build happens to populate them, only whether it `passed`.
 */
export interface SubmissionTestResultDto {
  input: string | null;
  expectedOutput: string | null;
  actualOutput: string | null;
  passed: boolean;
  isSample: boolean;
}

export interface SubmitSolutionResponse {
  status: CodeSubmissionStatus;
  testsPassed: number;
  totalTests: number;
  results: SubmissionTestResultDto[];
  xpAwarded: number;
  coinsAwarded: number;
  /** True when the user had already solved this problem before (first-ever Accepted is the only run that awards XP/coins — see `CodingScoring`). */
  alreadySolved: boolean;
}

/**
 * Grades a real submission via Judge0 — genuinely takes several real seconds
 * (actual code execution against every test case), so this overrides
 * `coreApiClient`'s shared 15s default the same way `getArticleQuiz`/
 * `createStudyPlan` do for their own real AI-generation calls.
 */
export async function submitSolution(
  problemId: string,
  request: SubmitSolutionRequest,
): Promise<SubmitSolutionResponse> {
  const { data } = await coreApiClient.post<SubmitSolutionResponse>(
    `/coding/problems/${problemId}/submit`,
    request,
    { timeout: 60_000 },
  );
  return data;
}

// ---------------------------------------------------------------------------
// POST /coding/problems/{id}/hint  →  HintResponse
// ---------------------------------------------------------------------------

export interface GetHintRequest {
  currentCode: string;
}

export interface HintResponse {
  hint: string;
}

/** Real AI call — same 120s override reasoning as `getArticleQuiz`. */
export async function getHint(problemId: string, request: GetHintRequest): Promise<HintResponse> {
  const { data } = await coreApiClient.post<HintResponse>(`/coding/problems/${problemId}/hint`, request, {
    timeout: 120_000,
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /coding/daily-challenge  →  DailyChallengeDto, or null when there's
// nothing to show yet. Not documented as ever-absent in the brief, but every
// other "one featured item per day" endpoint in this app (weekly digest,
// quiz daily challenge) tolerates a real 404 rather than assuming the pool
// can never be momentarily empty, so this follows the same defensive
// convention (see `getWeeklyDigest`).
// ---------------------------------------------------------------------------

export interface DailyChallengeDto {
  problemId: string;
  title: string;
  difficulty: CodingDifficulty;
}

export async function getDailyChallenge(): Promise<DailyChallengeDto | null> {
  try {
    const { data } = await coreApiClient.get<Partial<DailyChallengeDto> | null>("/coding/daily-challenge");
    if (!data || !data.problemId || !data.title || !data.difficulty) {
      return null;
    }
    return data as DailyChallengeDto;
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

// ---------------------------------------------------------------------------
// GET /coding/submissions?problemId=  →  SubmissionHistoryItemDto[]
// ---------------------------------------------------------------------------

export interface SubmissionHistoryItemDto {
  id: string;
  problemId: string;
  languageId: number;
  status: CodeSubmissionStatus;
  testsPassed: number;
  totalTests: number;
  submittedAtUtc: string;
}

export async function getSubmissions(problemId?: string): Promise<SubmissionHistoryItemDto[]> {
  const { data } = await coreApiClient.get<SubmissionHistoryItemDto[]>("/coding/submissions", {
    params: { problemId: problemId || undefined },
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /coding/stats  →  CodingStatsDto
// ---------------------------------------------------------------------------

export interface CodingStatsDto {
  totalSolved: number;
  solvedByDifficulty: {
    easy: number;
    medium: number;
    hard: number;
  };
  totalSubmissions: number;
  currentDailyStreak: number;
}

export async function getCodingStats(): Promise<CodingStatsDto> {
  const { data } = await coreApiClient.get<CodingStatsDto>("/coding/stats");
  return data;
}

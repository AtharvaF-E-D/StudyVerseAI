import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Interview Prep" backend contract (base path
// `api/v1/interview`), built in parallel with this mobile work. Unlike Quiz/
// Notes/Study Planner (whose real backend source eventually became readable
// in this checkout even before the server was runnable), NO trace of an
// Interview Prep feature exists anywhere in `backend/src` yet — confirmed by
// grepping the whole backend tree for "interview" case-insensitively (the
// only hits are unrelated `CodingPractice*` build artifacts whose *content*
// happens to contain the substring) — and `http://localhost:5221` refuses
// connections in this environment. So this is in the exact same spot
// `src/api/codingpractice.ts` was in: every shape below is coded directly to
// the phase brief's contract shorthand, verified visually via
// `app/(dev)/interview-preview.tsx`'s fixtures instead of a live round trip.
// Re-verify against the real controller/DTOs once they exist.
//
// Two calls made by extrapolating from every OTHER feature's CONFIRMED real
// backend behavior (Quiz, Notes, Coding Practice, and Study Planner all
// independently confirmed the same `Program.cs` registers a global
// `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`):
//   1. `InterviewCategory` is almost certainly a C# enum (`Hr`, `Technical`,
//      `Behavioral`), so the wire value is lowercase camelCase
//      (`"hr"`/`"technical"`/`"behavioral"`), not the brief's shorthand
//      `"Hr"`/`"Technical"`/`"Behavioral"` PascalCase.
//   2. The contract documents no `GET /resume/{id}` detail endpoint — only
//      `POST /resume` (returns one full analysis) and `GET /resume/history`
//      ("past analyses"). For a past analysis to be viewable at all after
//      the app restarts (or after tapping into it from the history list),
//      `GET /resume/history` must return the SAME full shape `POST /resume`
//      does (score/strengths/weaknesses/suggestions), not a lightweight
//      summary — so `ResumeAnalysisDto` is reused for both, and
//      `useInterviewPrep.ts`'s resume screen reads one specific analysis
//      back out of that single list query's cache instead of inventing a
//      round trip the contract never promises — the same reasoning
//      `src/lib/quizSessionCache.ts` / `mockTestAttemptCache.ts` use to avoid
//      re-fetching data a prior response already handed over in full.
// ---------------------------------------------------------------------------

export type InterviewCategory = "hr" | "technical" | "behavioral";

/** Fixed, known set — mirrors the fact that `CodingDifficulty`/`QuizDifficulty` are small closed enums too, not an open server-driven list. Used as the picker screen's fallback if `GET /categories` is ever unreachable/empty. */
export const INTERVIEW_CATEGORIES: InterviewCategory[] = ["hr", "technical", "behavioral"];

// ---------------------------------------------------------------------------
// GET /interview/categories  →  InterviewCategory[]
// ---------------------------------------------------------------------------

export async function getInterviewCategories(): Promise<InterviewCategory[]> {
  const { data } = await coreApiClient.get<InterviewCategory[]>("/interview/categories");
  return data;
}

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

export interface InterviewQuestionDto {
  questionId: string;
  questionText: string;
}

/** One already-graded answer, present when resuming a session that's partway through (see `GET /interview/sessions/{id}` below). */
export interface InterviewAnswerRecordDto {
  questionId: string;
  answerText: string;
  score: number;
  feedback: string;
}

// ---------------------------------------------------------------------------
// POST /interview/sessions  { type }  →  CreateInterviewSessionResponse
// ---------------------------------------------------------------------------

export interface CreateInterviewSessionRequest {
  type: InterviewCategory;
}

export interface CreateInterviewSessionResponse {
  id: string;
  type: InterviewCategory;
  questions: InterviewQuestionDto[];
}

export async function createInterviewSession(
  request: CreateInterviewSessionRequest,
): Promise<CreateInterviewSessionResponse> {
  const { data } = await coreApiClient.post<CreateInterviewSessionResponse>("/interview/sessions", request);
  return data;
}

// ---------------------------------------------------------------------------
// GET /interview/sessions/{id}  →  InterviewSessionDetailDto (resume)
// ---------------------------------------------------------------------------

export interface InterviewSessionDetailDto {
  id: string;
  type: InterviewCategory;
  questions: InterviewQuestionDto[];
  answers: InterviewAnswerRecordDto[];
  /** Null until `POST /sessions/{id}/complete` has been called. */
  overallScore: number | null;
  /** Null until `POST /sessions/{id}/complete` has been called. */
  improvementPlan: string | null;
}

export async function getInterviewSession(sessionId: string): Promise<InterviewSessionDetailDto> {
  const { data } = await coreApiClient.get<InterviewSessionDetailDto>(`/interview/sessions/${sessionId}`);
  return data;
}

// ---------------------------------------------------------------------------
// POST /interview/sessions/{id}/answers  →  SubmitInterviewAnswerResponse
// ---------------------------------------------------------------------------

export interface SubmitInterviewAnswerRequest {
  questionId: string;
  answerText: string;
}

export interface SubmitInterviewAnswerResponse {
  score: number;
  feedback: string;
}

/**
 * Grades one answer via a real AI call — genuinely takes a few real seconds,
 * so this overrides `coreApiClient`'s shared 15s default the same way
 * `submitSolution`/`getHint` (`src/api/codingpractice.ts`) do for their own
 * real per-item AI calls.
 */
export async function submitInterviewAnswer(
  sessionId: string,
  request: SubmitInterviewAnswerRequest,
): Promise<SubmitInterviewAnswerResponse> {
  const { data } = await coreApiClient.post<SubmitInterviewAnswerResponse>(
    `/interview/sessions/${sessionId}/answers`,
    request,
    { timeout: 60_000 },
  );
  return data;
}

// ---------------------------------------------------------------------------
// POST /interview/sessions/{id}/complete  →  CompleteInterviewSessionResponse
// ---------------------------------------------------------------------------

export interface CompleteInterviewSessionResponse {
  overallScore: number;
  improvementPlan: string;
}

/**
 * Finalizes a session with a real AI call that synthesizes every answer into
 * an overall score + improvement plan — the brief calls this out as taking
 * even longer than per-answer grading. `createStudyPlan`'s real measured
 * ~71s for a similarly-shaped "whole submission, one big AI call" request
 * (`src/api/studyplanner.ts`) is the closest data point this codebase has for
 * how badly this kind of call can undersell "a few seconds", so this uses
 * the same generous 120s override rather than the 60s per-answer one above.
 */
export async function completeInterviewSession(sessionId: string): Promise<CompleteInterviewSessionResponse> {
  const { data } = await coreApiClient.post<CompleteInterviewSessionResponse>(
    `/interview/sessions/${sessionId}/complete`,
    undefined,
    { timeout: 120_000 },
  );
  return data;
}

// ---------------------------------------------------------------------------
// GET /interview/sessions  →  InterviewSessionHistoryItemDto[]
// ---------------------------------------------------------------------------

export interface InterviewSessionHistoryItemDto {
  id: string;
  type: InterviewCategory;
  questionCount: number;
  /** Null for a session that was started but never completed. */
  overallScore: number | null;
  createdAtUtc: string;
}

export async function getInterviewSessions(): Promise<InterviewSessionHistoryItemDto[]> {
  const { data } = await coreApiClient.get<InterviewSessionHistoryItemDto[]>("/interview/sessions");
  return data;
}

// ---------------------------------------------------------------------------
// POST /interview/resume  (multipart/form-data, field name `file`)  →  ResumeAnalysisDto
// ---------------------------------------------------------------------------

/** Mirrors `UploadNoteFile` in `src/api/notes.ts` — see that file for why `webFile` exists (real `File` on web vs. RN's `{ uri, name, type }` convention on native). */
export interface UploadResumeFile {
  uri: string;
  name: string;
  mimeType: string;
  webFile?: File;
}

/** Same cap as `src/api/notes.ts`'s `MAX_NOTE_FILE_SIZE_BYTES` — no resume-specific limit is documented, and 10MB is already generous for a text-based document. */
export const MAX_RESUME_FILE_SIZE_BYTES = 10 * 1024 * 1024;

/** Resumes are text documents, not photographed pages — unlike Notes, there's no image/OCR path here, so this is narrower than `SUPPORTED_NOTE_DOCUMENT_MIME_TYPES`. */
export const SUPPORTED_RESUME_DOCUMENT_MIME_TYPES = [
  "application/pdf",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
];

export interface ResumeAnalysisDto {
  id: string;
  overallScore: number;
  strengths: string[];
  weaknesses: string[];
  suggestions: string[];
  /**
   * Not spelled out in the brief's literal contract shorthand, but a
   * timestamp is needed to order/label history rows — inferred the same way
   * every other upload-and-list feature in this app (`NoteSummaryDto`,
   * `SubmissionHistoryItemDto`, ...) really does carry a `createdAtUtc`.
   */
  createdAtUtc: string;
}

/**
 * Uploads a resume for real AI analysis. Genuinely takes several real
 * seconds (document parsing + a real AI call), so this overrides the shared
 * 15s default — see `completeInterviewSession`'s comment for why 120s.
 */
export async function uploadResume(file: UploadResumeFile): Promise<ResumeAnalysisDto> {
  const formData = new FormData();
  if (file.webFile) {
    formData.append("file", file.webFile, file.name);
  } else {
    // Not a real `Blob` — this object literal is React Native's own
    // documented FormData file-part convention (see `notes.ts`'s identical
    // cast), so the cast just satisfies the DOM-lib-shaped
    // `FormData.append` signature TypeScript sees.
    formData.append("file", { uri: file.uri, name: file.name, type: file.mimeType } as unknown as Blob);
  }

  const { data } = await coreApiClient.post<ResumeAnalysisDto>("/interview/resume", formData, {
    headers: { "Content-Type": "multipart/form-data" },
    timeout: 120_000,
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /interview/resume/history  →  ResumeAnalysisDto[]
// ---------------------------------------------------------------------------

/** Returns every past resume analysis IN FULL — see this file's header for why there's no separate detail endpoint to call instead. */
export async function getResumeHistory(): Promise<ResumeAnalysisDto[]> {
  const { data } = await coreApiClient.get<ResumeAnalysisDto[]>("/interview/resume/history");
  return data;
}

// ---------------------------------------------------------------------------
// GET /interview/stats  →  InterviewStatsDto
// ---------------------------------------------------------------------------

export interface InterviewStatsDto {
  sessionsCompleted: number;
  averageScoreByType: {
    hr: number;
    technical: number;
    behavioral: number;
  };
  resumeAnalysesCount: number;
}

export async function getInterviewStats(): Promise<InterviewStatsDto> {
  const { data } = await coreApiClient.get<InterviewStatsDto>("/interview/stats");
  return data;
}

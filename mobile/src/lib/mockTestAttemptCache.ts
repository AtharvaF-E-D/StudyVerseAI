import type { MockTestQuestionDto } from "../api/mocktests";

export interface StartedMockTestAttempt {
  /** The start endpoint itself doesn't return this — carried over from the template the caller already had in hand when it started the attempt. */
  templateTitle: string;
  durationMinutes: number;
  startedAtUtc: string;
  questions: MockTestQuestionDto[];
}

/**
 * Ephemeral, in-memory handoff for the payload `POST /mocktests/attempts`
 * returns — the only place the backend contract exposes the full question
 * list for an attempt (`GET /mocktests/attempts/{id}` returns a
 * post-submission results summary instead — see `src/api/mocktests.ts`'s file
 * header). `app/(app)/mocktests/index.tsx` stashes it (plus the template's
 * title) right after starting an attempt, and
 * `app/(app)/mocktests/[attemptId].tsx` reads it back on mount.
 *
 * Deliberately NOT persisted (no storage/AsyncStorage), and — unlike
 * `quizSessionCache` — there is no server-side resume fallback for this
 * feature (the contract has no "current state" endpoint for an in-progress
 * attempt), so a page reload mid-exam on web genuinely loses local progress;
 * the exam screen shows an honest error state pointing back to the mock
 * tests list in that case rather than pretending to recover it.
 */
const cache = new Map<string, StartedMockTestAttempt>();

export function stashStartedMockTestAttempt(attemptId: string, payload: StartedMockTestAttempt): void {
  cache.set(attemptId, payload);
}

export function takeStartedMockTestAttempt(attemptId: string): StartedMockTestAttempt | undefined {
  return cache.get(attemptId);
}

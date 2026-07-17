import type { StartQuizSessionResponse } from "../api/quiz";

/**
 * Ephemeral, in-memory handoff for the payload `POST /quiz/sessions` returns
 * — the only place the backend contract exposes the full question list for a
 * session (the resume endpoint, `GET /quiz/sessions/{id}`, only ever returns
 * the current question). `app/(app)/quiz/index.tsx` stashes it right after
 * starting a session, and `app/(app)/quiz/[sessionId].tsx` reads it back on
 * mount, avoiding round-tripping the whole question array through
 * expo-router's string-only params.
 *
 * Deliberately NOT persisted (no storage/AsyncStorage): if the play screen
 * mounts without a cached entry for that session id (a page reload on web, or
 * a deep link straight into an in-progress session), it falls back to
 * `useQuizSessionQuery` to resume from the server's authoritative session
 * state instead of failing.
 */
const cache = new Map<string, StartQuizSessionResponse>();

export function stashStartedQuizSession(sessionId: string, payload: StartQuizSessionResponse): void {
  cache.set(sessionId, payload);
}

export function takeStartedQuizSession(sessionId: string): StartQuizSessionResponse | undefined {
  return cache.get(sessionId);
}

import type { CreateInterviewSessionResponse } from "../api/interviewprep";

/**
 * Ephemeral, in-memory handoff for the payload `POST /interview/sessions`
 * returns, mirroring `src/lib/quizSessionCache.ts` exactly. Unlike the quiz's
 * resume endpoint (which only ever returns the CURRENT question),
 * `GET /interview/sessions/{id}` here returns the full question list (plus
 * any already-graded answers) too — so this cache is purely an optimization
 * to skip one redundant round trip on the normal start -> play navigation,
 * not a requirement for resume to work at all.
 *
 * `app/(app)/interview/index.tsx` stashes it right after starting a session,
 * and `app/(app)/interview/[sessionId].tsx` reads it back on mount, falling
 * back to `useInterviewSessionQuery` (the real resume path) whenever there's
 * no cached entry — a page reload on web, or a deep link straight into an
 * in-progress session id.
 */
const cache = new Map<string, CreateInterviewSessionResponse>();

export function stashStartedInterviewSession(sessionId: string, payload: CreateInterviewSessionResponse): void {
  cache.set(sessionId, payload);
}

export function takeStartedInterviewSession(sessionId: string): CreateInterviewSessionResponse | undefined {
  return cache.get(sessionId);
}

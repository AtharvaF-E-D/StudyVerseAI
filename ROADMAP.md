# ROADMAP.md — StudyVerse AI

Status legend: ✅ done · 🚧 in progress · ⬜ not started

| Phase | Name | Status | Notes |
|---|---|---|---|
| 1 | Foundation | ✅ | backend (.NET 9 Clean Architecture, full auth) and mobile (Expo Router auth flow) built and independently verified — see checklist below for the couple of items still open |
| 2 | Beautiful UI System | ✅ | elevation/motion tokens, icon system, full shared component library, responsive layout, dark mode — built and visually verified (screenshots) in both light and dark |
| 3 | Dashboard | ✅ | backend (streak/XP/coins, daily challenges, leaderboard, notifications) and mobile dashboard screen built and verified end-to-end through the real UI |
| 4 | AI Tutor | ✅ (scoped) | real OpenAI (gpt-4o-mini) chat, conversation history/search/bookmarks, offline KaTeX math + code rendering, daily token cap. Voice input/output, OCR/image understanding, and true token streaming deliberately deferred — time-boxed, not oversights |
| 5 | Rapid Fire Quiz | ✅ | 90 real seeded questions across 5 categories × 3 difficulties, lives/combo/scoring/power-ups/daily-challenge, anti-repetition question selection, review screen — built and verified (backend rigorously via live curl walkthrough, mobile via live UI screenshots + independent typecheck/lint) |
| 6 | AI Notes | ✅ (scoped, backend only) | PDF (PdfPig)/DOCX (OpenXml)/image (OpenAI vision) upload → local-disk storage (cloud-swappable via `IFileStorageService`) → one structured-JSON OpenAI call generating summary, key points, flashcards, mcqs, mind-map outline, revision sheet, vocabulary, formulas — built and verified end-to-end with a real uploaded PDF and a real OpenAI response. PPTX support and a mobile UI deliberately deferred — time-boxed, not oversights |
| 7 | Flashcards | ✅ | AI-generated + manual decks, real SM-2 spaced repetition, sharing, favorites, daily review queue — built and verified end-to-end through the real live UI |
| 8 | Mock Tests | ✅ | timed exams over the Phase 5 question bank, real percentile/rank, AI weakness analysis, review — built and verified end-to-end through the real live UI |
| 9 | Study Planner | ✅ | AI day-by-day plan generation with weak-topic weighting, daily/weekly views, automatic missed-task recovery — built and verified end-to-end through the real live UI |
| 10 | Current Affairs | ✅ | real live news via GNews API, category feed, search, bookmarks, per-article AI comprehension quiz, weekly AI digest — built and verified end-to-end with genuinely live headlines |
| 11 | Coding Practice | ✅ | real Judge0 code execution across 5 languages, 26 seeded problems, AI hints, daily challenge, progress tracking — built and verified end-to-end with real code actually compiled and run |
| 12 | Interview Preparation | ✅ (scoped) | HR/Technical/Behavioral practice with real per-answer AI grading + session improvement plans, real resume analysis — built and verified end-to-end. Voice interviews deliberately deferred, same as Phase 4 |
| 13 | Gamification | ✅ | 12 badges, 5 rotating weekly missions, 7-day escalating daily rewards + seasonal event bonus, weighted spin wheel — built and verified end-to-end through the real live UI (claimed a real daily reward, spun the real wheel) |
| 14 | Monetization | ⬜ | |
| 15 | Admin Portal | ⬜ | |
| 16 | Backend Infrastructure hardening | ⬜ | partial: health checks, rate limiting, and Serilog land in Phase 1; queueing/audit logging/backups deferred |
| 17 | Security hardening | ⬜ | partial: JWT+refresh, password hashing, input validation land in Phase 1; account lockout, abuse detection, GDPR export/delete deferred |
| 18 | Performance | ⬜ | |
| 19 | Testing | ⬜ | Phase 1 ships unit tests for auth handlers only; broader suite deferred |
| 20 | Production Deployment | ⬜ | cloud provider choice deferred to this phase |
| 21 | Launch Readiness | ⬜ | |

## Phase 1 — Foundation

- [x] Monorepo structure (`backend/`, `mobile/`, `admin/`, `docs/`)
- [x] PRODUCT.md / ARCHITECTURE.md / ROADMAP.md
- [x] .NET 9 Clean Architecture solution (Domain/Application/Infrastructure/Api) — `dotnet build` and `dotnet test` (7/7) verified
- [x] Postgres schema + EF Core migrations for auth (Users, RefreshTokens, OtpCodes, UserTokens) — `InitialCreate` migration generated
- [x] Auth endpoints: register, login, refresh (with rotation + reuse-detection), logout, verify-email, resend-verification, forgot/reset password, OTP request/verify, Google login, Apple login, `GET /me`
- [x] Redis integration (OTP cache, login-lockout counter, rate limiting)
- [x] Serilog, FluentValidation, AutoMapper, health checks (`/health/live`, `/health/ready`), Swagger/OpenAPI, API versioning, JWT bearer auth, fixed-window rate limiter on auth endpoints
- [x] Expo mobile app: TypeScript, Expo Router, NativeWind, theme tokens — `tsc --noEmit` and `eslint` verified clean
- [x] Mobile auth screens: splash, onboarding, login (+ Google/Apple sign-in), register, OTP (with resend cooldown), forgot/reset password — wired to the backend contract above
- [x] Zustand auth store (MMKV-persisted) + React Query client + network monitor (NetInfo) + axios client with single-flight 401 refresh-and-retry
- [x] Docker Compose (Postgres + Redis + backend) for local dev — not container-build-verified yet (no Docker in the build sandbox); verify `docker compose up` once Docker Desktop is available
- [x] GitHub Actions CI for backend and mobile
- [x] Env flavors: backend appsettings per environment (Development/Staging/Production); mobile `.env.*` + EAS build profiles

Not built in Phase 1 (was mistakenly listed here before implementation and is
now corrected): no analytics/crash-reporting abstraction
(`IAnalyticsClient`/`ICrashReporter`) exists in the mobile app yet, and no
`IAiChatProvider` abstraction exists in the backend yet. Add both when their
respective phases (crash/analytics is cross-cutting, do it alongside Phase 2;
AI provider abstraction is Phase 4) actually start.

## Phase 2 — Beautiful UI System

- [x] Elevation tokens (`src/theme/elevation.ts`, platform-correct shadow/elevation) and motion tokens (`src/theme/motion.ts`, durations/easings/entrance helpers, respects reduce-motion)
- [x] Icon system: `src/components/Icon.tsx` wrapping `@expo/vector-icons` Ionicons
- [x] Shared component library: `Card` (flat/raised/glass), `Badge`, `Chip`, `Avatar`, `Divider`, `ListItem`, `ProgressBar`, `Switch`, `Toast` (+ provider), `Skeleton`, `EmptyState`, `ErrorState`
- [x] Responsive layout: `useBreakpoint()`, capped max-width content column on tablet/web via `ScreenContainer`
- [x] Glassmorphism reserved for overlay/floating surfaces only (`Card` `glass` variant, via `expo-blur`)
- [x] Retrofitted the login screen's divider to use the shared `Divider` component
- [x] Dev-only showcase route (`app/(dev)/components.tsx`) exercising every component in light + dark
- [x] Verified: `tsc`/`eslint` clean; screenshotted the showcase screen (light + dark) and the login screen — all components visibly styled correctly, zero console/pageerror output, toast trigger confirmed working live

Caught and fixed one real bug via actually screenshotting (not just checking
for crashes): `Avatar`'s image variant rendered at the source image's
intrinsic size (1024×1024) on web instead of the intended 64×64, because
react-native-web's `Image` sets an inline size style that beats Tailwind
classes — fixed with an explicit `style={{ width, height }}`.

Not built in Phase 2 (deliberately out of scope, listed so it isn't assumed
done): a Modal/BottomSheet primitive, a full accessibility/contrast audit
beyond spot-checking the showcase screen, and any tablet-specific layouts
beyond the capped content width.

## Phase 3 — Dashboard

- [x] Domain: `UserProgress` (Xp/Coins/CurrentStreakDays/LongestStreakDays/LastActivityDateUtc), `Notification`, `ChallengeCompletion`; migration `AddDashboardGamification` applied to the real dev database
- [x] 6 static, feature-agnostic daily challenge templates (`Domain/Gamification/ChallengeTemplate.cs`), 3 selected per day via a deterministic date-based rotation (`DailyChallengeSelector`) — same 3 for every user on a given UTC day
- [x] Level formula (`LevelCalculator`: `floor(sqrt(xp/50)) + 1`)
- [x] `IStreakService` — records a login as "activity" (increments/resets/no-ops correctly), wired into the four real sign-in handlers (Login, OTP-login, Google, Apple) — not refresh-token renewal
- [x] `GET /api/v1/dashboard`, `POST /api/v1/dashboard/challenges/{id}/complete`, `GET/POST /api/v1/notifications[...]`, `GET /api/v1/leaderboard` — all `[Authorize]`
- [x] Welcome notification seeded on registration
- [x] 21 new backend unit tests (28/28 total passing)
- [x] Mobile dashboard screen: streak/level/coins summary, today's challenges (tap-to-complete with a toast confirmation), 7-day activity bar chart, leaderboard preview (own-row highlighted, or a "ranked #N" fallback when outside the top), notifications list, honest `EmptyState` for "Continue learning" (no fabricated content — nothing to show until Phases 4-11 exist), `Skeleton` loading state, `ErrorState` with retry
- [x] Verified end-to-end through the real UI (not mocks): registered a real user, verified via real OTP, landed on the real dashboard fetched from the real backend, tapped a real challenge to completion, watched XP/coins/weekly-chart/leaderboard update live via a real toast and a real dashboard refetch

Deliberately NOT built (honest gaps, not oversights): no `minutesStudied`/time-tracking field anywhere (no feature produces that data until the Study Planner, Phase 9); "Continue learning" and "AI recommendations" are empty states, not fake content, until Phases 4-11 exist; daily challenges are self-reported ("tap to mark done") rather than auto-completed by real quiz/flashcard activity, since those features don't exist yet — wire real auto-completion triggers into `CompleteChallengeCommand`'s call sites once they do.

## Phase 4 — AI Tutor (scoped)

- [x] `Conversation`/`Message` entities, `IAiChatProvider`/`OpenAiChatProvider` (official OpenAI SDK, `gpt-4o-mini`), daily per-user token cap reusing the streak service's date-rollover pattern
- [x] Create/list+search/get-messages/send/bookmark/delete conversations, usage endpoint — all `[Authorize]`
- [x] Mobile chat UI: offline-bundled KaTeX math rendering (WebView on native, DOM injection on web — no CDN, fonts embedded as base64), syntax-highlighted code blocks, tap-to-send follow-up suggestion chips, token-usage indicator, "Ask your AI tutor" dashboard entry point
- [x] 16 new backend unit tests (44/44 total at the time); verified against the **real live OpenAI API** — a real question got a real, correctly-LaTeX-formatted answer back

Deliberately NOT built this pass (time-boxed, not oversights — pick up when there's budget for them): voice input/output, OCR-from-camera/image understanding, and true token-by-token streaming (the backend returns the complete assembled reply; reliable RN-side stream consumption needed more time than was available in the window).

## Phase 5 — Rapid Fire Quiz

- [x] `QuizQuestion`/`QuizSession`/`QuizSessionQuestion` entities; **90 real, hand-written trivia questions** seeded across 5 categories (Science, Mathematics, History, Geography, General Knowledge) × 3 difficulties
- [x] Anti-repetition question selection (excludes questions seen in a user's last 3 completed sessions, with a documented small-pool fallback), lives (3, -1 on wrong answer), combo multiplier scoring (up to 1.5x at a 5+ streak), two power-ups (50-50, +10s), a daily-challenge mode (one per UTC day, deterministic category/difficulty rotation, real 409 rejection on a second attempt)
- [x] Full session lifecycle: start → answer (server validates correctness, never leaks the answer to the client) → complete (lives-out or all-answered) → review (every question with the user's answer, correct answer, explanation) → stats (accuracy, best combo, per-category breakdown)
- [x] 33 new backend unit tests (77/77 total); verified via a **real live curl walkthrough** — combo/lives/scoring/daily-challenge-rejection all confirmed changing correctly in real responses, and the awarded XP/coins confirmed landing in the real dashboard/leaderboard afterward
- [x] Mobile: category/difficulty picker with a stats strip and daily-challenge card, play screen (animated timer bar, hearts, pulsing combo badge, power-up buttons, pause/quit), review screen, "Rapid Fire Quiz" dashboard entry point — independently confirmed rendering real live data (categories, a real fetched question, hearts/timer/options all correctly styled) via screenshots against the running backend

One known gap worth fixing before this ships for real: on a client-side timeout, the mobile app currently submits a fallback guess (`selectedOptionIndex: 0`) rather than a true "no answer" signal, because the backend's answer validator currently requires an index 0-3. If a question's correct answer happens to be index 0, a timed-out non-answer would incorrectly score as correct. Fix: add an explicit "timed out / no answer" case to `SubmitAnswerCommand` rather than overloading index 0.

## Phase 6 — AI Notes (scoped)

- [x] `IFileStorageService`/`LocalFileStorageService` — uploaded files land on local disk under a configurable root; swapping to Cloudflare R2/S3 later is a new implementation of the same interface, no caller changes
- [x] Text extraction: PdfPig for PDF, DocumentFormat.OpenXml for DOCX; images have no separate OCR pipeline and instead reuse the existing OpenAI integration — sent straight to a vision-capable chat completion for transcription
- [x] `Note`/`NoteContent` entities (1:1); `NoteContent`'s seven generated pieces (summary, key points, flashcards, mcqs, mind map, revision sheet, vocabulary, formulas) are stored as JSON text columns — documented as a deliberate simplification for read-heavy, always-whole-unit content, not normalized relational sub-tables. The mind map is a nested outline tree (JSON), not a visual canvas
- [x] `INoteGenerationProvider`/`OpenAiNoteGenerationProvider` — one structured-JSON (OpenAI JSON mode) call per upload produces all seven pieces together; upload/extract/generate all run synchronously within the request (no background job queue yet — Phase 16 territory)
- [x] Upload/list/get/delete endpoints (`api/v1/notes`, `[Authorize]`), 10MB cap, ownership-checked get/delete, `Failed` status with a stored error message on any pipeline failure (never left stuck at `Processing`)
- [x] 29 new backend unit tests (106/106 total); verified end-to-end against the **real live OpenAI API** — a real hand-crafted PDF about the water cycle was uploaded, reached `Ready`, and a real, correctly-structured summary/key points/flashcards/mcqs/mind map/revision sheet came back
- [x] Mobile: upload (document or photo picker), list screen with status badges, tabbed detail screen (Summary/Key Points/Flashcards/MCQs/Mind Map outline/Revision Sheet/Vocabulary/Formulas — formulas reuse the Phase 4 KaTeX renderer), "AI Notes" dashboard entry point — verified against the real running backend: uploaded a real PDF through the actual UI, watched it reach `Ready`, and confirmed the real generated content rendered correctly

Deliberately NOT built this pass (time-boxed, not oversights): PPTX support.

**Real bug caught and fixed by actually driving the live UI (not just fixtures):** the notes list and the Phase 4 tutor conversation list both nested an interactive `Pressable` (delete / bookmark buttons) inside `ListItem`'s `trailing` slot while `ListItem` itself was also `onPress`-active — on web this renders as a literal `<button>` containing another `<button>`, which is invalid HTML that real browsers warn about and can mishandle click-wise. Fixed in both screens by moving the extra buttons to be siblings of `ListItem` instead of children of its `trailing` slot. Worth checking any future `ListItem` usage for the same trap.

## Phase 7 — Flashcards

- [x] `FlashcardDeck`/`Flashcard` entities; real SM-2 spaced-repetition scheduler (`Domain/SpacedRepetition/Sm2Scheduler.cs`) — ease-factor floor 1.3, standard interval progression 1→6→previous×easeFactor, resets on a failed review; a 4-point client scale (Again/Hard/Good/Easy) mapped directly onto SM-2 quality points 0/3/4/5
- [x] Three ways to build a deck: manual (blank + add cards), AI-generated from a topic (`IFlashcardGenerationProvider`/`OpenAiFlashcardGenerationProvider`, one structured-JSON OpenAI call), or copied from an existing Phase 6 note's already-AI-generated flashcards (no extra OpenAI call needed)
- [x] Cross-deck daily review queue (`GET /due`), public no-auth deck sharing (`GET /shared/{token}`, `[AllowAnonymous]`), favorites, per-deck and aggregate stats
- [x] 53 new backend unit tests (159/159 total) — thorough SM-2 scenario coverage (Good/Good/Good, Easy, Hard, Again-resets, floor enforcement) plus ownership/date-filtering/no-auth-share/generate-from-note validation
- [x] Mobile: deck list (stats strip, due-today CTA, favorite stars), deck detail (cards, add/edit/delete, share toggle), review session with a Reanimated 3D card-flip and 4 color-coded rating buttons, "Flashcards" dashboard entry point with a due-count badge
- [x] Verified end-to-end through the real live UI: generated a real AI deck ("Spanish Basics" — real, correct Spanish phrases), reviewed a card through Good→Good→Easy→Again and watched the exact hand-verified SM-2 math play out (ease factor 2.5→2.5→2.6→1.8, interval 1→6→15→reset), confirmed the due queue correctly dropped the reviewed card, shared a deck and fetched it with zero auth headers, flipped a real generated card in the mobile UI and saw the rating buttons

Deliberately NOT built this pass: a dedicated public (unauthenticated) "shared deck" viewer screen on mobile — the share/unshare toggle and the `getSharedDeck` API client function are fully implemented, just no standalone viewing screen for a token-holder without an account yet.

## Phase 8 — Mock Tests

- [x] 5 exam templates (`MockTestCatalog`, stable hardcoded GUIDs) layered over the existing Phase 5 question bank — no new question content needed
- [x] Real percentile/rank via a mean-rank formula that correctly handles ties (`(strictly-lower + 0.5×tied) / totalOthers × 100`), verified live across two real users on the same template (60% scorer → percentile 100 as the first attempt, a 33% scorer → 0, a later 60% tie → 75, matching the formula exactly)
- [x] Real AI-generated weakness analysis (reuses the existing `IAiChatProvider` from Phase 4 — no new AI provider needed) built from the actual wrong answers grouped by category
- [x] Free question navigation (unlike Rapid Fire Quiz's linear flow), a single overall exam timer with auto-submit-on-timeout, confirm-before-submit-with-unanswered-questions, and a full review screen
- [x] 23 new backend unit tests (182/182 total)
- [x] Mobile: templates/history screen, exam-taking screen (timer banner, question navigator dots for answered/current/unanswered), results screen (percentile framing + AI analysis), review screen, "Mock Tests" dashboard entry point

**Real bug caught and fixed by actually driving the live UI (not just fixtures or curl):** the mobile client's `MockTestAttemptListItemDto` typed the attempt identifier as `id`, but the real backend's DTO names it `AttemptId` (→ `attemptId` in JSON) — a genuine contract mismatch, not a naming nitpick. It surfaced first as a React "missing key" warning on the past-attempts list, but the real consequence was worse: tapping a past attempt navigated to `/mocktests/undefined/results`, a completely broken feature, since the mobile agent had built and verified this screen only against fixtures (the backend wasn't reachable yet at the time). Root-caused via a live Playwright run capturing the actual console arguments and a temporary in-component diagnostic log, then fixed by renaming the field to `attemptId` everywhere it's used on the mobile side and re-verified live. This is exactly the class of bug independent live-UI verification exists to catch — a mismatch two separately-built, individually-correct halves can only produce together.

## Phase 9 — Study Planner

- [x] `StudyPlan`/`StudyPlanTask` entities; only one `Active` plan per user (creating a new one archives the prior one); `IAiChatProvider` extended with an optional JSON-mode flag (backward-compatible, existing call sites untouched — 230/230 tests confirm no regression) rather than building a new AI provider abstraction
- [x] Real AI day-by-day plan generation weighting weak topics more heavily — verified live: weak-topic sessions averaged 57.5 min vs 34.3 min for non-weak sessions across a real 33-task generated plan; a 60-day generation horizon cap with out-of-range-date filtering on the AI's response
- [x] Automatic missed-task recovery (`MissedTaskRecoveryService`) — runs transparently on every today/active-plan fetch, no manual trigger needed; verified live by moving a real task's date into the past via direct DB manipulation and confirming it self-healed into a new task on the nearest day with spare capacity, preserving the original date
- [x] 48 new backend unit tests (230/230 total)
- [x] Mobile: plan setup form (exam date, subjects, weak topics, daily budget), plan overview (progress, today's tasks with weak-topic badges and one-tap completion), weekly view (day-grouped, Previous/Next navigation), "Study Planner" dashboard entry point
- [x] Verified end-to-end through the real live UI: created a real plan, watched a real ~27s AI generation complete, completed a task and watched the progress percentage update live, viewed the weekly view showing the full real AI-generated schedule grouped correctly by day

**Real bug caught and fixed by the mobile agent's own live testing (not fixtures):** the shared `coreApiClient`'s 15s axios timeout was killing the create-plan request before real OpenAI generation (which took up to ~70s for a full multi-week plan) could finish — the backend logs showed the client disconnect actually cancelled the server-side OpenAI call too via `HttpContext.RequestAborted`, so no plan was created at all, silently. Fixed with a scoped 120s timeout override on just that one API call, not the shared client default. A good example of why testing against a slow real dependency (not a fast fixture) matters — this would never have surfaced against mocked data.

Deliberately NOT built this pass: a real date-picker (exam date is a validated `yyyy-mm-dd` text field — no date-picker pattern existed anywhere in the app yet to reuse); a visualization specifically calling out which tasks were auto-rescheduled from a missed date (the data — `originalScheduledDateUtc` — is already returned by the API and passed through client-side, just not surfaced in the UI yet).

## Phase 10 — Current Affairs

- [x] Real news via the GNews API (free tier, 12h delay — a real, documented limitation, not a bug), a new `GNewsOptions`/`GNews:ApiKey` config binding mirroring `OpenAiOptions` exactly. `NewsArticle` cache with a 6-hour per-category staleness window (checked before ever calling GNews again) and `ExternalId`-based dedup on refresh
- [x] Categories (9, matching GNews's real taxonomy), live search (never cached, since query terms are unpredictable), bookmarks, single-article view
- [x] Per-article AI comprehension quiz (reuses `IAiChatProvider`, no new AI provider) — cache-first per article, verified live that a repeat request for the same article's quiz is instant (0.009s) and returns byte-identical questions vs. the real first-generation call (7.10s)
- [x] Weekly AI digest, cache-first per ISO week, shared across all users (not per-user) — built from whatever's already cached that week rather than triggering fresh GNews calls just for the digest; a documented "not enough data yet" path instead of fabricating a summary when nothing's cached
- [x] 22 new backend unit tests (252/252 total)
- [x] Mobile: news feed (categories, digest teaser, search), article detail, per-article quiz screen, bookmarks screen, "Current Affairs" dashboard entry point
- [x] Verified end-to-end through the real live UI with genuinely live headlines (not fixtures) — a real feed of actual current news (matching what a live GNews query returns), bookmarked a real article, read its full real content, and generated (and viewed) a real AI quiz question directly testing comprehension of that specific article's actual content

**Deliberately did not fabricate AI "current events"**: the honest per-article description shown in the feed is GNews's own real description field, not an AI-generated summary — inventing per-article AI summaries of news content would risk quietly drifting from what the source article actually says, which matters more for a "current affairs" feature than almost anywhere else in the app.

## Phase 11 — Coding Practice

- [x] Real code execution via Judge0 CE (RapidAPI) — 5 languages (Python, JavaScript, Java, C++, C#), Judge0's own output comparison (no manual string-diffing), graceful `Error` sentinel on outage/rate-limit rather than a 500
- [x] 26 hand-written, verified-correct problems (Easy 11/Medium 10/Hard 5; Arrays/Strings/Math/Recursion/Data Structures; 5 tagged interview-classic — Two Sum, Valid Parentheses, Binary Search, etc.), 107 test cases (52 sample shown to the user, the rest hidden and never leaked in results)
- [x] AI hints (reuses `IAiChatProvider`, no new provider) — verified live to nudge toward an approach without handing over working code or naming the exact algorithm
- [x] Daily coding challenge (same rotation style as other daily-challenge features), first-solve-only XP/coins (15/25/40 XP, 3/5/8 coins by difficulty — no re-farming via resubmission), progress stats
- [x] 26 new backend unit tests (278/278 total)
- [x] Mobile: problem list (stats, daily challenge, difficulty/category/interview filters), detail+editor screen (language picker swaps starter code, hint reveal, submit with a real "Running..." state), color-coded results panel
- [x] Verified end-to-end through the real live UI: browsed real seeded problems, wrote a real Python FizzBuzz solution in the editor, submitted it and watched real Judge0 grading come back (Accepted, 4/4, XP/coins awarded, hidden tests correctly pass/fail-only)

**Two real bugs caught by testing against real infrastructure, not fixtures or mocks:**
1. (Backend) `GetSubmissionsQueryHandler` ordered results by a property read off an already-projected DTO — EF Core's **InMemory** test provider silently tolerated this (all 278 tests passed), but the real **Npgsql** provider threw a 500 the first time it was hit live, since the SQL translator couldn't express it. Fixed by ordering on the raw entity property before projecting to the DTO.
2. (Mobile) The submit mutation had an `onError` handler but no `onSuccess` handler — the real Judge0 grading came back correctly every time, but the result was never stored in component state, so the results panel silently never rendered. Backend correctness alone didn't catch this; only actually clicking Submit and watching the screen did.

**Editor honesty note**: the code editor is a plain monospace multi-line text input with a line-number gutter, not a live-syntax-highlighting editor — the two candidate RN packages for that were last published in 2022, four-plus years stale against this app's RN 0.86/Expo 57/New Architecture stack, so adopting either was the wrong trade for a time-boxed phase. Fully functional for writing and submitting real code, just without color-as-you-type.

## Phase 12 — Interview Preparation (scoped)

- [x] 36 hand-written questions (12 each HR/Technical/Behavioral), each with an internal (never client-visible) grading rubric
- [x] Real per-answer AI grading (0-10, reuses `IAiChatProvider`) — verified live that a weak, short answer and a detailed STAR-format answer score meaningfully differently and the feedback is genuinely responsive to what was actually written, not generic
- [x] Session completion averages the 5 scores into a 0-100 overall score (verified exact math live) and generates a real, specific AI improvement plan referencing the actual weak and strong answers
- [x] Resume upload (reuses Phase 6's `IFileStorageService`/`ITextExtractionService` pipeline exactly, no new file pipeline) → real AI analysis (score + strengths + weaknesses + suggestions) — verified live that the analysis references specifics from the actual uploaded resume content, not boilerplate advice
- [x] 39 new backend unit tests (317/317 total)
- [x] Mobile: category/history screen with stats, one-question-at-a-time practice session with inline graded feedback, resume upload + analysis result screen, dashboard entry point
- [x] Verified end-to-end through the real live UI: started a Behavioral session, submitted a real detailed answer, watched real AI grading (score 8, feedback specifically referencing the disagreement/resolution/outcome described) render correctly inline

**Real bug caught and fixed by live UI testing (not fixtures)**: the mobile `InterviewQuestionDto` typed the question identifier field as `id`, but the real backend's `InterviewSessionQuestionDto` serializes it as `questionId`. Since `currentQuestion.id` was therefore always `undefined`, and `JSON.stringify` silently drops object keys whose value is `undefined`, every answer submission actually sent `{"answerText": "..."}` with no `questionId` field at all — a 400 from FluentValidation's `NotEmpty` check, not a crash, so it would have been easy to miss without checking the actual request/response bodies. Fixed by renaming the mobile field to `questionId` everywhere it's used. This is the third phase in a row where actually exercising the live backend+mobile pair together caught a contract mismatch neither half's own isolated testing could have found.

Deliberately NOT built this pass (time-boxed, not an oversight): voice interviews (real speech-to-text/text-to-speech is a separate, larger effort — same reasoning as Phase 4's deferred voice input/output) and a post-session "review all Q&A" screen (mock tests/quiz have one; not required here, cut as UI polish before the core grading/resume loop).

## Phase 13 — Gamification

- [x] 12 hand-written badges across every existing feature area (Quiz, Flashcards, Coding, Mock Tests, AI Tutor, Study Planner, Current Affairs, Interview Prep, Streak, General) with real, cross-feature activity evaluation — not client-side approximations
- [x] 5 weekly mission templates, ISO-week-rotation selection (`(isoWeek + year) % N`, same deterministic-rotation pattern as the daily coding/quiz challenges), real progress tracking against actual activity
- [x] 7-day escalating daily reward schedule (10/15/20/25/30/40/50 coins), a live seasonal event ("Exam Season Sprint", +15 coin bonus) layered on top
- [x] 8-outcome weighted spin-the-wheel prize table, once-per-day gate (same date-rollover pattern as `StreakService`)
- [x] 382/382 backend unit tests passing
- [x] Mobile: gamification hub (summary strip, badges grid, missions list, daily-reward card, an animated segmented spin wheel built from Reanimated rotations rather than a literal pie chart, since no conic-gradient primitive/SVG dependency exists in this app), dashboard teaser card
- [x] Verified end-to-end through the real live UI: registered, loaded the hub (all 12 badges/3 missions/summary rendered with zero crashes), claimed the real Day-1 daily reward (+25 coins, "Claimed — see you tomorrow!"), spun the real wheel (landed "10 Coins", reveal panel matched the server's `prizeLabel` exactly), confirmed the summary strip's coin total updated to 35 after both real awards

**Four real contract mismatches caught by live UI testing, not by either half's isolated tests** — the worst batch yet, all from the same root cause: the mobile contract was coded to the phase brief's shorthand before the real backend was reachable, and every guessed field name turned out wrong:
1. `GET /missions` and `GET /badges` both actually wrap their list in an envelope object (`{ completedCount, totalCount, missions: [...] }` / `{ earnedCount, totalCount, badges: [...] }`), not a bare array as assumed — crashed the hub outright with `missions.map is not a function` on first load.
2. Every identifier/flag was renamed on the real backend: mission/badge `id` (not `missionTemplateId`/`badgeId`), badge `title`/`category` (not `name`/`iconName` — there is no server-driven icon at all, so the mobile `BadgeTile` now maps each real category string to a local icon itself), and `claimedToday`/`dayNumber`/`spunToday`/`currentStreakDays` (not `alreadyClaimedToday`/`consecutiveDayNumber`/`alreadySpunToday`/`currentStreak`).
3. The real daily-reward-status and summary responses carry several fields the shorthand never anticipated (`tomorrowCoins`/`tomorrowXp`/`activeSeasonalEventName`/`activeSeasonalEventBonusCoins`), and the real claim/spin POST responses return running totals and seasonal-event data (`newXpTotal`/`newCoinsTotal`/`seasonalEventName`/`seasonalEventBonusCoins`) not in the original contract at all.
4. A stale `alreadyClaimedToday` reference survived in the **dashboard** screen's gamification teaser (`app/(app)/index.tsx`), not just the hub — found only by a systematic re-grep across every gamification-consuming file after the hub itself was fixed, confirming the value of re-checking beyond the file that actually crashed.

This is the fifth phase in a row (Phases 9-13) where a live integration test caught a real mobile/backend contract mismatch that neither side's isolated verification (`dotnet test` / `tsc`+`eslint`+fixtures) could have found — every fix above was driven by directly capturing real request/response bodies over the wire, not by re-reading either side's source in isolation. The dev-only `gamification-preview.tsx` fixture screen (used for earlier visual QA while the real backend was unreachable) was deleted once the real hub was verified end-to-end against it, per its own header comment.

## Explicit non-goals for Phase 1 (unchanged, still true)

- No AI provider calls yet (Phase 4).
- No payments (Phase 14).
- No admin portal (Phase 15).
- No production cloud deployment — Docker Compose is local-dev only until
  Phase 20 picks a cloud provider.

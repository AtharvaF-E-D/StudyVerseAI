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
| 8 | Mock Tests | ⬜ | |
| 9 | Study Planner | ⬜ | |
| 10 | Current Affairs | ⬜ | |
| 11 | Coding Practice | ⬜ | |
| 12 | Interview Preparation | ⬜ | |
| 13 | Gamification | ⬜ | minimal primitives (XP, coins, streak, leaderboard) already landed in Phase 3; this phase adds badges, achievements, daily rewards, spin wheel, missions, seasonal events |
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

## Explicit non-goals for Phase 1 (unchanged, still true)

- No AI provider calls yet (Phase 4).
- No payments (Phase 14).
- No admin portal (Phase 15).
- No production cloud deployment — Docker Compose is local-dev only until
  Phase 20 picks a cloud provider.

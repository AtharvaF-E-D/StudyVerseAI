# ROADMAP.md — StudyVerse AI

Status legend: ✅ done · 🚧 in progress · ⬜ not started

| Phase | Name | Status | Notes |
|---|---|---|---|
| 1 | Foundation | ✅ | backend (.NET 9 Clean Architecture, full auth) and mobile (Expo Router auth flow) built and independently verified — see checklist below for the couple of items still open |
| 2 | Beautiful UI System | ✅ | elevation/motion tokens, icon system, full shared component library, responsive layout, dark mode — built and visually verified (screenshots) in both light and dark |
| 3 | Dashboard | ✅ | backend (streak/XP/coins, daily challenges, leaderboard, notifications) and mobile dashboard screen built and verified end-to-end through the real UI |
| 4 | AI Tutor | ⬜ | no `IAiChatProvider` abstraction exists yet — add it when Phase 4 starts |
| 5 | Rapid Fire Quiz | ⬜ | |
| 6 | AI Notes | ⬜ | |
| 7 | Flashcards | ⬜ | |
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

## Explicit non-goals for Phase 1 (unchanged, still true)

- No AI provider calls yet (Phase 4).
- No payments (Phase 14).
- No admin portal (Phase 15).
- No production cloud deployment — Docker Compose is local-dev only until
  Phase 20 picks a cloud provider.

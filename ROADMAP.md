# ROADMAP.md — StudyVerse AI

Status legend: ✅ done · 🚧 in progress · ⬜ not started

| Phase | Name | Status | Notes |
|---|---|---|---|
| 1 | Foundation | ✅ | backend (.NET 9 Clean Architecture, full auth) and mobile (Expo Router auth flow) built and independently verified — see checklist below for the couple of items still open |
| 2 | Beautiful UI System | ⬜ | design tokens + a handful of shared primitives exist in `mobile/src/theme` and `mobile/src/components`; full component library not built |
| 3 | Dashboard | ⬜ | |
| 4 | AI Tutor | ⬜ | no `IAiChatProvider` abstraction exists yet — add it when Phase 4 starts |
| 5 | Rapid Fire Quiz | ⬜ | |
| 6 | AI Notes | ⬜ | |
| 7 | Flashcards | ⬜ | |
| 8 | Mock Tests | ⬜ | |
| 9 | Study Planner | ⬜ | |
| 10 | Current Affairs | ⬜ | |
| 11 | Coding Practice | ⬜ | |
| 12 | Interview Preparation | ⬜ | |
| 13 | Gamification | ⬜ | |
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

## Explicit non-goals for Phase 1 (unchanged, still true)

- No AI provider calls yet (Phase 4).
- No payments (Phase 14).
- No admin portal (Phase 15).
- No production cloud deployment — Docker Compose is local-dev only until
  Phase 20 picks a cloud provider.

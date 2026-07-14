# ARCHITECTURE.md — StudyVerse AI

## Repository Layout

Monorepo, three deployables:

```
StudyVerseAi/
├── backend/     .NET 9 Web API — Clean Architecture + CQRS
├── mobile/      Expo (React Native) — TypeScript
├── admin/       Web admin portal (Phase 15, not yet started)
├── docs/        supplementary technical notes
├── docker-compose.yml
└── .github/workflows/
```

## Backend

### Layering (Clean Architecture)

```
backend/src/
├── StudyVerse.Domain          — entities, value objects, domain events, no dependencies
├── StudyVerse.Application     — CQRS commands/queries (MediatR), interfaces, validators, DTOs
├── StudyVerse.Infrastructure  — EF Core, Postgres, Redis, external services (email, SMS, OAuth, AI providers)
└── StudyVerse.Api             — controllers, middleware, DI composition root, appsettings
```

Dependency rule: `Api → Infrastructure → Application → Domain`. Domain and
Application never reference Infrastructure or Api.

### Key decisions

| Concern | Decision | Why |
|---|---|---|
| Runtime | .NET 9 (`net9.0` TFM) | matches spec; SDK machine has both .NET 9 and 10 runtimes installed |
| CQRS | MediatR | decouples handlers, enables pipeline behaviors (validation, logging, transactions) |
| Validation | FluentValidation, run as a MediatR pipeline behavior | validation lives next to the command, not scattered in controllers |
| Mapping | AutoMapper | entity ↔ DTO projection |
| Persistence | PostgreSQL via EF Core (Npgsql) | relational integrity for users/subscriptions/progress; JSONB where flexible schema is needed (AI content) |
| Caching / ephemeral state | Redis (StackExchange.Redis) | OTP codes, refresh-token deny-list, rate-limit counters, SignalR backplane (future) |
| Background jobs | Hangfire (Postgres storage) | streak resets, scheduled notifications, digest generation — avoids standing up a separate broker for Phase 1 |
| Auth | JWT access tokens (short-lived) + rotating refresh tokens (stored hashed, Redis-checked deny-list) | stateless API auth, revocable sessions |
| Password hashing | ASP.NET Core `PasswordHasher<T>` (PBKDF2) | framework-provided, no extra dependency, upgrade path to Argon2 later if needed |
| OTP delivery | Infrastructure abstraction (`IOtpSender`) with a console/log implementation for dev | swappable for real SMS/email providers per environment without touching Application |
| Logging | Serilog → console (dev) / file + structured sink (prod) | structured logs, environment-driven sinks |
| API docs | Swashbuckle (OpenAPI/Swagger), versioned via `Asp.Versioning` | contract-first collaboration with mobile team |
| Rate limiting | ASP.NET Core built-in `Microsoft.AspNetCore.RateLimiting` | fixed-window per-IP/per-user limiter on auth endpoints, no extra dependency |
| Health checks | `Microsoft.Extensions.Diagnostics.HealthChecks` + Npgsql/Redis probes | `/health/live` and `/health/ready` for orchestrators |

### Provider defaults (deferred decisions the user asked us to default)

- **Cloud provider**: undecided (Azure/AWS/DigitalOcean) — deferred to Phase
  20 (Production Deployment). Phase 1 only assumes "runs in a container behind
  a reverse proxy," nothing provider-specific.
- **AI provider**: OpenAI primary, Gemini as a fallback/secondary provider.
  Modeled behind an `IAiChatProvider` abstraction in Infrastructure so Phase 4
  (AI Tutor) can add providers without changing Application code.
- **Local dev infra**: Docker Compose runs Postgres 16 and Redis 7. No cloud
  account required to develop Phase 1.

### Auth flows implemented in Phase 1

- Register (email + password) → email verification token (logged in dev,
  emailed in prod via `IEmailSender`).
- Login (email + password) → access + refresh token pair.
- Refresh token rotation (old token invalidated on use; reuse triggers
  full session revocation for that device).
- OTP request/verify (phone or email channel) for passwordless/second-factor
  flows used later by mobile OTP screens.
- Forgot password → reset token → reset password.
- Google login (verifies Google ID token server-side via
  `Google.Apis.Auth`).
- Apple login (verifies Apple identity token server-side, JWKS validation).
- Device tracking: each refresh token is bound to a device id/name so a user
  can be signed out of individual devices later (Phase 17).

## Mobile

### Stack

Expo SDK (managed workflow) + TypeScript + Expo Router (file-based navigation)
+ NativeWind (Tailwind for RN) + React Query (server state) + Zustand (client
state) + React Hook Form + Zod (form validation) + Reanimated (animation) +
MMKV (fast encrypted key-value storage for tokens/preferences).

### Structure

```
mobile/
├── app/                # Expo Router routes (file-based)
│   ├── (auth)/          # splash, onboarding, login, register, otp, forgot-password
│   └── (app)/            # authenticated tab/stack routes (Phase 3+)
├── src/
│   ├── api/              # typed API client, endpoint modules
│   ├── stores/           # Zustand stores (auth, session)
│   ├── theme/             # design tokens, light/dark themes
│   ├── components/        # shared component library
│   ├── hooks/
│   └── lib/                # storage (MMKV), network monitor, query client
├── app.config.ts          # env-driven Expo config (dev/staging/prod)
├── eas.json                # EAS build profiles per environment
```

### Environment flavors

Three EAS build profiles (`development`, `staging`, `production`) map to
three `.env.*` files read by `app.config.ts` (`app.config.ts` picks the file
via `APP_ENV`). Each profile gets its own API base URL, bundle identifier
suffix, and app name suffix so all three can be installed side-by-side on one
device.

### Auth/session flow

`useAuthStore` (Zustand, persisted to MMKV) holds the access token in memory
+ MMKV, and the refresh token in MMKV only (never in JS memory longer than
needed). An Axios interceptor attaches the access token, and on 401 attempts
one refresh-and-retry before forcing logout. React Query's `QueryClient` is
wired to the same store to invalidate all queries on logout.

## Cross-cutting

- **CI**: GitHub Actions — `backend-ci.yml` (restore/build/test the .NET
  solution), `mobile-ci.yml` (install/typecheck/lint the Expo app). Both run
  on PR and push to `main`.
- **Secrets**: never committed. Backend reads from `appsettings.{Env}.json`
  (checked in, no secrets) + environment variables / user-secrets for
  anything sensitive. Mobile reads from `.env.*` (gitignored) with
  `.env.example` checked in.

## Deferred / not yet built

Everything from Phase 2 onward (design system, dashboard, AI tutor, quizzes,
notes, flashcards, mock tests, planner, current affairs, coding practice,
interview prep, gamification, monetization, admin portal, and the
production-hardening phases 16–21) is tracked in
[ROADMAP.md](ROADMAP.md) and not yet implemented.

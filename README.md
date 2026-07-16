# StudyVerse AI

> Learn Smarter. Practice Faster. Achieve More.

An AI-powered student learning platform — AI tutor, doubt solver, notes
generator, flashcards, quizzes, mock tests, coding practice, interview prep,
current affairs, and gamification, in one app.

See [PRODUCT.md](PRODUCT.md) for the product vision, [ARCHITECTURE.md](ARCHITECTURE.md)
for technical decisions, and [ROADMAP.md](ROADMAP.md) for phase-by-phase status.

## Repository layout

```
StudyVerseAi/
├── backend/   .NET 9 Web API — Clean Architecture + CQRS (MediatR)
├── mobile/    Expo (React Native) + TypeScript
├── admin/     Web admin portal — not started yet (Phase 15)
└── docs/      supplementary technical notes
```

## Local development

### Prerequisites

- .NET 9 SDK
- Node.js 20+ and npm
- A Postgres 16+ instance reachable from the API — either `docker compose up
  -d postgres redis`, or a native local install (see note below)
- Expo Go app or an Android/iOS simulator, for running the mobile app

Docker isn't required. If it's unavailable, the backend falls back to an
in-memory cache instead of Redis (see next section) — you only need a
reachable Postgres database.

### 1. Start infrastructure

```bash
docker compose up -d postgres redis
```

If you don't have Docker, point `ConnectionStrings:Postgres` in
`backend/src/StudyVerse.Api/appsettings.Development.json` at any local
Postgres instance and create the database it names. Leave `Redis:ConnectionString`
empty (the default) to use the in-memory `ICacheService` fallback — fine for
local development, but OTP/lockout/rate-limit state won't survive a restart
or be shared across instances, so Staging/Production must always configure a
real Redis connection string.

### 2. Run the backend

```bash
cd backend
dotnet tool restore
dotnet ef database update -p src/StudyVerse.Infrastructure -s src/StudyVerse.Api
dotnet run --project src/StudyVerse.Api
```

The API listens on the port printed by `dotnet run` (see
`backend/src/StudyVerse.Api/Properties/launchSettings.json` — `5221` for the
`http` profile by default); Swagger UI is available at `/swagger` in
Development.

### 3. Run the mobile app

```bash
cd mobile
cp .env.example .env.development
npm install
npm run start   # or `npm run web` to run in a browser
```

Set `API_BASE_URL` in `mobile/.env.development` to match the port the backend
actually printed in step 2, e.g. `http://localhost:5221` — **without** the
`/api/v1` suffix (`src/api/client.ts` appends `/api/v1/auth` itself). Use
your machine's LAN IP instead of `localhost` if testing on a physical device
via Expo Go.

## CI

GitHub Actions runs on every PR and push to `main`:

- `.github/workflows/backend-ci.yml` — restore, build, `dotnet test` for the
  backend solution.
- `.github/workflows/mobile-ci.yml` — install, `tsc --noEmit`, `eslint` for
  the mobile app.

## Contributing conventions

- Follow the layering rules in [ARCHITECTURE.md](ARCHITECTURE.md) — Domain
  and Application never depend on Infrastructure or Api.
- Every new backend feature is a MediatR command/query with a
  FluentValidation validator and at least one unit test.
- Keep PRODUCT.md / ARCHITECTURE.md / ROADMAP.md current as phases land.

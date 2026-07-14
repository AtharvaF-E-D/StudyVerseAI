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
- Docker Desktop (for Postgres + Redis)
- Expo Go app or an Android/iOS simulator, for running the mobile app

### 1. Start infrastructure

```bash
docker compose up -d postgres redis
```

### 2. Run the backend

```bash
cd backend
dotnet tool restore
dotnet ef database update -p src/StudyVerse.Infrastructure -s src/StudyVerse.Api
dotnet run --project src/StudyVerse.Api
```

The API listens on the port printed by `dotnet run` (see
`backend/src/StudyVerse.Api/Properties/launchSettings.json`); Swagger UI is
available at `/swagger` in Development.

### 3. Run the mobile app

```bash
cd mobile
cp .env.example .env.development
npm install
npm run start
```

Set `API_BASE_URL` in `mobile/.env.development` to your machine's LAN IP
(not `localhost`) if testing on a physical device via Expo Go, e.g.
`http://192.168.1.20:5080/api/v1`.

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

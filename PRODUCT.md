# PRODUCT.md — StudyVerse AI

## Vision

Build the world's best AI-powered student learning platform: an AI tutor, doubt
solver, notes generator, flashcard system, quiz engine, mock test platform,
coding practice ground, interview prep coach, and current-affairs digest — all
wrapped in a gamified, personalized study experience that scales to millions
of users.

**Tagline:** Learn Smarter. Practice Faster. Achieve More.

## Target Users

- School and college students preparing for exams (competitive + academic).
- Job seekers preparing for technical/HR interviews.
- Self-learners who want AI-assisted study material generation from their own
  PDFs, notes, and slides.

## Core Product Pillars

1. **AI Tutor** — conversational tutor with voice, OCR, math/code rendering.
2. **AI Doubt Solver** — snap-a-photo or type a question, get a worked answer.
3. **AI Notes** — upload PDF/DOCX/PPT/images → summary, key points, flashcards,
   MCQs, mind maps, revision sheets.
4. **Flashcards** — AI-generated and manual decks with spaced repetition.
5. **Rapid Fire Quiz** — timed, gamified quiz mode with combos and power-ups.
6. **Mock Tests** — timed exams with AI evaluation, rank, percentile, weakness
   analysis.
7. **Study Planner** — exam-date-driven daily/weekly plans with smart
   rescheduling.
8. **Current Affairs** — daily/weekly digest with auto-generated quizzes.
9. **Coding Practice** — editor + compiler + AI hints + daily challenges.
10. **Interview Preparation** — HR/technical/behavioral, voice interviews,
    resume analysis, AI feedback and scoring.
11. **Gamification** — XP, coins, levels, badges, streaks, leaderboards,
    missions, seasonal events.
12. **Personalized AI Study Coach** — ties recommendations across all pillars
    into one dashboard.

## Non-Functional Requirements

- Production-ready, secure, modular, and horizontally scalable.
- Must support millions of users without architectural rewrites.
- Offline-tolerant mobile experience with background sync.
- GDPR-style data export/deletion workflows.

## Phased Delivery

See [ROADMAP.md](ROADMAP.md) for the full 21-phase breakdown, current phase
status, and what's deliberately deferred.

## Monetization

- Freemium core with subscription tiers (Razorpay + Google Play Billing).
- AdMob (banner/native/interstitial/rewarded) gated to never interrupt an
  active study session (quiz-in-progress, mock test, tutor conversation).
- Wallet, coupons, and referral program feed the coin economy used across
  gamification.

## Explicitly Out of Scope for v1

- iOS-first parity is not required at launch (Android + Play Store is the
  primary launch target given Razorpay/Play Billing emphasis); iOS follows
  once Apple sign-in and App Store review are budgeted for.
- Web admin portal (Phase 15) ships after the mobile app has a stable core
  loop (Phases 1–8), not before.

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the real "Gamification Hub" backend (base path
// `api/v1/gamification`), verified live against the running backend — every
// shape below was corrected against actual captured responses, replacing an
// earlier draft coded to the phase brief's contract shorthand before the
// backend was reachable. Real, confirmed deviations from that shorthand:
// - `GET /badges` wraps the list: `{ earnedCount, totalCount, badges: [...] }`,
//   not a bare array; each badge has `id`/`title`/`category` (a plain string
//   like "Quiz"/"Coding", not an icon name) — there is no `iconName` field.
// - `GET /missions` likewise wraps: `{ weekStartDateUtc, completedCount,
//   totalCount, missions: [...] }`, each mission has `id`, not
//   `missionTemplateId`.
// - Daily reward / spin status use `claimedToday`/`dayNumber`/`todayCoins`/
//   `todayXp`/`tomorrowCoins`/`tomorrowXp`/`activeSeasonalEventName`/
//   `activeSeasonalEventBonusCoins` and `spunToday`/`todaysPrizeLabel` — not
//   the `alreadyClaimedToday`/`consecutiveDayNumber`/`alreadySpunToday`
//   names guessed beforehand.
// - `GET /summary` uses `currentStreakDays`, not `currentStreak`.
// - `POST /daily-reward/claim` and `POST /spin` both also return
//   `newXpTotal`/`newCoinsTotal` (the user's post-award running totals) plus
//   `seasonalEventName`/`seasonalEventBonusCoins` on the claim response —
//   fields the original shorthand didn't anticipate at all, captured live
//   from an actual claim/spin round trip rather than guessed.
// `getBadges()`/`getMissions()` unwrap their list field for callers that just
// want the flat array to render.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// GET /gamification/badges  →  GetBadgesResponse
// ---------------------------------------------------------------------------

export interface BadgeDto {
  id: string;
  title: string;
  description: string;
  /** A plain category label (e.g. "Quiz", "Coding", "Streak") — there is no server-driven icon name; the render site picks a local Icon per category. */
  category: string;
  isEarned: boolean;
  earnedAtUtc: string | null;
}

export interface GetBadgesResponse {
  earnedCount: number;
  totalCount: number;
  badges: BadgeDto[];
}

export async function getBadges(): Promise<BadgeDto[]> {
  const { data } = await coreApiClient.get<GetBadgesResponse>("/gamification/badges");
  return data.badges;
}

// ---------------------------------------------------------------------------
// GET /gamification/missions  →  GetMissionsResponse
//
// The real endpoint wraps the mission list in an object with week metadata
// (weekStartDateUtc/completedCount/totalCount) rather than returning a bare
// array — verified against the live response, which also uses `id` for the
// mission's identifier, not `missionTemplateId`. `getMissions()` unwraps
// `.missions` for callers that just want the flat list to render.
// ---------------------------------------------------------------------------

export interface MissionDto {
  id: string;
  title: string;
  description: string;
  targetCount: number;
  currentCount: number;
  isCompleted: boolean;
  xpReward: number;
  coinReward: number;
}

export interface GetMissionsResponse {
  weekStartDateUtc: string;
  completedCount: number;
  totalCount: number;
  missions: MissionDto[];
}

export async function getMissions(): Promise<MissionDto[]> {
  const { data } = await coreApiClient.get<GetMissionsResponse>("/gamification/missions");
  return data.missions;
}

// ---------------------------------------------------------------------------
// GET /gamification/daily-reward/status  →  DailyRewardStatusDto
// ---------------------------------------------------------------------------

export interface DailyRewardStatusDto {
  claimedToday: boolean;
  dayNumber: number;
  todayCoins: number;
  todayXp: number;
  tomorrowCoins: number;
  tomorrowXp: number;
  /** Null when no seasonal event is currently active. */
  activeSeasonalEventName: string | null;
  activeSeasonalEventBonusCoins: number;
}

export async function getDailyRewardStatus(): Promise<DailyRewardStatusDto> {
  const { data } = await coreApiClient.get<DailyRewardStatusDto>("/gamification/daily-reward/status");
  return data;
}

// ---------------------------------------------------------------------------
// POST /gamification/daily-reward/claim  →  ClaimDailyRewardResponse
// ---------------------------------------------------------------------------

export interface ClaimDailyRewardResponse {
  dayNumber: number;
  coinsAwarded: number;
  xpAwarded: number;
  newXpTotal: number;
  newCoinsTotal: number;
  seasonalEventName: string | null;
  seasonalEventBonusCoins: number;
}

export async function claimDailyReward(): Promise<ClaimDailyRewardResponse> {
  const { data } = await coreApiClient.post<ClaimDailyRewardResponse>("/gamification/daily-reward/claim");
  return data;
}

// ---------------------------------------------------------------------------
// GET /gamification/spin/status  →  SpinStatusDto
// ---------------------------------------------------------------------------

export interface SpinStatusDto {
  spunToday: boolean;
  todaysPrizeLabel: string | null;
}

export async function getSpinStatus(): Promise<SpinStatusDto> {
  const { data } = await coreApiClient.get<SpinStatusDto>("/gamification/spin/status");
  return data;
}

// ---------------------------------------------------------------------------
// POST /gamification/spin  →  SpinResponse
// ---------------------------------------------------------------------------

export interface SpinResponse {
  prizeLabel: string;
  coinsAwarded: number;
  xpAwarded: number;
  newXpTotal: number;
  newCoinsTotal: number;
}

export async function spinWheel(): Promise<SpinResponse> {
  const { data } = await coreApiClient.post<SpinResponse>("/gamification/spin");
  return data;
}

// ---------------------------------------------------------------------------
// GET /gamification/summary  →  GamificationSummaryDto
// ---------------------------------------------------------------------------

export interface GamificationSummaryDto {
  level: number;
  xp: number;
  coins: number;
  currentStreakDays: number;
  badgesEarnedCount: number;
  totalBadgesCount: number;
  missionsCompletedThisWeek: number;
  totalMissionsThisWeek: number;
  dailyRewardStatus: DailyRewardStatusDto;
  spinStatus: SpinStatusDto;
}

export async function getGamificationSummary(): Promise<GamificationSummaryDto> {
  const { data } = await coreApiClient.get<GamificationSummaryDto>("/gamification/summary");
  return data;
}

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  claimDailyReward,
  getBadges,
  getDailyRewardStatus,
  getGamificationSummary,
  getMissions,
  getSpinStatus,
  spinWheel,
  type BadgeDto,
  type ClaimDailyRewardResponse,
  type DailyRewardStatusDto,
  type GamificationSummaryDto,
  type MissionDto,
  type SpinResponse,
  type SpinStatusDto,
} from "../api/gamification";

export const gamificationBadgesQueryKey = ["gamification", "badges"] as const;
export const gamificationMissionsQueryKey = ["gamification", "missions"] as const;
export const gamificationDailyRewardStatusQueryKey = ["gamification", "daily-reward", "status"] as const;
export const gamificationSpinStatusQueryKey = ["gamification", "spin", "status"] as const;
export const gamificationSummaryQueryKey = ["gamification", "summary"] as const;

/** Every badge the user can earn, flagged `isEarned` — drives the hub's badges grid. */
export function useBadgesQuery() {
  return useQuery<BadgeDto[]>({
    queryKey: gamificationBadgesQueryKey,
    queryFn: getBadges,
  });
}

/** Active mission templates with the user's current progress toward each. */
export function useMissionsQuery() {
  return useQuery<MissionDto[]>({
    queryKey: gamificationMissionsQueryKey,
    queryFn: getMissions,
  });
}

/** Whether today's daily reward has been claimed yet, plus today's preview amounts. */
export function useDailyRewardStatusQuery() {
  return useQuery<DailyRewardStatusDto>({
    queryKey: gamificationDailyRewardStatusQueryKey,
    queryFn: getDailyRewardStatus,
  });
}

/** Whether the wheel has already been spun today. */
export function useSpinStatusQuery() {
  return useQuery<SpinStatusDto>({
    queryKey: gamificationSpinStatusQueryKey,
    queryFn: getSpinStatus,
  });
}

/** Aggregate level/xp/coins/streak + badge/mission/reward snapshot for the hub's summary strip. */
export function useGamificationSummaryQuery() {
  return useQuery<GamificationSummaryDto>({
    queryKey: gamificationSummaryQueryKey,
    queryFn: getGamificationSummary,
  });
}

/**
 * Claims today's daily reward via a real request. The shared `onSuccess`
 * here refetches every query whose data the claim just changed server-side
 * (claim status itself, the summary strip's coins/xp/level, and
 * badges/missions — claiming could complete a "claim N days in a row"
 * mission or unlock a streak badge) so every screen reflects the server's
 * authoritative post-claim state rather than a guess.
 *
 * That refetch-on-invalidate alone is NOT enough to drive the card's
 * celebratory "+coins/+xp" reveal, though — the caller (the hub screen) also
 * passes its own `onSuccess` at the `.mutate()` call site to push the
 * returned `coinsAwarded`/`xpAwarded` into local state. Calling this out
 * explicitly: a claim/spin mutation with no `onSuccess` wired to local state
 * is exactly the bug class this phase's brief flagged as having been found
 * twice in a row in this exact codebase — every mutation below is written to
 * avoid it.
 */
export function useClaimDailyRewardMutation() {
  const queryClient = useQueryClient();

  return useMutation<ClaimDailyRewardResponse, unknown, void>({
    mutationFn: () => claimDailyReward(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: gamificationDailyRewardStatusQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationSummaryQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationMissionsQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationBadgesQueryKey });
    },
  });
}

/**
 * Spins the wheel via a real request. Same reasoning as
 * `useClaimDailyRewardMutation` above: this shared `onSuccess` keeps every
 * OTHER screen's cache fresh (spin status, summary, missions, badges), while
 * the hub screen's own `.mutate()` call site supplies the `onSuccess` that
 * actually feeds the returned prize into the wheel's local reveal state.
 */
export function useSpinMutation() {
  const queryClient = useQueryClient();

  return useMutation<SpinResponse, unknown, void>({
    mutationFn: () => spinWheel(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: gamificationSpinStatusQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationSummaryQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationMissionsQueryKey });
      void queryClient.invalidateQueries({ queryKey: gamificationBadgesQueryKey });
    },
  });
}

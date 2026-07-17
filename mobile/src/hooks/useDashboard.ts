import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  completeChallenge,
  getDashboard,
  markNotificationRead,
  type CompleteChallengeResponse,
  type DashboardResponse,
} from "../api/dashboard";

/** Shared query key for the dashboard snapshot — reused by both hooks below so a completed challenge or a read notification invalidates the same cache entry. */
export const dashboardQueryKey = ["dashboard"] as const;

/** Fetches the authenticated user's dashboard snapshot (xp, streak, challenges, weekly activity, leaderboard, notifications). */
export function useDashboardQuery() {
  return useQuery<DashboardResponse>({
    queryKey: dashboardQueryKey,
    queryFn: getDashboard,
  });
}

/**
 * Marks a today's-challenge complete, then refetches the dashboard so xp/
 * coins/level and the challenge's `isCompleted` flag reflect the server's
 * authoritative result rather than an optimistic guess.
 */
export function useCompleteChallengeMutation() {
  const queryClient = useQueryClient();

  return useMutation<CompleteChallengeResponse, unknown, string>({
    mutationFn: (challengeId: string) => completeChallenge(challengeId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: dashboardQueryKey });
    },
  });
}

/** Marks a notification read, then refetches the dashboard so `unreadCount` and the notification's `readAtUtc` reflect the server's result. */
export function useMarkNotificationReadMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (notificationId: string) => markNotificationRead(notificationId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: dashboardQueryKey });
    },
  });
}

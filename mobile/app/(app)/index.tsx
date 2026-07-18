import React, { useState } from "react";
import { Alert } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { ErrorState } from "../../src/components/ErrorState";
import { DashboardContent } from "../../src/components/dashboard/DashboardContent";
import { DashboardSkeleton } from "../../src/components/dashboard/DashboardSkeleton";
import { useAuthStore } from "../../src/stores/authStore";
import { logout as logoutRequest } from "../../src/api/auth";
import { getOrCreateDeviceId } from "../../src/lib/storage";
import { useToast } from "../../src/lib/toast";
import {
  useCompleteChallengeMutation,
  useDashboardQuery,
  useMarkNotificationReadMutation,
} from "../../src/hooks/useDashboard";
import { useFlashcardStatsQuery } from "../../src/hooks/useFlashcards";

/**
 * Authenticated home screen: streak/xp/coins summary, today's challenges,
 * weekly activity, a leaderboard preview, recent notifications, and an
 * honest placeholder for "continue learning" (no feature produces that data
 * until later phases). All layout lives in `DashboardContent` so the dev
 * preview at `app/(dev)/dashboard-preview.tsx` can render the exact same UI
 * against mocked fixtures.
 */
export default function AppHomeScreen() {
  const user = useAuthStore((s) => s.user);
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const clearSession = useAuthStore((s) => s.clearSession);
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const { show } = useToast();

  const dashboardQuery = useDashboardQuery();
  const flashcardStatsQuery = useFlashcardStatsQuery();
  const completeChallengeMutation = useCompleteChallengeMutation();
  const markNotificationReadMutation = useMarkNotificationReadMutation();

  async function handleLogout() {
    setIsLoggingOut(true);
    try {
      if (refreshToken) {
        await logoutRequest({ refreshToken, deviceId: getOrCreateDeviceId() });
      }
    } catch {
      // Best-effort: the server-side session may already be gone, or the
      // network may be unreachable. Either way we still clear locally so
      // the user isn't stuck signed in on this device.
    } finally {
      clearSession();
      setIsLoggingOut(false);
      router.replace("/(auth)/login");
    }
  }

  function confirmLogout() {
    Alert.alert("Log out", "Are you sure you want to log out?", [
      { text: "Cancel", style: "cancel" },
      { text: "Log out", style: "destructive", onPress: handleLogout },
    ]);
  }

  function handleCompleteChallenge(challengeId: string) {
    if (completeChallengeMutation.isPending) return;
    completeChallengeMutation.mutate(challengeId, {
      onSuccess: (result) => {
        show(`+${result.xpAwarded} XP, +${result.coinsAwarded} coins!`, "success");
      },
      onError: () => {
        show("Couldn't complete that challenge. Try again.", "danger");
      },
    });
  }

  function handleMarkNotificationRead(notificationId: string) {
    if (markNotificationReadMutation.isPending) return;
    markNotificationReadMutation.mutate(notificationId);
  }

  return (
    <ScreenContainer>
      {dashboardQuery.isLoading ? (
        <DashboardSkeleton />
      ) : dashboardQuery.isError || !dashboardQuery.data ? (
        <ErrorState
          title="Couldn't load your dashboard"
          description="Check your connection and try again."
          onRetry={() => void dashboardQuery.refetch()}
        />
      ) : (
        <DashboardContent
          displayName={user?.displayName ?? "there"}
          data={dashboardQuery.data}
          currentUserId={user?.id}
          isLoggingOut={isLoggingOut}
          onLogout={confirmLogout}
          onCompleteChallenge={handleCompleteChallenge}
          completingChallengeId={
            completeChallengeMutation.isPending ? (completeChallengeMutation.variables ?? null) : null
          }
          onMarkNotificationRead={handleMarkNotificationRead}
          markingNotificationId={
            markNotificationReadMutation.isPending ? (markNotificationReadMutation.variables ?? null) : null
          }
          flashcardsDueToday={flashcardStatsQuery.data?.dueToday ?? 0}
        />
      )}
    </ScreenContainer>
  );
}

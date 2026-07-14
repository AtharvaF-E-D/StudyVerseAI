import React, { useState } from "react";
import { Alert, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { useAuthStore } from "../../src/stores/authStore";
import { logout as logoutRequest } from "../../src/api/auth";
import { getDeviceName, getOrCreateDeviceId } from "../../src/lib/storage";

/**
 * Placeholder authenticated home screen. It only needs to prove the auth
 * flow works end-to-end (show the logged-in user and allow logging out) —
 * the real dashboard is built in Phase 3.
 */
export default function AppHomeScreen() {
  const user = useAuthStore((s) => s.user);
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const clearSession = useAuthStore((s) => s.clearSession);
  const [isLoggingOut, setIsLoggingOut] = useState(false);

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

  return (
    <ScreenContainer scrollable={false}>
      <View className="flex-1 items-center justify-center px-6">
        <Text className="mb-2 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
          Welcome to StudyVerse AI
        </Text>
        <Text className="mb-1 text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
          Signed in as
        </Text>
        <Text className="mb-8 text-center text-subheading font-semibold text-brand dark:text-brand-light">
          {user?.email ?? "unknown user"}
        </Text>
        {user && !user.emailVerified ? (
          <Text className="mb-8 text-center text-caption text-warning">
            Your email isn&apos;t verified yet.
          </Text>
        ) : null}
        <Text className="mb-8 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Device: {getDeviceName()}
        </Text>
        <Button
          title="Log out"
          variant="secondary"
          onPress={() => {
            Alert.alert("Log out", "Are you sure you want to log out?", [
              { text: "Cancel", style: "cancel" },
              { text: "Log out", style: "destructive", onPress: handleLogout },
            ]);
          }}
          loading={isLoggingOut}
          fullWidth={false}
        />
      </View>
    </ScreenContainer>
  );
}

import React from "react";
import { ActivityIndicator, View } from "react-native";
import { Redirect } from "expo-router";

import { useAuthStore, useIsAuthenticated } from "../src/stores/authStore";

/**
 * Entry route: waits for the auth store to finish reading any persisted
 * session from MMKV, then redirects to the authenticated app shell or the
 * unauthenticated splash/onboarding flow. In practice the root layout keeps
 * the native splash screen visible until hydration completes, so the
 * loading branch below is only a defensive fallback.
 */
export default function Index() {
  const isHydrated = useAuthStore((s) => s.isHydrated);
  const isAuthenticated = useIsAuthenticated();

  if (!isHydrated) {
    return (
      <View className="flex-1 items-center justify-center bg-background dark:bg-background-dark">
        <ActivityIndicator />
      </View>
    );
  }

  return <Redirect href={isAuthenticated ? "/(app)" : "/(auth)/splash"} />;
}

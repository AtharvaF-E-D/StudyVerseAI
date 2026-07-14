import React, { useEffect } from "react";
import { ActivityIndicator, Text, View } from "react-native";
import { router } from "expo-router";

import { appStorage } from "../../src/lib/storage";

const ONBOARDING_SEEN_KEY = "studyverse.hasSeenOnboarding";

/**
 * Brief branded splash shown for unauthenticated users while we decide
 * whether they've seen onboarding before (persisted in MMKV). This route
 * itself is only reached after the native splash screen has already been
 * hidden by the root layout, so it renders a lightweight in-app splash
 * rather than a blank screen during that decision.
 */
export default function AuthSplashScreen() {
  useEffect(() => {
    const hasSeenOnboarding = appStorage.getBoolean(ONBOARDING_SEEN_KEY) ?? false;
    const timer = setTimeout(() => {
      router.replace(hasSeenOnboarding ? "/(auth)/login" : "/(auth)/onboarding");
    }, 350);
    return () => clearTimeout(timer);
  }, []);

  return (
    <View className="flex-1 items-center justify-center bg-brand">
      <Text className="text-display font-bold text-white">StudyVerse AI</Text>
      <Text className="mt-2 text-body text-white/80">Learn smarter, together.</Text>
      <ActivityIndicator color="#FFFFFF" style={{ marginTop: 24 }} />
    </View>
  );
}

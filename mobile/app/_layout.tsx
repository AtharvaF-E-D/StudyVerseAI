import "../global.css";

import React, { useEffect } from "react";
import { QueryClientProvider } from "@tanstack/react-query";
import { Stack } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { StatusBar } from "expo-status-bar";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { queryClient } from "../src/lib/queryClient";
import { useAuthStore } from "../src/stores/authStore";
import { ThemeProvider, useTheme } from "../src/theme/ThemeProvider";

// Keep the native splash screen up until auth-state hydration (reading any
// persisted session from MMKV) has finished, so `app/index.tsx` never has
// to render a "flash of the wrong screen" before it can decide where to
// route the user.
void SplashScreen.preventAutoHideAsync();

function RootNavigator() {
  const { scheme } = useTheme();
  const isHydrated = useAuthStore((s) => s.isHydrated);

  useEffect(() => {
    if (isHydrated) {
      void SplashScreen.hideAsync();
    }
  }, [isHydrated]);

  if (!isHydrated) {
    return null;
  }

  return (
    <>
      <StatusBar style={scheme === "dark" ? "light" : "dark"} />
      <Stack screenOptions={{ headerShown: false }} />
    </>
  );
}

export default function RootLayout() {
  return (
    <QueryClientProvider client={queryClient}>
      <SafeAreaProvider>
        <ThemeProvider>
          <RootNavigator />
        </ThemeProvider>
      </SafeAreaProvider>
    </QueryClientProvider>
  );
}

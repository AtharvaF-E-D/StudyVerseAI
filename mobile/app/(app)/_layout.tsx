import { Redirect, Stack } from "expo-router";

import { useIsAuthenticated } from "../../src/stores/authStore";

/**
 * Auth-gated layout for everything behind login. `isHydrated` is
 * guaranteed true by the time any route under `(app)` mounts (the root
 * layout keeps the native splash screen up until hydration finishes), so
 * this only needs to check whether a session is actually present.
 */
export default function AppLayout() {
  const isAuthenticated = useIsAuthenticated();

  if (!isAuthenticated) {
    return <Redirect href="/(auth)/login" />;
  }

  return <Stack screenOptions={{ headerShown: false }} />;
}

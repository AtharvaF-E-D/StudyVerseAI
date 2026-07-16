import { Stack } from "expo-router";

/**
 * Route group for internal/manual-QA screens only — never linked from app
 * navigation. See `components.tsx` for the design-system showcase screen.
 */
export default function DevLayout() {
  return <Stack screenOptions={{ headerShown: false }} />;
}

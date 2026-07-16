/**
 * Motion tokens shared across `src/components`.
 *
 * Guideline: keep micro-interactions (press feedback, toggles, chip
 * selection) under 300ms; use 300-500ms for screen-level transitions
 * (modals, screen entrances, toasts). Entrances should always ease OUT
 * (fast start, slow settle — `Easing.out(...)`) so content feels like it's
 * arriving under its own motion; transitions between two settled states
 * (e.g. a tab switch or a layout resize) should ease IN-OUT so both ends of
 * the motion feel deliberate. Never use `Easing.linear` for UI motion — it
 * reads as mechanical.
 *
 * Every helper here respects the OS-level "reduce motion" accessibility
 * setting. The `fadeInUp`/`fadeOutDown` entering/exiting builders do this
 * automatically (Reanimated defaults every layout animation builder to
 * `ReduceMotion.System`). For components driven by a manual `useSharedValue`
 * + `withTiming` (press feedback, progress bars, toasts, skeletons), use the
 * `useReduceMotion()` hook below to collapse those to an instant/no-op state.
 */
import { useEffect, useState } from "react";
import { AccessibilityInfo } from "react-native";
import { Easing, FadeInUp, FadeOutDown, type WithTimingConfig } from "react-native-reanimated";

/** Named durations, in ms. */
export const durations = {
  fast: 150,
  base: 250,
  slow: 400,
} as const;

/** Reanimated easing presets. */
export const easings = {
  /** Entrances: fast start, slow settle. */
  entrance: Easing.out(Easing.cubic),
  /** Exits: slow start, fast finish — mirrors `entrance`. */
  exit: Easing.in(Easing.cubic),
  /** Transitions between two settled states (layout/tab changes). */
  standard: Easing.inOut(Easing.cubic),
} as const;

export const timingConfigs: Record<"fast" | "base" | "slow", WithTimingConfig> = {
  fast: { duration: durations.fast, easing: easings.standard },
  base: { duration: durations.base, easing: easings.standard },
  slow: { duration: durations.slow, easing: easings.standard },
};

/**
 * Tracks the OS "reduce motion" preference. Starts `false` (matches most
 * users/devices) and flips once `AccessibilityInfo` resolves, then stays in
 * sync if the setting changes while the app is open.
 */
export function useReduceMotion(): boolean {
  const [isReduceMotionEnabled, setIsReduceMotionEnabled] = useState(false);

  useEffect(() => {
    let isMounted = true;
    AccessibilityInfo.isReduceMotionEnabled().then((enabled) => {
      if (isMounted) setIsReduceMotionEnabled(enabled);
    });
    const subscription = AccessibilityInfo.addEventListener("reduceMotionChanged", setIsReduceMotionEnabled);
    return () => {
      isMounted = false;
      subscription.remove();
    };
  }, []);

  return isReduceMotionEnabled;
}

/**
 * A Reanimated `entering` config for content fading/sliding up into place —
 * the standard entrance for cards, list items, and toasts. Pass a `delay`
 * (ms) to stagger a list of these. Reanimated's layout animation builders
 * already respect the OS reduce-motion setting by default (`ReduceMotion.System`),
 * so no extra gating is needed for anything built with `entering`/`exiting` —
 * that's only required for the hand-rolled `useSharedValue` + `withTiming`
 * animations elsewhere in this library (see `useReduceMotion` below).
 */
export function fadeInUp(delay = 0) {
  return FadeInUp.duration(durations.base).easing(easings.entrance).delay(delay);
}

/** Matching exit for `fadeInUp`, used when a component is removed from the tree. */
export function fadeOutDown(delay = 0) {
  return FadeOutDown.duration(durations.fast).easing(easings.exit).delay(delay);
}

import React from "react";
import { Text, View } from "react-native";
import Animated from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Button } from "../Button";
import { Card } from "../Card";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { fadeInUp, useReduceMotion } from "../../theme/motion";
import { SpinWheel } from "./SpinWheel";
import type { SpinResponse, SpinStatusDto } from "../../api/gamification";

cssInterop(Animated.View, { className: "style" });

export type SpinPhase = "idle" | "spinning" | "landing" | "revealed";

export interface SpinWheelCardProps {
  status: SpinStatusDto;
  phase: SpinPhase;
  /** The real `POST /spin` result, once it has arrived — `null` until then (and while `spinFailed`). */
  spinResult: SpinResponse | null;
  /** True if the in-flight spin request failed — the wheel still lands (so the interaction doesn't just freeze), but the reveal panel shows a retry prompt instead of a prize. */
  spinFailed: boolean;
  landingIndex: number | null;
  onSpin: () => void;
  onLandingComplete: () => void;
  className?: string;
}

/**
 * "Spin the wheel" card: the wheel itself (see `SpinWheel.tsx` for the
 * animation + why it's a segmented ring rather than a literal pie chart),
 * plus the surrounding state — a "Spin the wheel!" CTA, an in-progress
 * caption while the real request/animation are running, an unambiguous
 * prize reveal once the animation lands, and a disabled "come back
 * tomorrow" state both immediately after spinning and on next visit before
 * the daily reset (driven by `status.spunToday`, refetched after a
 * successful spin).
 */
export function SpinWheelCard({
  status,
  phase,
  spinResult,
  spinFailed,
  landingIndex,
  onSpin,
  onLandingComplete,
  className = "",
}: SpinWheelCardProps) {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();

  const alreadyDoneToday = status.spunToday || phase === "revealed";
  const isBusy = phase === "spinning" || phase === "landing";

  return (
    <Card className={className}>
      <View className="mb-4 flex-row items-center">
        <Icon name="game-controller" size={20} color={colors.brand} />
        <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Spin the wheel</Text>
      </View>

      <SpinWheel
        isSpinning={phase === "spinning"}
        landingIndex={landingIndex}
        onLandingComplete={onLandingComplete}
        className="mb-4"
      />

      {phase === "revealed" ? (
        <Animated.View entering={reduceMotion ? undefined : fadeInUp()}>
          {spinFailed ? (
            <View className="mb-4 items-center rounded-lg bg-danger/10 px-4 py-3">
              <Text className="text-center text-body font-semibold text-danger">Couldn&apos;t spin the wheel</Text>
              <Text className="mt-0.5 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
                Check your connection and try again.
              </Text>
            </View>
          ) : (
            <View className="mb-4 items-center rounded-lg bg-brand/10 px-4 py-3 dark:bg-brand-light/10">
              <Text className="text-body font-semibold text-brand dark:text-brand-light">
                {spinResult?.prizeLabel}
              </Text>
              <Text className="mt-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                +{spinResult?.xpAwarded} XP · +{spinResult?.coinsAwarded} coins
              </Text>
            </View>
          )}
        </Animated.View>
      ) : isBusy ? (
        <Text className="mb-4 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Spinning…
        </Text>
      ) : null}

      {alreadyDoneToday && !spinFailed ? (
        <Button title="Come back tomorrow" variant="secondary" disabled onPress={() => {}} />
      ) : (
        <Button
          title={spinFailed ? "Try again" : "Spin the wheel!"}
          loading={isBusy}
          disabled={isBusy}
          onPress={onSpin}
        />
      )}
    </Card>
  );
}

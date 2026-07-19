import React, { useEffect } from "react";
import { Text, View } from "react-native";
import Animated, { useAnimatedStyle, useSharedValue, withSequence, withTiming } from "react-native-reanimated";
import { cssInterop } from "nativewind";

import { Badge } from "../Badge";
import { Button } from "../Button";
import { Card } from "../Card";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { fadeInUp, useReduceMotion } from "../../theme/motion";
import type { ClaimDailyRewardResponse, DailyRewardStatusDto } from "../../api/gamification";

cssInterop(Animated.View, { className: "style" });

export interface DailyRewardCardProps {
  status: DailyRewardStatusDto;
  /** Set once `POST /daily-reward/claim` has actually returned — drives the celebratory reveal below the preview. */
  claimResult: ClaimDailyRewardResponse | null;
  isClaiming: boolean;
  onClaim: () => void;
  className?: string;
}

/**
 * "Claim daily reward" card. Shows today's preview amounts and a streak
 * counter while unclaimed; once claimed (`status.claimedToday` from a
 * refetch, OR `claimResult` from the just-completed mutation — whichever is
 * available first, so the disabled state appears the instant the response
 * lands rather than waiting on a second round trip) it disables the button
 * and shows what was actually awarded, with a small scale/fade pop so the
 * coin/xp gain reads as a reward rather than a static label change.
 */
export function DailyRewardCard({ status, claimResult, isClaiming, onClaim, className = "" }: DailyRewardCardProps) {
  const { colors } = useTheme();
  const reduceMotion = useReduceMotion();
  const isClaimed = status.claimedToday || claimResult !== null;

  const scale = useSharedValue(1);
  useEffect(() => {
    if (claimResult && !reduceMotion) {
      scale.value = withSequence(withTiming(1.15, { duration: 160 }), withTiming(1, { duration: 220 }));
    }
  }, [claimResult, reduceMotion, scale]);
  const popStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  const coins = claimResult?.coinsAwarded ?? status.todayCoins;
  const xp = claimResult?.xpAwarded ?? status.todayXp;
  const streakDay = claimResult?.dayNumber ?? status.dayNumber;

  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-center justify-between">
        <View className="flex-row items-center">
          <Icon name="calendar-number" size={20} color={colors.brand} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Daily reward</Text>
        </View>
        <Badge label={`Day ${streakDay}`} variant="brand" />
      </View>

      <Animated.View style={popStyle} className="mb-4 flex-row items-center justify-center rounded-lg bg-surface py-4 dark:bg-surface-dark">
        <View className="items-center px-4">
          <Icon name="star" size={22} color={colors.warning} />
          <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">+{xp}</Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">XP</Text>
        </View>
        <View className="items-center px-4">
          <Icon name="cash" size={22} color={colors.accent} />
          <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">+{coins}</Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">coins</Text>
        </View>
      </Animated.View>

      {isClaimed ? (
        <Animated.View entering={reduceMotion ? undefined : fadeInUp()} className="flex-row items-center justify-center rounded-lg bg-success/10 py-3">
          <Icon name="checkmark-circle" size={18} color={colors.success} />
          <Text className="ml-2 text-body font-semibold text-success">Claimed — see you tomorrow!</Text>
        </Animated.View>
      ) : (
        <Button title="Claim daily reward" loading={isClaiming} disabled={isClaiming} onPress={onClaim} />
      )}
    </Card>
  );
}

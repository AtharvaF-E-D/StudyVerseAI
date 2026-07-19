import React from "react";
import { Text, View } from "react-native";

import { Icon } from "../Icon";
import { ProgressBar } from "../ProgressBar";
import { useTheme } from "../../theme/ThemeProvider";
import type { MissionDto } from "../../api/gamification";

export interface MissionRowProps {
  mission: MissionDto;
  className?: string;
}

/** One active mission: title/description, a readable progress bar, and a checkmark once complete instead of a bar that's merely full. */
export function MissionRow({ mission, className = "" }: MissionRowProps) {
  const { colors } = useTheme();
  const progress = mission.targetCount > 0 ? mission.currentCount / mission.targetCount : 0;

  return (
    <View className={["px-3 py-3", className].join(" ")}>
      <View className="mb-1.5 flex-row items-start justify-between">
        <View className="flex-1 pr-3">
          <Text className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">{mission.title}</Text>
          <Text className="mt-0.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {mission.description}
          </Text>
        </View>
        {mission.isCompleted ? (
          <Icon name="checkmark-circle" size={22} color={colors.success} />
        ) : (
          <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
            {mission.currentCount}/{mission.targetCount}
          </Text>
        )}
      </View>
      <ProgressBar
        value={mission.isCompleted ? 1 : progress}
        accessibilityLabel={`${mission.title} progress`}
        className="mb-1.5"
        fillClassName={mission.isCompleted ? "bg-success" : undefined}
      />
      <View className="flex-row items-center">
        <Icon name="star" size={13} color={colors.warning} />
        <Text className="ml-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          +{mission.xpReward} XP
        </Text>
        <Icon name="cash" size={13} color={colors.accent} className="ml-3" />
        <Text className="ml-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          +{mission.coinReward} coins
        </Text>
      </View>
    </View>
  );
}

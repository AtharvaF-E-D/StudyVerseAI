import React from "react";
import { Text, View } from "react-native";

import { Card } from "../Card";
import { Icon, type IconName } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import type { QuizSessionSummaryDto } from "../../api/quiz";

export interface QuizSessionSummaryCardProps {
  summary: QuizSessionSummaryDto;
  className?: string;
}

function SummaryTile({
  icon,
  value,
  label,
  color,
}: {
  icon: IconName;
  value: string;
  label: string;
  color: string;
}) {
  return (
    <View className="flex-1 items-center">
      <Icon name={icon} size={22} color={color} />
      <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">{value}</Text>
      <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{label}</Text>
    </View>
  );
}

/**
 * Post-session recap card shown at the end of the play screen's game loop.
 * `summary` has no `accuracy` field on the wire (see `QuizSessionSummaryDto`),
 * so it's derived here from `correctAnswers / totalQuestions`.
 */
export function QuizSessionSummaryCard({ summary, className = "" }: QuizSessionSummaryCardProps) {
  const { colors } = useTheme();
  const accuracyPct =
    summary.totalQuestions > 0 ? Math.round((summary.correctAnswers / summary.totalQuestions) * 100) : 0;
  const bonusXp = summary.dailyChallengeBonusXp;
  const bonusCoins = summary.dailyChallengeBonusCoins;

  return (
    <Card className={className}>
      <Text className="mb-1 text-center text-heading text-ink-primary dark:text-ink-primary-dark">
        Session complete!
      </Text>
      <Text className="mb-4 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
        {summary.ranOutOfLives ? "Out of lives" : "All questions answered"}
      </Text>
      <View className="flex-row items-center justify-between">
        <SummaryTile icon="star" value={`${summary.xpEarned}`} label="XP" color={colors.brand} />
        <SummaryTile icon="cash-outline" value={`${summary.coinsEarned}`} label="Coins" color={colors.accent} />
        <SummaryTile icon="checkmark-done-outline" value={`${accuracyPct}%`} label="Accuracy" color={colors.success} />
        <SummaryTile icon="flame" value={`${summary.bestCombo}`} label="Best combo" color={colors.warning} />
      </View>
      <Text className="mt-4 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
        {summary.correctAnswers} of {summary.totalQuestions} correct · Score {summary.score}
      </Text>
      {bonusXp > 0 || bonusCoins > 0 ? (
        <Text className="mt-1 text-center text-caption font-medium text-warning">
          +{bonusXp} XP, +{bonusCoins} coins — daily challenge bonus
        </Text>
      ) : null}
    </Card>
  );
}

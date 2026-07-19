import React from "react";
import { Text, View } from "react-native";

import { Button } from "../Button";
import { Card } from "../Card";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { INTERVIEW_CATEGORY_DESCRIPTIONS, INTERVIEW_CATEGORY_ICONS, INTERVIEW_CATEGORY_LABELS } from "./category";
import type { InterviewCategory } from "../../api/interviewprep";

export interface InterviewCategoryStartCardProps {
  category: InterviewCategory;
  /** Average past score for this category, when the stats call has resolved — omitted while loading/unavailable. */
  averageScore?: number;
  starting: boolean;
  onStart: (category: InterviewCategory) => void;
  className?: string;
}

/** One HR/Technical/Behavioral "start a new session" card on the picker screen, and in the dev fixture preview. */
export function InterviewCategoryStartCard({
  category,
  averageScore,
  starting,
  onStart,
  className = "",
}: InterviewCategoryStartCardProps) {
  const { colors } = useTheme();

  return (
    <Card className={className}>
      <View className="mb-3 flex-row items-center">
        <View className="mr-3 h-10 w-10 items-center justify-center rounded-full bg-brand/10 dark:bg-brand-light/10">
          <Icon name={INTERVIEW_CATEGORY_ICONS[category]} size={20} color={colors.brand} />
        </View>
        <View className="flex-1">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {INTERVIEW_CATEGORY_LABELS[category]}
          </Text>
          {averageScore !== undefined ? (
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Average score: {Math.round(averageScore)}
            </Text>
          ) : null}
        </View>
      </View>
      <Text className="mb-4 text-body text-ink-secondary dark:text-ink-secondary-dark">
        {INTERVIEW_CATEGORY_DESCRIPTIONS[category]}
      </Text>
      <Button title="Start session" loading={starting} disabled={starting} onPress={() => onStart(category)} />
    </Card>
  );
}

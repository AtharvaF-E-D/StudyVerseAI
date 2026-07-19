import React from "react";
import { Text, View } from "react-native";

import { Card } from "../Card";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import type { ResumeAnalysisDto } from "../../api/interviewprep";

function BulletList({ items, color }: { items: string[]; color: string }) {
  return (
    <View>
      {items.map((item, index) => (
        <View key={index} className="mb-2 flex-row items-start">
          <View className="mr-2 mt-2 h-1.5 w-1.5 rounded-full" style={{ backgroundColor: color }} />
          <Text className="flex-1 text-body text-ink-primary dark:text-ink-primary-dark">{item}</Text>
        </View>
      ))}
    </View>
  );
}

export interface ResumeAnalysisSectionsProps {
  analysis: ResumeAnalysisDto;
  className?: string;
}

/**
 * Overall score + strengths/weaknesses/suggestions, each in its own
 * visually-distinct tinted `Card` — green for strengths, amber for
 * weaknesses, brand-tinted for suggestions, per the phase brief. Shared
 * between the real resume analysis screen
 * (`app/(app)/interview/resume/[analysisId].tsx`) and
 * `app/(dev)/interview-preview.tsx`'s fixtures, so both render identically.
 */
export function ResumeAnalysisSections({ analysis, className = "" }: ResumeAnalysisSectionsProps) {
  const { colors } = useTheme();

  return (
    <View className={className}>
      <Card className="mb-5 items-center">
        <Icon name="document-text" size={28} color={colors.brand} />
        <Text className="mt-2 text-display text-ink-primary dark:text-ink-primary-dark">
          {Math.round(analysis.overallScore)}
        </Text>
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Overall resume score</Text>
      </Card>

      <Card className="mb-5 border-success bg-success/5">
        <View className="mb-3 flex-row items-center">
          <Icon name="checkmark-circle" size={20} color={colors.success} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Strengths</Text>
        </View>
        {analysis.strengths.length > 0 ? (
          <BulletList items={analysis.strengths} color={colors.success} />
        ) : (
          <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
            No specific strengths were called out.
          </Text>
        )}
      </Card>

      <Card className="mb-5 border-warning bg-warning/5">
        <View className="mb-3 flex-row items-center">
          <Icon name="alert-circle" size={20} color={colors.warning} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Areas to improve
          </Text>
        </View>
        {analysis.weaknesses.length > 0 ? (
          <BulletList items={analysis.weaknesses} color={colors.warning} />
        ) : (
          <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
            No specific weaknesses were called out.
          </Text>
        )}
      </Card>

      <Card className="border-brand bg-brand/5 dark:border-brand-light dark:bg-brand-light/10">
        <View className="mb-3 flex-row items-center">
          <Icon name="bulb" size={20} color={colors.brand} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Suggestions</Text>
        </View>
        {analysis.suggestions.length > 0 ? (
          <BulletList items={analysis.suggestions} color={colors.brand} />
        ) : (
          <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
            No specific suggestions were given.
          </Text>
        )}
      </Card>
    </View>
  );
}

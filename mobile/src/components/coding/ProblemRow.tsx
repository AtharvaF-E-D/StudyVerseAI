import React from "react";
import { View } from "react-native";

import { Badge } from "../Badge";
import { Icon } from "../Icon";
import { ListItem } from "../ListItem";
import { useTheme } from "../../theme/ThemeProvider";
import { DIFFICULTY_BADGE_VARIANT, DIFFICULTY_LABELS } from "./difficulty";
import type { ProblemSummaryDto } from "../../api/codingpractice";

export interface ProblemRowProps {
  problem: ProblemSummaryDto;
  onPress: (problem: ProblemSummaryDto) => void;
  className?: string;
}

/**
 * One problem row, shared by the problems list screen and the dev fixture
 * preview. The solved checkmark and difficulty/interview `Badge`s are both
 * non-interactive, so — unlike `ArticleRow`'s bookmark star — there's no
 * nested-`<button>` risk in passing them straight into `ListItem`'s
 * `trailing` slot alongside its own `onPress`; see
 * `app/(app)/currentaffairs/index.tsx`'s `ArticleRow` for the pattern this
 * deliberately doesn't need (that one nests an actually-interactive
 * `Pressable`, which does need to be a sibling instead).
 */
export function ProblemRow({ problem, onPress, className = "" }: ProblemRowProps) {
  const { colors } = useTheme();

  return (
    <ListItem
      leading={
        <Icon
          name={problem.isSolved ? "checkmark-circle" : "ellipse-outline"}
          size={22}
          color={problem.isSolved ? colors.success : colors.textSecondary}
        />
      }
      title={problem.title}
      subtitle={problem.category}
      trailing={
        <View className="items-end gap-1">
          <Badge
            label={DIFFICULTY_LABELS[problem.difficulty]}
            variant={DIFFICULTY_BADGE_VARIANT[problem.difficulty]}
          />
          {problem.isInterviewQuestion ? <Badge label="Interview" variant="brand" /> : null}
        </View>
      }
      onPress={() => onPress(problem)}
      className={className}
    />
  );
}

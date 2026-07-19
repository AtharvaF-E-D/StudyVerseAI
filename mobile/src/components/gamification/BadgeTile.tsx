import React from "react";
import { Text, View } from "react-native";

import { Icon, type IconName } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import { formatRelativeTime } from "../../lib/relativeTime";
import type { BadgeDto } from "../../api/gamification";

export interface BadgeTileProps {
  badge: BadgeDto;
  className?: string;
}

/**
 * The real backend has no icon-name field on a badge — just a plain
 * `category` label (e.g. "Quiz", "Coding"). This maps each real category
 * string (confirmed live: Quiz, Flashcards, Coding, Mock Tests, AI Tutor,
 * Study Planner, Current Affairs, Interview Prep, Streak, General) to a
 * local Ionicons glyph. Falls back to a generic ribbon for any unrecognized
 * category rather than crashing on an unmapped value.
 */
const CATEGORY_ICON_NAMES: Record<string, IconName> = {
  Quiz: "flash",
  Flashcards: "albums",
  Coding: "code-slash",
  "Mock Tests": "document-text",
  "AI Tutor": "chatbubbles",
  "Study Planner": "calendar",
  "Current Affairs": "newspaper",
  "Interview Prep": "briefcase",
  Streak: "flame",
  General: "star",
};
const DEFAULT_BADGE_ICON_NAME: IconName = "ribbon";

/**
 * One badge in the hub's grid. Earned badges get the full-color brand
 * treatment (filled icon circle + relative "earned" timestamp); unearned
 * badges are rendered greyed-out/outlined with a small lock glyph so the
 * two states are unmistakable at a glance, not just distinguishable on close
 * reading — the phase brief specifically calls out that earned-vs-unearned
 * needs to read clearly in a screenshot.
 */
export function BadgeTile({ badge, className = "" }: BadgeTileProps) {
  const { colors } = useTheme();

  const iconName = CATEGORY_ICON_NAMES[badge.category] ?? DEFAULT_BADGE_ICON_NAME;

  return (
    <View className={["w-1/2 p-1.5", className].join(" ")}>
      <View
        className={[
          "items-center rounded-xl border p-3",
          badge.isEarned
            ? "border-brand/40 bg-brand/10 dark:border-brand-light/40 dark:bg-brand-light/10"
            : "border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
        ].join(" ")}
      >
        <View
          className={[
            "mb-2 h-14 w-14 items-center justify-center rounded-full",
            badge.isEarned ? "bg-brand dark:bg-brand-light" : "border border-border bg-transparent dark:border-border-dark",
          ].join(" ")}
        >
          <Icon
            name={iconName}
            size={26}
            color={badge.isEarned ? "#FFFFFF" : colors.textSecondary}
          />
          {!badge.isEarned ? (
            <View className="absolute -bottom-1 -right-1 h-5 w-5 items-center justify-center rounded-full border border-border bg-background dark:border-border-dark dark:bg-background-dark">
              <Icon name="lock-closed" size={11} color={colors.textSecondary} />
            </View>
          ) : null}
        </View>
        <Text
          numberOfLines={1}
          className={[
            "text-center text-caption font-semibold",
            badge.isEarned
              ? "text-ink-primary dark:text-ink-primary-dark"
              : "text-ink-secondary dark:text-ink-secondary-dark",
          ].join(" ")}
        >
          {badge.title}
        </Text>
        <Text
          numberOfLines={2}
          className="mt-0.5 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark"
          style={!badge.isEarned ? { opacity: 0.7 } : undefined}
        >
          {badge.isEarned && badge.earnedAtUtc ? `Earned ${formatRelativeTime(badge.earnedAtUtc)}` : badge.description}
        </Text>
      </View>
    </View>
  );
}

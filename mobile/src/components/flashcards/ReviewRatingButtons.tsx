import React from "react";
import { Pressable, Text, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";
import type { ReviewQuality } from "../../api/flashcards";

/**
 * The palette (`src/theme/tokens.ts`) has no semantic "blue" — `brand` is
 * indigo and `accent` is teal — but the spec calls for red/orange/blue/green
 * specifically, so "Good" uses a literal hex here rather than stretching an
 * existing token to mean something it doesn't anywhere else in the app.
 */
const GOOD_BLUE = "#2F6FED";

interface RatingOption {
  label: string;
  quality: ReviewQuality;
  color: string;
}

export interface ReviewRatingButtonsProps {
  onRate: (quality: ReviewQuality) => void;
  disabled?: boolean;
  className?: string;
}

/**
 * The four spaced-repetition rating buttons shown once a flashcard is
 * flipped to its back (Again/Hard/Good/Easy → quality 0/3/4/5). Color-coded
 * so the choice is legible at a glance, not just by label.
 */
export function ReviewRatingButtons({ onRate, disabled = false, className = "" }: ReviewRatingButtonsProps) {
  const { colors } = useTheme();

  const options: RatingOption[] = [
    { label: "Again", quality: 0, color: colors.danger },
    { label: "Hard", quality: 3, color: colors.warning },
    { label: "Good", quality: 4, color: GOOD_BLUE },
    { label: "Easy", quality: 5, color: colors.success },
  ];

  return (
    <View className={["flex-row gap-2", className].join(" ")}>
      {options.map((option) => (
        <Pressable
          key={option.label}
          onPress={() => onRate(option.quality)}
          disabled={disabled}
          accessibilityRole="button"
          accessibilityLabel={option.label}
          style={{ backgroundColor: option.color }}
          className={[
            "flex-1 items-center rounded-xl py-3.5",
            disabled ? "opacity-50" : "active:opacity-80",
          ].join(" ")}
        >
          <Text className="text-body font-semibold text-white">{option.label}</Text>
        </Pressable>
      ))}
    </View>
  );
}

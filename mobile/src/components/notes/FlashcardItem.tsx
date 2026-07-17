import React, { useState } from "react";
import { Pressable, Text, View } from "react-native";

import { Card } from "../Card";
import { Divider } from "../Divider";
import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";
import type { FlashcardDto } from "../../api/notes";

export interface FlashcardItemProps {
  index: number;
  flashcard: FlashcardDto;
  className?: string;
}

/**
 * One study flashcard: the question is always visible; tapping the card
 * reveals the answer beneath a divider (tap again to hide it). No existing
 * flip/reveal component was found elsewhere in the app to reuse, so this is
 * a simple tap-to-reveal `Card` rather than a flip animation.
 */
export function FlashcardItem({ index, flashcard, className = "" }: FlashcardItemProps) {
  const { colors } = useTheme();
  const [revealed, setRevealed] = useState(false);

  return (
    <Pressable
      onPress={() => setRevealed((prev) => !prev)}
      accessibilityRole="button"
      accessibilityLabel={revealed ? "Hide answer" : "Reveal answer"}
    >
      <Card className={className}>
        <View className="mb-2 flex-row items-center justify-between">
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Card {index + 1}</Text>
          <Icon name={revealed ? "eye-off-outline" : "eye-outline"} size={18} color={colors.textSecondary} />
        </View>
        <Text className="text-bodyMedium text-ink-primary dark:text-ink-primary-dark">{flashcard.question}</Text>
        {revealed ? (
          <>
            <Divider className="my-3" />
            <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">{flashcard.answer}</Text>
          </>
        ) : (
          <Text className="mt-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Tap to reveal answer
          </Text>
        )}
      </Card>
    </Pressable>
  );
}

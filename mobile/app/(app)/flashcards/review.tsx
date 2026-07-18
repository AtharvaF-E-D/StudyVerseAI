import React, { useState } from "react";
import { Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { FlashcardFlip } from "../../../src/components/flashcards/FlashcardFlip";
import { ReviewRatingButtons } from "../../../src/components/flashcards/ReviewRatingButtons";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { DueCardDto, ReviewQuality } from "../../../src/api/flashcards";
import { useDueCardsQuery, useReviewCardMutation } from "../../../src/hooks/useFlashcards";

function ReviewSkeleton() {
  return (
    <ScreenContainer scrollable={false}>
      <View className="mb-4 flex-row items-center justify-between">
        <Skeleton variant="circle" width={36} height={36} />
        <Skeleton variant="text" width={90} />
        <View className="w-9" />
      </View>
      <View className="flex-1 items-center justify-center">
        <Skeleton variant="rect" width="100%" height={220} className="rounded-2xl" />
      </View>
      <View className="flex-row gap-2">
        {[0, 1, 2, 3].map((i) => (
          <Skeleton key={i} variant="rect" height={52} className="flex-1 rounded-xl" />
        ))}
      </View>
    </ScreenContainer>
  );
}

/**
 * Study/review session — one card at a time, front then (on tap) its
 * flipped-back answer, then the four spaced-repetition rating buttons.
 * With `deckId` supplied (via `?deckId=`) this reviews just that deck's due
 * cards; omitted, it's the cross-deck daily queue from `GET /due`.
 *
 * The fetched queue is captured into local state exactly once (mirroring
 * the quiz play screen's `quizSessionCache` pattern — see
 * `app/(app)/quiz/[sessionId].tsx`) so this session's card order/count stays
 * stable while the underlying `useDueCardsQuery` is invalidated and
 * refetched in the background after each rating.
 */
export default function FlashcardReviewScreen() {
  const params = useLocalSearchParams<{ deckId?: string }>();
  const deckId = params.deckId;

  const { colors } = useTheme();
  const { show } = useToast();

  const dueQuery = useDueCardsQuery(deckId);
  const reviewMutation = useReviewCardMutation();

  // Freezes the queue the first time data arrives by calling `setState`
  // directly in the render body (guarded so it only ever fires once,
  // immediately settling before paint) rather than in a `useEffect` — the
  // "adjusting state when a prop changes" pattern React's own docs
  // recommend for exactly this "snapshot an incoming value once" case. This
  // keeps the session's card order/count stable afterward even as
  // `useDueCardsQuery` refetches in the background post-review.
  const [queue, setQueue] = useState<DueCardDto[] | null>(null);
  if (dueQuery.data && queue === null) {
    setQueue(dueQuery.data);
  }

  const [currentIndex, setCurrentIndex] = useState(0);
  const [flipped, setFlipped] = useState(false);
  const [reviewedCount, setReviewedCount] = useState(0);

  const currentCard = queue ? (queue[currentIndex] ?? null) : null;
  const isComplete = queue !== null && currentIndex >= queue.length;

  function handleFlip() {
    if (!currentCard || reviewMutation.isPending) return;
    setFlipped((prev) => !prev);
  }

  function handleRate(quality: ReviewQuality) {
    if (!currentCard || reviewMutation.isPending) return;
    reviewMutation.mutate(
      { cardId: currentCard.id, quality, deckId: currentCard.deckId },
      {
        onSuccess: () => {
          setReviewedCount((count) => count + 1);
          setFlipped(false);
          setCurrentIndex((index) => index + 1);
        },
        onError: () => show("Couldn't submit that review. Please try again.", "danger"),
      },
    );
  }

  function backToWhereItCameFrom() {
    if (deckId) router.replace(`/(app)/flashcards/${deckId}`);
    else router.replace("/(app)/flashcards");
  }

  if (dueQuery.isLoading || queue === null) {
    return <ReviewSkeleton />;
  }

  if (dueQuery.isError) {
    return (
      <ScreenContainer>
        <ErrorState
          title="Couldn't load your review queue"
          description="Check your connection and try again."
          onRetry={() => void dueQuery.refetch()}
        />
      </ScreenContainer>
    );
  }

  if (isComplete) {
    return (
      <ScreenContainer>
        <EmptyState
          icon="checkmark-circle-outline"
          title="All caught up!"
          description={
            reviewedCount > 0
              ? `Nice work — you reviewed ${reviewedCount} card${reviewedCount === 1 ? "" : "s"}.`
              : "Nothing is due for review right now."
          }
          actionLabel={deckId ? "Back to deck" : "Back to Flashcards"}
          onAction={backToWhereItCameFrom}
        />
      </ScreenContainer>
    );
  }

  if (!currentCard) return null;

  return (
    <ScreenContainer scrollable={false}>
      <View className="flex-1">
        <View className="mb-4 flex-row items-center justify-between">
          <Pressable
            onPress={() => router.back()}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Close review session"
            className="h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon name="close" size={22} color={colors.textPrimary} />
          </Pressable>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Card {currentIndex + 1} / {queue.length}
          </Text>
          <View className="w-9" />
        </View>

        {!deckId ? (
          <Text className="mb-3 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {currentCard.deckTitle}
          </Text>
        ) : null}

        <View className="flex-1 items-center justify-center">
          {/* `key` forces a clean remount per card so the flip animation never
              carries over stale rotation/text from the previous card. */}
          <FlashcardFlip
            key={currentCard.id}
            frontText={currentCard.frontText}
            backText={currentCard.backText}
            flipped={flipped}
            onPress={handleFlip}
            className="w-full"
          />
        </View>

        {!flipped ? (
          <Text className="mb-4 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Tap the card to reveal the answer
          </Text>
        ) : (
          <ReviewRatingButtons onRate={handleRate} disabled={reviewMutation.isPending} className="mb-2" />
        )}
      </View>
    </ScreenContainer>
  );
}

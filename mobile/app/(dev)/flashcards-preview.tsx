import React, { useState } from "react";
import { Pressable, Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Badge } from "../../src/components/Badge";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { EmptyState } from "../../src/components/EmptyState";
import { Icon } from "../../src/components/Icon";
import { ListItem } from "../../src/components/ListItem";
import { TextField } from "../../src/components/TextField";
import { FlashcardFlip } from "../../src/components/flashcards/FlashcardFlip";
import { ReviewRatingButtons } from "../../src/components/flashcards/ReviewRatingButtons";
import { useTheme } from "../../src/theme/ThemeProvider";
import type { DeckSummaryDto, FlashcardCardDto, ReviewQuality } from "../../src/api/flashcards";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/flashcards-preview`. Mirrors `app/(dev)/quiz-preview.tsx`'s/
// `tutor-preview.tsx`'s approach: the real backend has no `FlashcardsController`
// reachable yet (only its domain layer has landed —
// `backend/src/StudyVerse.Domain/Entities/Flashcard.cs`/`FlashcardDeck.cs` —
// and the server itself wasn't reachable over HTTP during this pass), so this
// feeds hand-written fixtures straight into the same shared building blocks
// the real deck-list/deck-detail/review screens use (`Card`, `ListItem`,
// `FlashcardFlip`, `ReviewRatingButtons`) for manual/automated visual QA in
// both light and dark mode. None of the data below comes from — or is wired
// to — the real flashcards API. Delete this file once the real backend is
// reachable and the flashcards screens have been re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_DECKS: DeckSummaryDto[] = [
  {
    id: "deck-1",
    title: "Spanish Verbs",
    description: "Common irregular verbs",
    isFavorite: true,
    cardCount: 24,
    dueTodayCount: 6,
    isShared: false,
    createdAtUtc: "2026-07-01T10:00:00Z",
  },
  {
    id: "deck-2",
    title: "Cell Biology",
    description: "Organelles and their functions",
    isFavorite: false,
    cardCount: 18,
    dueTodayCount: 0,
    isShared: true,
    createdAtUtc: "2026-07-05T10:00:00Z",
  },
  {
    id: "deck-3",
    title: "World Capitals",
    description: null,
    isFavorite: false,
    cardCount: 40,
    dueTodayCount: 12,
    isShared: false,
    createdAtUtc: "2026-07-10T10:00:00Z",
  },
];

function DeckListFixtureSection() {
  const { colors } = useTheme();
  const [decks, setDecks] = useState(FIXTURE_DECKS);

  function toggleFavorite(deckId: string) {
    setDecks((prev) => prev.map((d) => (d.id === deckId ? { ...d, isFavorite: !d.isFavorite } : d)));
  }

  const totalCards = decks.reduce((sum, d) => sum + d.cardCount, 0);
  const dueToday = decks.reduce((sum, d) => sum + d.dueTodayCount, 0);

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: deck list
      </Text>

      <Card className="mb-6">
        <View className="flex-row items-center justify-between">
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{decks.length}</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Decks</Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{totalCards}</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Cards</Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{dueToday}</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Due today</Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">7</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Mastered</Text>
          </View>
        </View>
      </Card>

      <View className="mb-6">
        <Card className="border-brand dark:border-brand-light">
          <View className="mb-1 flex-row items-center">
            <Icon name="flash" size={18} color={colors.brand} />
            <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
              {dueToday} cards due today
            </Text>
          </View>
          <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Review everything due across all your decks in one session.
          </Text>
          <Button title="Review due cards" onPress={() => {}} />
        </Card>
      </View>

      <View className="mb-3 flex-row items-center gap-2">
        <View className="rounded-full border border-border bg-surface px-3.5 py-2 dark:border-border-dark dark:bg-surface-dark">
          <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
            + Blank deck
          </Text>
        </View>
        <View className="rounded-full border border-border bg-surface px-3.5 py-2 dark:border-border-dark dark:bg-surface-dark">
          <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
            + Generate from topic (AI)
          </Text>
        </View>
      </View>

      <Card>
        {decks.map((deck, index) => (
          <React.Fragment key={deck.id}>
            {index > 0 ? <Divider /> : null}
            <View className="flex-row items-center">
              <ListItem
                leading={<Icon name="albums-outline" size={22} color={colors.brand} />}
                title={deck.title}
                subtitle={`${deck.cardCount} card${deck.cardCount === 1 ? "" : "s"}${deck.isShared ? " · Shared" : ""}`}
                onPress={() => {}}
                trailing={deck.dueTodayCount > 0 ? <Badge label={`${deck.dueTodayCount} due`} variant="brand" /> : null}
                className="flex-1"
              />
              <Pressable
                onPress={() => toggleFavorite(deck.id)}
                hitSlop={8}
                accessibilityRole="button"
                accessibilityLabel={deck.isFavorite ? "Remove favorite" : "Mark as favorite"}
                className="ml-2 mr-3"
              >
                <Icon
                  name={deck.isFavorite ? "star" : "star-outline"}
                  size={19}
                  color={deck.isFavorite ? colors.warning : colors.textSecondary}
                />
              </Pressable>
            </View>
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

const FIXTURE_CARDS: FlashcardCardDto[] = [
  {
    id: "card-1",
    frontText: "¿Cómo se dice 'to eat' en español?",
    backText: "Comer",
    imageUrl: null,
    nextReviewDateUtc: "2026-07-18T00:00:00Z",
    repetitions: 3,
  },
  {
    id: "card-2",
    frontText: "¿Cómo se dice 'to speak' en español?",
    backText: "Hablar",
    imageUrl: null,
    nextReviewDateUtc: "2026-07-20T00:00:00Z",
    repetitions: 1,
  },
  {
    id: "card-3",
    frontText: "¿Cómo se dice 'to live' en español?",
    backText: "Vivir",
    imageUrl: null,
    nextReviewDateUtc: "2026-07-19T00:00:00Z",
    repetitions: 0,
  },
];

function DeckDetailFixtureSection() {
  const { colors } = useTheme();
  const [isShared, setIsShared] = useState(false);
  const [shareToken, setShareToken] = useState<string | null>(null);

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: deck detail
      </Text>

      <View className="mb-6 flex-row items-center">
        <View className="mr-3 h-9 w-9 items-center justify-center rounded-full">
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </View>
        <View className="flex-1">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">Spanish Verbs</Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">3 cards</Text>
        </View>
        <View className="ml-3 h-9 w-9 items-center justify-center rounded-full">
          <Icon name="trash-outline" size={20} color={colors.danger} />
        </View>
      </View>

      <View className="mb-6">
        <Button title="Study this deck" onPress={() => {}} />
      </View>

      <Card className="mb-6">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Share deck</Text>
        {!isShared ? (
          <Button
            title="Share deck"
            variant="secondary"
            onPress={() => {
              setIsShared(true);
              setShareToken("A1B2C3D4");
            }}
          />
        ) : (
          <>
            <View className="mb-3 rounded-md border border-border bg-background px-3 py-2.5 dark:border-border-dark dark:bg-background-dark">
              <Text selectable className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">
                {shareToken}
              </Text>
            </View>
            <View className="flex-row gap-2">
              <View className="flex-1">
                <Button title="Share link" variant="secondary" onPress={() => {}} />
              </View>
              <View className="flex-1">
                <Button
                  title="Stop sharing"
                  variant="danger"
                  onPress={() => {
                    setIsShared(false);
                    setShareToken(null);
                  }}
                />
              </View>
            </View>
          </>
        )}
      </Card>

      <Card className="mb-6">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Add a card</Text>
        <TextField label="Front" placeholder="Question or prompt" value="" onChangeText={() => {}} multiline />
        <TextField label="Back" placeholder="Answer" value="" onChangeText={() => {}} multiline />
        <Button title="Add card" onPress={() => {}} disabled />
      </Card>

      <Card>
        {FIXTURE_CARDS.map((card, index) => (
          <React.Fragment key={card.id}>
            {index > 0 ? <Divider /> : null}
            <View className="flex-row items-center">
              <ListItem
                title={card.frontText}
                subtitle={card.backText}
                onPress={() => {}}
                trailing={<Icon name="create-outline" size={16} color={colors.textSecondary} />}
                className="flex-1"
              />
              <Pressable hitSlop={8} accessibilityRole="button" accessibilityLabel="Delete card" className="ml-2 mr-3">
                <Icon name="trash-outline" size={19} color={colors.danger} />
              </Pressable>
            </View>
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

function InteractiveReviewCard() {
  const [flipped, setFlipped] = useState(false);
  const [lastRating, setLastRating] = useState<ReviewQuality | null>(null);

  return (
    <View className="mb-8">
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
        Interactive (tap the card to flip)
      </Text>
      <FlashcardFlip
        frontText="What is the powerhouse of the cell?"
        backText="The mitochondria"
        flipped={flipped}
        onPress={() => setFlipped((prev) => !prev)}
        className="mb-4 w-full"
      />
      {!flipped ? (
        <Text className="text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Tap the card to reveal the answer
        </Text>
      ) : (
        <>
          <ReviewRatingButtons onRate={setLastRating} className="mb-2" />
          {lastRating !== null ? (
            <Text className="text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Last rated: {lastRating}
            </Text>
          ) : null}
        </>
      )}
    </View>
  );
}

function ReviewFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: review session
      </Text>

      <InteractiveReviewCard />

      <View className="mb-8">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
          Pinned: front state
        </Text>
        <FlashcardFlip
          frontText="What is the chemical symbol for gold?"
          backText="Au"
          flipped={false}
          onPress={() => {}}
          className="w-full"
        />
      </View>

      <View className="mb-8">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
          Pinned: flipped-back state + rating buttons
        </Text>
        <FlashcardFlip
          frontText="What is the chemical symbol for gold?"
          backText="Au"
          flipped
          onPress={() => {}}
          className="mb-4 w-full"
        />
        <ReviewRatingButtons onRate={() => {}} />
      </View>

      <View>
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
          Fixture: queue empty
        </Text>
        <Card>
          <EmptyState
            icon="checkmark-circle-outline"
            title="All caught up!"
            description="Nice work — you reviewed 12 cards."
            actionLabel="Back to Flashcards"
            onAction={() => {}}
          />
        </Card>
      </View>
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Flashcards feature, mirroring
 * the pattern established by `app/(dev)/quiz-preview.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/flashcards-preview`.
 */
export default function FlashcardsPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Flashcards Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <DeckListFixtureSection />
      <DeckDetailFixtureSection />
      <ReviewFixtureSection />
    </ScreenContainer>
  );
}

import React, { useState } from "react";
import { Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Chip } from "../../../src/components/Chip";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { ListItem } from "../../../src/components/ListItem";
import { Skeleton } from "../../../src/components/Skeleton";
import { TextField } from "../../../src/components/TextField";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { useNotesQuery } from "../../../src/hooks/useNotes";
import type { DeckSummaryDto } from "../../../src/api/flashcards";
import {
  useCreateDeckMutation,
  useDecksQuery,
  useFlashcardStatsQuery,
  useGenerateDeckFromNoteMutation,
  useGenerateDeckFromTopicMutation,
  useToggleFavoriteMutation,
} from "../../../src/hooks/useFlashcards";

const DEFAULT_AI_CARD_COUNT = 10;
const MIN_AI_CARD_COUNT = 3;
const MAX_AI_CARD_COUNT = 30;

type CreationMode = "blank" | "topic" | "note" | null;

function StatsStripSkeleton() {
  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        {[0, 1, 2].map((i) => (
          <View key={i} className="flex-1 items-center">
            <Skeleton variant="text" width={32} className="mb-2" />
            <Skeleton variant="text" width={56} />
          </View>
        ))}
      </View>
    </Card>
  );
}

function DecksListSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="flex-row items-center px-3 py-3">
          <Skeleton variant="circle" width={22} height={22} className="mr-3" />
          <View className="flex-1">
            <Skeleton variant="text" width="55%" className="mb-2" />
            <Skeleton variant="text" width="35%" />
          </View>
        </View>
      ))}
    </Card>
  );
}

interface NewDeckPanelProps {
  onCreatedDeckId: (deckId: string) => void;
}

/**
 * Modal-less "+ New deck" flow: a row of choice chips (blank / AI-from-topic
 * / AI-from-note) that reveals a small inline form beneath, rather than a
 * separate modal screen. Kept as its own top-level component (not a closure
 * defined inside the screen function) so React doesn't treat it as a new
 * component type — and reset its internal form state — on every parent render.
 */
function NewDeckPanel({ onCreatedDeckId }: NewDeckPanelProps) {
  const { show } = useToast();
  const [mode, setMode] = useState<CreationMode>(null);

  const [blankTitle, setBlankTitle] = useState("");
  const [blankDescription, setBlankDescription] = useState("");

  const [topicTitle, setTopicTitle] = useState("");
  const [topic, setTopic] = useState("");
  const [cardCountText, setCardCountText] = useState(String(DEFAULT_AI_CARD_COUNT));

  const [selectedNoteId, setSelectedNoteId] = useState<string | null>(null);

  const notesQuery = useNotesQuery();
  const readyNotes = (notesQuery.data ?? []).filter((note) => note.status === "ready");

  const createDeckMutation = useCreateDeckMutation();
  const generateFromTopicMutation = useGenerateDeckFromTopicMutation();
  const generateFromNoteMutation = useGenerateDeckFromNoteMutation();

  const isBusy =
    createDeckMutation.isPending || generateFromTopicMutation.isPending || generateFromNoteMutation.isPending;

  function closePanel() {
    setMode(null);
    setBlankTitle("");
    setBlankDescription("");
    setTopicTitle("");
    setTopic("");
    setCardCountText(String(DEFAULT_AI_CARD_COUNT));
    setSelectedNoteId(null);
  }

  function handleCreateBlank() {
    const title = blankTitle.trim();
    if (!title || isBusy) return;
    createDeckMutation.mutate(
      { title, description: blankDescription.trim() || undefined },
      {
        onSuccess: (deck) => {
          closePanel();
          onCreatedDeckId(deck.id);
        },
        onError: () => show("Couldn't create that deck. Please try again.", "danger"),
      },
    );
  }

  function handleGenerateFromTopic() {
    const title = topicTitle.trim();
    const topicText = topic.trim();
    if (!title || !topicText || isBusy) return;
    const parsedCount = parseInt(cardCountText, 10);
    const cardCount = Number.isFinite(parsedCount)
      ? Math.min(MAX_AI_CARD_COUNT, Math.max(MIN_AI_CARD_COUNT, parsedCount))
      : DEFAULT_AI_CARD_COUNT;
    generateFromTopicMutation.mutate(
      { title, topic: topicText, cardCount },
      {
        onSuccess: (deck) => {
          closePanel();
          onCreatedDeckId(deck.id);
        },
        onError: () => show("Couldn't generate that deck. Please try again.", "danger"),
      },
    );
  }

  function handleGenerateFromNote() {
    if (!selectedNoteId || isBusy) return;
    generateFromNoteMutation.mutate(selectedNoteId, {
      onSuccess: (deck) => {
        closePanel();
        onCreatedDeckId(deck.id);
      },
      onError: () => show("Couldn't generate that deck. Please try again.", "danger"),
    });
  }

  return (
    <View className="mb-6">
      {mode === null ? (
        <View className="flex-row flex-wrap gap-2">
          <Chip label="+ Blank deck" onPress={() => setMode("blank")} />
          <Chip label="+ Generate from topic (AI)" onPress={() => setMode("topic")} />
          {readyNotes.length > 0 ? (
            <Chip label="+ Generate from note (AI)" onPress={() => setMode("note")} />
          ) : null}
        </View>
      ) : (
        <Card>
          <View className="mb-4 flex-row items-center justify-between">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
              {mode === "blank" ? "New blank deck" : mode === "topic" ? "Generate from topic" : "Generate from note"}
            </Text>
            <Pressable
              onPress={closePanel}
              hitSlop={8}
              accessibilityRole="button"
              accessibilityLabel="Cancel"
              disabled={isBusy}
            >
              <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
                Cancel
              </Text>
            </Pressable>
          </View>

          {mode === "blank" ? (
            <>
              <TextField label="Title" placeholder="e.g. Spanish Verbs" value={blankTitle} onChangeText={setBlankTitle} />
              <TextField
                label="Description (optional)"
                placeholder="What's this deck for?"
                value={blankDescription}
                onChangeText={setBlankDescription}
              />
              <Button
                title="Create deck"
                onPress={handleCreateBlank}
                loading={createDeckMutation.isPending}
                disabled={!blankTitle.trim()}
              />
            </>
          ) : null}

          {mode === "topic" ? (
            <>
              <TextField label="Deck title" placeholder="e.g. Cell Biology" value={topicTitle} onChangeText={setTopicTitle} />
              <TextField
                label="Topic"
                placeholder="What should the cards cover?"
                value={topic}
                onChangeText={setTopic}
                multiline
              />
              <TextField
                label={`Number of cards (${MIN_AI_CARD_COUNT}-${MAX_AI_CARD_COUNT})`}
                placeholder={String(DEFAULT_AI_CARD_COUNT)}
                value={cardCountText}
                onChangeText={setCardCountText}
                keyboardType="number-pad"
              />
              <Button
                title="Generate deck"
                onPress={handleGenerateFromTopic}
                loading={generateFromTopicMutation.isPending}
                disabled={!topicTitle.trim() || !topic.trim()}
              />
            </>
          ) : null}

          {mode === "note" ? (
            <>
              <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                Pick a note to turn into a flashcard deck.
              </Text>
              <View className="mb-4 flex-row flex-wrap gap-2">
                {readyNotes.map((note) => (
                  <Chip
                    key={note.id}
                    label={note.title}
                    selected={selectedNoteId === note.id}
                    onPress={() => setSelectedNoteId(note.id)}
                  />
                ))}
              </View>
              <Button
                title="Generate deck"
                onPress={handleGenerateFromNote}
                loading={generateFromNoteMutation.isPending}
                disabled={!selectedNoteId}
              />
            </>
          ) : null}
        </Card>
      )}
    </View>
  );
}

/**
 * Flashcard decks list — the entry point reached from the "Flashcards" card
 * on the dashboard. Shows aggregate stats, a "Review due cards" CTA when
 * anything is due today (the cross-deck queue from `GET /due`), a
 * modal-less "+ New deck" flow (blank / AI-from-topic / AI-from-note), and
 * the deck rows themselves.
 */
export default function FlashcardsListScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const statsQuery = useFlashcardStatsQuery();
  const decksQuery = useDecksQuery();
  const toggleFavoriteMutation = useToggleFavoriteMutation();

  function handleToggleFavorite(deckId: string) {
    if (toggleFavoriteMutation.isPending) return;
    toggleFavoriteMutation.mutate(deckId, {
      onError: () => show("Couldn't update favorite.", "danger"),
    });
  }

  function openDeck(deckId: string) {
    router.push(`/(app)/flashcards/${deckId}`);
  }

  const decks = decksQuery.data ?? [];
  const stats = statsQuery.data;

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Flashcards</Text>

      {statsQuery.isLoading ? (
        <StatsStripSkeleton />
      ) : stats ? (
        <Card className="mb-6">
          <View className="flex-row items-center justify-between">
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {stats.totalDecks}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Decks</Text>
            </View>
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {stats.totalCards}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Cards</Text>
            </View>
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {stats.dueToday}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Due today</Text>
            </View>
            <View className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {stats.mastered}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Mastered</Text>
            </View>
          </View>
        </Card>
      ) : null}

      {stats && stats.dueToday > 0 ? (
        <View className="mb-6">
          <Card className="border-brand dark:border-brand-light">
            <View className="mb-1 flex-row items-center">
              <Icon name="flash" size={18} color={colors.brand} />
              <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
                {stats.dueToday} card{stats.dueToday === 1 ? "" : "s"} due today
              </Text>
            </View>
            <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Review everything due across all your decks in one session.
            </Text>
            <Button title="Review due cards" onPress={() => router.push("/(app)/flashcards/review")} />
          </Card>
        </View>
      ) : null}

      <View className="mb-3 flex-row items-center justify-between">
        <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">Your decks</Text>
      </View>

      <NewDeckPanel onCreatedDeckId={openDeck} />

      {decksQuery.isLoading ? (
        <DecksListSkeleton />
      ) : decksQuery.isError ? (
        <ErrorState
          title="Couldn't load your decks"
          description="Check your connection and try again."
          onRetry={() => void decksQuery.refetch()}
        />
      ) : decks.length === 0 ? (
        <EmptyState
          icon="albums-outline"
          title="No decks yet"
          description="Create a blank deck, or let AI generate one from a topic or an existing note."
        />
      ) : (
        <Card>
          {decks.map((deck: DeckSummaryDto, index) => (
            <React.Fragment key={deck.id}>
              {index > 0 ? <Divider /> : null}
              {/* Favorite star is a sibling of ListItem, not inside its `trailing`
                  slot — ListItem renders as a <button> when `onPress` is set, and
                  nesting another pressable inside it produces invalid HTML
                  (<button> cannot contain a nested <button>). */}
              <View className="flex-row items-center">
                <ListItem
                  leading={<Icon name="albums-outline" size={22} color={colors.brand} />}
                  title={deck.title}
                  subtitle={`${deck.cardCount} card${deck.cardCount === 1 ? "" : "s"}${deck.isShared ? " · Shared" : ""}`}
                  onPress={() => openDeck(deck.id)}
                  trailing={
                    deck.dueTodayCount > 0 ? (
                      <Badge label={`${deck.dueTodayCount} due`} variant="brand" />
                    ) : null
                  }
                  className="flex-1"
                />
                <Pressable
                  onPress={() => handleToggleFavorite(deck.id)}
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
      )}
    </ScreenContainer>
  );
}

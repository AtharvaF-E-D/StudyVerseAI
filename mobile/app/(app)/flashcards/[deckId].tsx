import React, { useState } from "react";
import { Alert, Pressable, Share, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { ListItem } from "../../../src/components/ListItem";
import { Skeleton } from "../../../src/components/Skeleton";
import { TextField } from "../../../src/components/TextField";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { FlashcardCardDto } from "../../../src/api/flashcards";
import {
  useAddCardMutation,
  useDeckQuery,
  useDeleteCardMutation,
  useDeleteDeckMutation,
  useShareDeckMutation,
  useUnshareDeckMutation,
  useUpdateCardMutation,
} from "../../../src/hooks/useFlashcards";

function DetailSkeleton() {
  return (
    <View>
      <Skeleton variant="rect" width="100%" height={48} className="mb-4 rounded-xl" />
      <Card>
        {[0, 1, 2].map((i) => (
          <View key={i} className="px-3 py-3">
            <Skeleton variant="text" width="80%" className="mb-2" />
            <Skeleton variant="text" width="55%" />
          </View>
        ))}
      </Card>
    </View>
  );
}

/**
 * One deck's detail screen: its card list, a "Study this deck" entry into
 * the review session scoped to this deck, an add/edit-card form, a
 * share/unshare toggle, and delete-deck. The anonymous public share viewer
 * (`GET /shared/{token}`) was cut for time per the phase spec — sharing here
 * only produces the token/string to hand off manually; see `src/api/flashcards.ts`'s
 * `getSharedDeck` for the client function that a future viewer screen would call.
 */
export default function FlashcardDeckDetailScreen() {
  const params = useLocalSearchParams<{ deckId: string }>();
  const deckId = params.deckId ?? "";

  const { colors } = useTheme();
  const { show } = useToast();

  const deckQuery = useDeckQuery(deckId);
  const deleteDeckMutation = useDeleteDeckMutation();
  const shareDeckMutation = useShareDeckMutation(deckId);
  const unshareDeckMutation = useUnshareDeckMutation(deckId);
  const addCardMutation = useAddCardMutation(deckId);
  const updateCardMutation = useUpdateCardMutation(deckId);
  const deleteCardMutation = useDeleteCardMutation(deckId);

  // The `GET /decks/{id}` contract only returns `isShared` (a flag), not the
  // token itself — the token is only ever returned by `POST .../share`. So
  // this screen only ever has a token to display after the user (re-)shares
  // during this visit; a deck already shared from a previous visit shows
  // "Get link" instead, assuming the backend returns the same token
  // idempotently rather than minting a new one on every call.
  const [shareToken, setShareToken] = useState<string | null>(null);
  const [editingCardId, setEditingCardId] = useState<string | null>(null);
  const [frontText, setFrontText] = useState("");
  const [backText, setBackText] = useState("");

  function resetCardForm() {
    setEditingCardId(null);
    setFrontText("");
    setBackText("");
  }

  function startEditCard(card: FlashcardCardDto) {
    setEditingCardId(card.id);
    setFrontText(card.frontText);
    setBackText(card.backText);
  }

  function handleSubmitCard() {
    const front = frontText.trim();
    const back = backText.trim();
    if (!front || !back) return;

    if (editingCardId) {
      updateCardMutation.mutate(
        { cardId: editingCardId, request: { frontText: front, backText: back } },
        {
          onSuccess: () => resetCardForm(),
          onError: () => show("Couldn't save that card. Please try again.", "danger"),
        },
      );
    } else {
      addCardMutation.mutate(
        { frontText: front, backText: back },
        {
          onSuccess: () => resetCardForm(),
          onError: () => show("Couldn't add that card. Please try again.", "danger"),
        },
      );
    }
  }

  function confirmDeleteCard(card: FlashcardCardDto) {
    Alert.alert("Delete card", "Delete this card? This can't be undone.", [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () =>
          deleteCardMutation.mutate(card.id, {
            onSuccess: () => {
              if (editingCardId === card.id) resetCardForm();
            },
            onError: () => show("Couldn't delete that card.", "danger"),
          }),
      },
    ]);
  }

  function confirmDeleteDeck() {
    if (!deckQuery.data) return;
    Alert.alert("Delete deck", `Delete "${deckQuery.data.title}"? This can't be undone.`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () =>
          deleteDeckMutation.mutate(deckId, {
            onSuccess: () => router.back(),
            onError: () => show("Couldn't delete that deck.", "danger"),
          }),
      },
    ]);
  }

  function handleShare() {
    shareDeckMutation.mutate(undefined, {
      onSuccess: (result) => setShareToken(result.shareToken),
      onError: () => show("Couldn't share that deck. Please try again.", "danger"),
    });
  }

  function handleUnshare() {
    unshareDeckMutation.mutate(undefined, {
      onSuccess: () => setShareToken(null),
      onError: () => show("Couldn't stop sharing that deck.", "danger"),
    });
  }

  async function handleShareLink() {
    if (!shareToken) return;
    try {
      await Share.share({ message: `Check out my "${deckQuery.data?.title ?? "flashcard"}" deck on StudyVerse AI — share code: ${shareToken}` });
    } catch {
      // User dismissed the share sheet, or it isn't available on this platform — nothing to recover from.
    }
  }

  const deck = deckQuery.data;
  const cards = deck?.cards ?? [];

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center">
        <Pressable
          onPress={() => router.back()}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back"
          className="mr-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        <View className="flex-1">
          <Text numberOfLines={1} className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {deck?.title ?? "Deck"}
          </Text>
          {deck ? (
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {cards.length} card{cards.length === 1 ? "" : "s"}
            </Text>
          ) : null}
        </View>
        {deck ? (
          <Pressable
            onPress={confirmDeleteDeck}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Delete deck"
            className="ml-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon name="trash-outline" size={20} color={colors.danger} />
          </Pressable>
        ) : null}
      </View>

      {deckQuery.isLoading ? (
        <DetailSkeleton />
      ) : deckQuery.isError || !deck ? (
        <ErrorState
          title="Couldn't load this deck"
          description="Check your connection and try again."
          onRetry={() => void deckQuery.refetch()}
        />
      ) : (
        <>
          <View className="mb-6">
            <Button
              title="Study this deck"
              disabled={cards.length === 0}
              onPress={() => router.push({ pathname: "/(app)/flashcards/review", params: { deckId } })}
            />
          </View>

          {/* Share toggle */}
          <Card className="mb-6">
            <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Share deck</Text>
            {!deck.isShared && !shareToken ? (
              <Button title="Share deck" variant="secondary" loading={shareDeckMutation.isPending} onPress={handleShare} />
            ) : (
              <>
                {shareToken ? (
                  <View className="mb-3 rounded-md border border-border bg-background px-3 py-2.5 dark:border-border-dark dark:bg-background-dark">
                    <Text selectable className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">
                      {shareToken}
                    </Text>
                  </View>
                ) : (
                  <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                    This deck is shared. Get its link to hand it off again.
                  </Text>
                )}
                <View className="flex-row gap-2">
                  <View className="flex-1">
                    <Button
                      title={shareToken ? "Share link" : "Get link"}
                      variant="secondary"
                      loading={shareDeckMutation.isPending}
                      onPress={shareToken ? () => void handleShareLink() : handleShare}
                    />
                  </View>
                  <View className="flex-1">
                    <Button
                      title="Stop sharing"
                      variant="danger"
                      loading={unshareDeckMutation.isPending}
                      onPress={handleUnshare}
                    />
                  </View>
                </View>
              </>
            )}
          </Card>

          {/* Add/edit card form */}
          <Card className="mb-6">
            <View className="mb-3 flex-row items-center justify-between">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {editingCardId ? "Edit card" : "Add a card"}
              </Text>
              {editingCardId ? (
                <Pressable onPress={resetCardForm} hitSlop={8} accessibilityRole="button" accessibilityLabel="Cancel edit">
                  <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
                    Cancel
                  </Text>
                </Pressable>
              ) : null}
            </View>
            <TextField label="Front" placeholder="Question or prompt" value={frontText} onChangeText={setFrontText} multiline />
            <TextField label="Back" placeholder="Answer" value={backText} onChangeText={setBackText} multiline />
            <Button
              title={editingCardId ? "Save changes" : "Add card"}
              onPress={handleSubmitCard}
              loading={addCardMutation.isPending || updateCardMutation.isPending}
              disabled={!frontText.trim() || !backText.trim()}
            />
          </Card>

          {/* Card list */}
          {cards.length === 0 ? (
            <EmptyState icon="albums-outline" title="No cards yet" description="Add your first card above." />
          ) : (
            <Card>
              {cards.map((card, index) => (
                <React.Fragment key={card.id}>
                  {index > 0 ? <Divider /> : null}
                  {/* Delete is a sibling of ListItem, not inside its `trailing` slot —
                      ListItem renders as a <button> when `onPress` is set (here, tap
                      to edit), and nesting another pressable inside it produces
                      invalid HTML (<button> cannot contain a nested <button>). */}
                  <View className="flex-row items-center">
                    <ListItem
                      title={card.frontText}
                      subtitle={card.backText}
                      onPress={() => startEditCard(card)}
                      trailing={<Icon name="create-outline" size={16} color={colors.textSecondary} />}
                      className="flex-1"
                    />
                    <Pressable
                      onPress={() => confirmDeleteCard(card)}
                      hitSlop={8}
                      accessibilityRole="button"
                      accessibilityLabel="Delete card"
                      className="ml-2 mr-3"
                    >
                      <Icon name="trash-outline" size={19} color={colors.danger} />
                    </Pressable>
                  </View>
                </React.Fragment>
              ))}
            </Card>
          )}
        </>
      )}
    </ScreenContainer>
  );
}

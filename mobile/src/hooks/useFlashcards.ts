import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  addCard,
  createDeck,
  deleteCard,
  deleteDeck,
  generateDeckFromNote,
  generateDeckFromTopic,
  getDeck,
  getDecks,
  getDueCards,
  getFlashcardStats,
  reviewCard,
  shareDeck,
  toggleDeckFavorite,
  unshareDeck,
  updateCard,
  type CreateDeckRequest,
  type CreateDeckResponse,
  type DeckDetailDto,
  type DeckSummaryDto,
  type DueCardDto,
  type FlashcardCardDto,
  type FlashcardStatsDto,
  type GenerateDeckFromTopicRequest,
  type GenerateDeckResponse,
  type ReviewCardRequest,
  type ReviewCardResponse,
  type ShareDeckResponse,
  type ToggleFavoriteResponse,
  type UpsertCardRequest,
} from "../api/flashcards";

export const flashcardDecksQueryKey = ["flashcards", "decks"] as const;
export const flashcardStatsQueryKey = ["flashcards", "stats"] as const;

export function flashcardDeckQueryKey(deckId: string) {
  return ["flashcards", "decks", deckId] as const;
}

/** `deckId` omitted maps to the cross-deck daily queue — kept as a stable `"all"` key segment rather than `undefined` so React Query doesn't treat every call with the same intent as a different query. */
export function flashcardDueQueryKey(deckId?: string) {
  return ["flashcards", "due", deckId ?? "all"] as const;
}

/** Fetches every deck the signed-in user owns, including favorite/due-today/share state for the deck list rows. */
export function useDecksQuery() {
  return useQuery<DeckSummaryDto[]>({
    queryKey: flashcardDecksQueryKey,
    queryFn: getDecks,
  });
}

/** Fetches one deck's full detail (its cards, favorite/share state) for the deck detail screen. */
export function useDeckQuery(deckId: string) {
  return useQuery<DeckDetailDto>({
    queryKey: flashcardDeckQueryKey(deckId),
    queryFn: () => getDeck(deckId),
    enabled: deckId.length > 0,
  });
}

/** Fetches the signed-in user's aggregate flashcard stats (total decks/cards, due today, mastered) for the deck list's stats strip. */
export function useFlashcardStatsQuery() {
  return useQuery<FlashcardStatsDto>({
    queryKey: flashcardStatsQueryKey,
    queryFn: getFlashcardStats,
  });
}

/** Creates a blank deck (no cards yet), then invalidates the deck list/stats so it appears there. */
export function useCreateDeckMutation() {
  const queryClient = useQueryClient();

  return useMutation<CreateDeckResponse, unknown, CreateDeckRequest>({
    mutationFn: (request) => createDeck(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Creates a deck and AI-generates its cards from a free-text topic, then invalidates the deck list/stats. */
export function useGenerateDeckFromTopicMutation() {
  const queryClient = useQueryClient();

  return useMutation<GenerateDeckResponse, unknown, GenerateDeckFromTopicRequest>({
    mutationFn: (request) => generateDeckFromTopic(request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Creates a deck and AI-generates its cards from an existing AI Note, then invalidates the deck list/stats. */
export function useGenerateDeckFromNoteMutation() {
  const queryClient = useQueryClient();

  return useMutation<GenerateDeckResponse, unknown, string>({
    mutationFn: (noteId) => generateDeckFromNote(noteId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Deletes a deck, then refetches the deck list/stats so it disappears. */
export function useDeleteDeckMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (deckId) => deleteDeck(deckId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Toggles a deck's favorite flag, then invalidates the deck list/detail so both reflect the server's authoritative flag. */
export function useToggleFavoriteMutation() {
  const queryClient = useQueryClient();

  return useMutation<ToggleFavoriteResponse, unknown, string>({
    mutationFn: (deckId) => toggleDeckFavorite(deckId),
    onSuccess: (_result, deckId) => {
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
    },
  });
}

/** Shares a deck, returning its public share token, then invalidates the deck detail so `isShared` reflects the server. */
export function useShareDeckMutation(deckId: string) {
  const queryClient = useQueryClient();

  return useMutation<ShareDeckResponse, unknown, void>({
    mutationFn: () => shareDeck(deckId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
    },
  });
}

/** Revokes a deck's share link, then invalidates the deck detail so `isShared` flips back. */
export function useUnshareDeckMutation(deckId: string) {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, void>({
    mutationFn: () => unshareDeck(deckId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
    },
  });
}

/** Adds a card to a deck, then invalidates that deck's detail and the deck list/stats (card counts changed). */
export function useAddCardMutation(deckId: string) {
  const queryClient = useQueryClient();

  return useMutation<FlashcardCardDto, unknown, UpsertCardRequest>({
    mutationFn: (request) => addCard(deckId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Edits an existing card's front/back text, then invalidates that deck's detail. */
export function useUpdateCardMutation(deckId: string) {
  const queryClient = useQueryClient();

  return useMutation<FlashcardCardDto, unknown, { cardId: string; request: UpsertCardRequest }>({
    mutationFn: ({ cardId, request }) => updateCard(deckId, cardId, request),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
    },
  });
}

/** Deletes a card, then invalidates that deck's detail and the deck list/stats (card counts changed). */
export function useDeleteCardMutation(deckId: string) {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (cardId) => deleteCard(deckId, cardId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(deckId) });
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
    },
  });
}

/** Fetches today's review queue — every due card across decks when `deckId` is omitted, or one deck's due cards when it's passed. */
export function useDueCardsQuery(deckId?: string) {
  return useQuery<DueCardDto[]>({
    queryKey: flashcardDueQueryKey(deckId),
    queryFn: () => getDueCards(deckId),
  });
}

/**
 * Submits a review rating for one card. The active review session advances
 * its own local queue itself (mirroring the quiz play screen's pattern —
 * see `useQuiz.ts`/`app/(app)/quiz/[sessionId].tsx`) rather than waiting on
 * a refetch, but this still invalidates the due-queue/stats/deck-detail
 * queries so counts are fresh the next time the deck list or deck detail is
 * visited.
 */
export function useReviewCardMutation() {
  const queryClient = useQueryClient();

  return useMutation<ReviewCardResponse, unknown, { cardId: string; deckId?: string } & ReviewCardRequest>({
    mutationFn: ({ cardId, quality }) => reviewCard(cardId, { quality }),
    onSuccess: (_result, variables) => {
      // Partial key match: invalidates both `["flashcards","due","all"]` and
      // any single-deck `["flashcards","due",deckId]` query.
      void queryClient.invalidateQueries({ queryKey: ["flashcards", "due"] });
      void queryClient.invalidateQueries({ queryKey: flashcardStatsQueryKey });
      void queryClient.invalidateQueries({ queryKey: flashcardDecksQueryKey });
      if (variables.deckId) {
        void queryClient.invalidateQueries({ queryKey: flashcardDeckQueryKey(variables.deckId) });
      }
    },
  });
}

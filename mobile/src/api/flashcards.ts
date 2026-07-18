import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Flashcards" backend contract (base path
// `api/v1/flashcards`), built in parallel with this mobile work. As of this
// writing the backend only has its domain layer in place (`Flashcard.cs`,
// `FlashcardDeck.cs`, `IFlashcardGenerationProvider.cs` under
// `backend/src/StudyVerse.*`) — no `FlashcardsController` yet, and the
// server wasn't reachable over HTTP during this pass (see the `(dev)`
// fixture preview screen used for visual QA instead). Shaped strictly to
// the contract handed off for this phase; revisit once the real controller
// lands and can be hit directly, the same way `quiz.ts`/`notes.ts` were
// cross-checked once their backends became available.
//
// One nuance carried over from `quiz.ts`/`notes.ts`: every other controller
// in this app registers `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`
// on the backend, but nothing here is an enum on the wire — `quality` is a
// plain number (see `ReviewCardRequest` below), so no camelCase gotcha
// applies to this file.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// POST /flashcards/decks  { title, description? }  →  CreateDeckResponse
// ---------------------------------------------------------------------------

export interface CreateDeckRequest {
  title: string;
  description?: string;
}

export interface CreateDeckResponse {
  id: string;
  title: string;
  description: string | null;
  createdAtUtc: string;
}

export async function createDeck(request: CreateDeckRequest): Promise<CreateDeckResponse> {
  const { data } = await coreApiClient.post<CreateDeckResponse>("/flashcards/decks", request);
  return data;
}

// ---------------------------------------------------------------------------
// POST /flashcards/decks/from-topic  { title, topic, cardCount }  →  GenerateDeckResponse
// POST /flashcards/decks/from-note/{noteId}  →  GenerateDeckResponse
// ---------------------------------------------------------------------------

export interface GenerateDeckFromTopicRequest {
  title: string;
  topic: string;
  cardCount: number;
}

/** Shared by both AI-generation endpoints — neither returns the generated cards themselves, just the new deck's id/title/count. */
export interface GenerateDeckResponse {
  id: string;
  title: string;
  cardCount: number;
}

export async function generateDeckFromTopic(
  request: GenerateDeckFromTopicRequest,
): Promise<GenerateDeckResponse> {
  const { data } = await coreApiClient.post<GenerateDeckResponse>("/flashcards/decks/from-topic", request);
  return data;
}

export async function generateDeckFromNote(noteId: string): Promise<GenerateDeckResponse> {
  const { data } = await coreApiClient.post<GenerateDeckResponse>(`/flashcards/decks/from-note/${noteId}`);
  return data;
}

// ---------------------------------------------------------------------------
// GET /flashcards/decks  →  DeckSummaryDto[]
// ---------------------------------------------------------------------------

export interface DeckSummaryDto {
  id: string;
  title: string;
  description: string | null;
  isFavorite: boolean;
  cardCount: number;
  dueTodayCount: number;
  isShared: boolean;
  createdAtUtc: string;
}

export async function getDecks(): Promise<DeckSummaryDto[]> {
  const { data } = await coreApiClient.get<DeckSummaryDto[]>("/flashcards/decks");
  return data;
}

// ---------------------------------------------------------------------------
// GET /flashcards/decks/{id}  →  DeckDetailDto
// ---------------------------------------------------------------------------

export interface FlashcardCardDto {
  id: string;
  frontText: string;
  backText: string;
  imageUrl: string | null;
  nextReviewDateUtc: string;
  repetitions: number;
}

export interface DeckDetailDto {
  id: string;
  title: string;
  description: string | null;
  isFavorite: boolean;
  isShared: boolean;
  cards: FlashcardCardDto[];
}

export async function getDeck(deckId: string): Promise<DeckDetailDto> {
  const { data } = await coreApiClient.get<DeckDetailDto>(`/flashcards/decks/${deckId}`);
  return data;
}

// ---------------------------------------------------------------------------
// DELETE /flashcards/decks/{id}  →  204 No Content
// ---------------------------------------------------------------------------

export async function deleteDeck(deckId: string): Promise<void> {
  await coreApiClient.delete<void>(`/flashcards/decks/${deckId}`);
}

// ---------------------------------------------------------------------------
// POST /flashcards/decks/{id}/favorite  →  { isFavorite: boolean }
// ---------------------------------------------------------------------------

export interface ToggleFavoriteResponse {
  isFavorite: boolean;
}

/** Toggles the deck's favorite flag (there's no separate set-true/set-false endpoint — this call just flips it). */
export async function toggleDeckFavorite(deckId: string): Promise<ToggleFavoriteResponse> {
  const { data } = await coreApiClient.post<ToggleFavoriteResponse>(`/flashcards/decks/${deckId}/favorite`);
  return data;
}

// ---------------------------------------------------------------------------
// POST /flashcards/decks/{id}/share    →  { shareToken: string }
// DELETE /flashcards/decks/{id}/share  →  204 No Content
// ---------------------------------------------------------------------------

export interface ShareDeckResponse {
  shareToken: string;
}

export async function shareDeck(deckId: string): Promise<ShareDeckResponse> {
  const { data } = await coreApiClient.post<ShareDeckResponse>(`/flashcards/decks/${deckId}/share`);
  return data;
}

export async function unshareDeck(deckId: string): Promise<void> {
  await coreApiClient.delete<void>(`/flashcards/decks/${deckId}/share`);
}

// ---------------------------------------------------------------------------
// GET /flashcards/shared/{shareToken}  →  DeckDetailDto (public, no auth)
// ---------------------------------------------------------------------------

/**
 * Fetches a shared deck by its public token — the one anonymous endpoint in
 * this contract. Reuses `coreApiClient` for consistency with every other
 * call in this file: it attaches a Bearer token when a session happens to
 * exist, but this endpoint is `[AllowAnonymous]` per spec, so an absent
 * token is equally fine. No screen in this phase calls this yet (the
 * anonymous shared-deck viewer was cut for time — see the flashcards
 * screens' file headers), but the contract function is here so that screen
 * can be added later without touching this file.
 */
export async function getSharedDeck(shareToken: string): Promise<DeckDetailDto> {
  const { data } = await coreApiClient.get<DeckDetailDto>(`/flashcards/shared/${shareToken}`);
  return data;
}

// ---------------------------------------------------------------------------
// POST /flashcards/decks/{id}/cards  { frontText, backText, imageUrl? }  →  FlashcardCardDto
// PUT  /flashcards/decks/{deckId}/cards/{cardId}                        →  FlashcardCardDto
// DELETE /flashcards/decks/{deckId}/cards/{cardId}                      →  204 No Content
// ---------------------------------------------------------------------------

export interface UpsertCardRequest {
  frontText: string;
  backText: string;
  imageUrl?: string;
}

export async function addCard(deckId: string, request: UpsertCardRequest): Promise<FlashcardCardDto> {
  const { data } = await coreApiClient.post<FlashcardCardDto>(`/flashcards/decks/${deckId}/cards`, request);
  return data;
}

export async function updateCard(
  deckId: string,
  cardId: string,
  request: UpsertCardRequest,
): Promise<FlashcardCardDto> {
  const { data } = await coreApiClient.put<FlashcardCardDto>(
    `/flashcards/decks/${deckId}/cards/${cardId}`,
    request,
  );
  return data;
}

export async function deleteCard(deckId: string, cardId: string): Promise<void> {
  await coreApiClient.delete<void>(`/flashcards/decks/${deckId}/cards/${cardId}`);
}

// ---------------------------------------------------------------------------
// GET /flashcards/due?deckId=  →  DueCardDto[]
// ---------------------------------------------------------------------------

export interface DueCardDto {
  id: string;
  deckId: string;
  deckTitle: string;
  frontText: string;
  backText: string;
  imageUrl: string | null;
}

/** Omit `deckId` for today's cross-deck review queue; pass it to scope the queue to a single deck. */
export async function getDueCards(deckId?: string): Promise<DueCardDto[]> {
  const { data } = await coreApiClient.get<DueCardDto[]>("/flashcards/due", { params: { deckId } });
  return data;
}

// ---------------------------------------------------------------------------
// POST /flashcards/cards/{id}/review  { quality }  →  { nextReviewDateUtc, intervalDays }
// ---------------------------------------------------------------------------

/** Client-facing 4-point rating scale: Again=0, Hard=3, Good=4, Easy=5 (matches the SM-2-style spacing the backend expects). */
export type ReviewQuality = 0 | 3 | 4 | 5;

export interface ReviewCardRequest {
  quality: ReviewQuality;
}

export interface ReviewCardResponse {
  nextReviewDateUtc: string;
  intervalDays: number;
}

export async function reviewCard(cardId: string, request: ReviewCardRequest): Promise<ReviewCardResponse> {
  const { data } = await coreApiClient.post<ReviewCardResponse>(`/flashcards/cards/${cardId}/review`, request);
  return data;
}

// ---------------------------------------------------------------------------
// GET /flashcards/stats  →  FlashcardStatsDto
// ---------------------------------------------------------------------------

export interface FlashcardStatsDto {
  totalDecks: number;
  totalCards: number;
  dueToday: number;
  mastered: number;
}

export async function getFlashcardStats(): Promise<FlashcardStatsDto> {
  const { data } = await coreApiClient.get<FlashcardStatsDto>("/flashcards/stats");
  return data;
}

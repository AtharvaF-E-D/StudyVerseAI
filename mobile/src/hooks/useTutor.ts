import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  createConversation,
  deleteConversation,
  getAiUsage,
  getConversationMessages,
  getConversations,
  sendMessage,
  toggleBookmark,
  type AiUsageResponse,
  type ConversationSummaryDto,
  type CreateConversationResponse,
  type MessageDto,
  type SendMessageResponse,
  type ToggleBookmarkResponse,
} from "../api/tutor";

/** Shared root key for every conversation-list variant (all search strings) — invalidating this prefix refreshes any of them. */
const conversationsListKey = ["tutor", "conversations"] as const;

/** Query key for one conversation's message list — messages are keyed by conversation id since each conversation has its own React Query cache entry. */
export function tutorMessagesQueryKey(conversationId: string) {
  return ["tutor", "conversations", conversationId, "messages"] as const;
}

export const tutorUsageQueryKey = ["tutor", "usage"] as const;

/** Fetches the signed-in user's conversation list, optionally filtered by `search`. */
export function useConversationsQuery(search?: string) {
  return useQuery<ConversationSummaryDto[]>({
    queryKey: [...conversationsListKey, search ?? ""],
    queryFn: () => getConversations({ search, take: 20 }),
  });
}

/** Fetches one conversation's full message history, oldest first. */
export function useConversationMessagesQuery(conversationId: string) {
  return useQuery<MessageDto[]>({
    queryKey: tutorMessagesQueryKey(conversationId),
    queryFn: () => getConversationMessages(conversationId),
    enabled: conversationId.length > 0,
  });
}

/** Starts a new (empty) conversation, then invalidates the conversation list so it appears there once the caller navigates to it. */
export function useCreateConversationMutation() {
  const queryClient = useQueryClient();

  return useMutation<CreateConversationResponse, unknown, void>({
    mutationFn: () => createConversation(),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: conversationsListKey });
    },
  });
}

/**
 * Sends a user message in `conversationId` and waits for the (non-streaming,
 * potentially multi-second) assistant reply. On success, the new user +
 * assistant messages are appended straight into the messages cache for an
 * immediate UI update, and both the messages cache and the conversation list
 * (whose preview/updatedAt just changed) are invalidated so a background
 * refetch reconciles with the server's authoritative state.
 */
export function useSendMessageMutation(conversationId: string) {
  const queryClient = useQueryClient();

  return useMutation<SendMessageResponse, unknown, string>({
    mutationFn: (content: string) => sendMessage(conversationId, content),
    onSuccess: (result) => {
      queryClient.setQueryData<MessageDto[]>(tutorMessagesQueryKey(conversationId), (previous) => [
        ...(previous ?? []),
        result.userMessage,
        result.assistantMessage,
      ]);
      void queryClient.invalidateQueries({ queryKey: tutorMessagesQueryKey(conversationId) });
      void queryClient.invalidateQueries({ queryKey: conversationsListKey });
    },
  });
}

/** Toggles a conversation's bookmark flag, then refetches the conversation list so `isBookmarked` reflects the server's result. */
export function useToggleBookmarkMutation() {
  const queryClient = useQueryClient();

  return useMutation<ToggleBookmarkResponse, unknown, string>({
    mutationFn: (conversationId: string) => toggleBookmark(conversationId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: conversationsListKey });
    },
  });
}

/** Deletes a conversation, then refetches the conversation list so it disappears. */
export function useDeleteConversationMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (conversationId: string) => deleteConversation(conversationId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: conversationsListKey });
    },
  });
}

/** Fetches today's AI token usage (used for the usage indicator on the chat screen). */
export function useAiUsageQuery() {
  return useQuery<AiUsageResponse>({
    queryKey: tutorUsageQueryKey,
    queryFn: getAiUsage,
  });
}

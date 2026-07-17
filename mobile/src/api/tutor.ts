import { isAxiosError } from "axios";

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// Shared shapes
// ---------------------------------------------------------------------------

export type MessageRole = "user" | "assistant";

export interface MessageDto {
  id: string;
  role: MessageRole;
  content: string;
  createdAtUtc: string;
}

export interface ConversationSummaryDto {
  id: string;
  title: string;
  isBookmarked: boolean;
  updatedAtUtc: string;
  lastMessagePreview: string;
}

// ---------------------------------------------------------------------------
// POST /tutor/conversations
// ---------------------------------------------------------------------------

export interface CreateConversationResponse {
  id: string;
  title: string;
  createdAtUtc: string;
}

export async function createConversation(): Promise<CreateConversationResponse> {
  const { data } = await coreApiClient.post<CreateConversationResponse>("/tutor/conversations");
  return data;
}

// ---------------------------------------------------------------------------
// GET /tutor/conversations?search=&take=
// ---------------------------------------------------------------------------

export interface GetConversationsParams {
  search?: string;
  take?: number;
}

export async function getConversations(
  params?: GetConversationsParams,
): Promise<ConversationSummaryDto[]> {
  const { data } = await coreApiClient.get<ConversationSummaryDto[]>("/tutor/conversations", {
    params: {
      search: params?.search || undefined,
      take: params?.take,
    },
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /tutor/conversations/{id}/messages
// ---------------------------------------------------------------------------

export async function getConversationMessages(conversationId: string): Promise<MessageDto[]> {
  const { data } = await coreApiClient.get<MessageDto[]>(
    `/tutor/conversations/${conversationId}/messages`,
  );
  return data;
}

// ---------------------------------------------------------------------------
// POST /tutor/conversations/{id}/messages
// ---------------------------------------------------------------------------

export interface SendMessageResponse {
  userMessage: MessageDto;
  assistantMessage: MessageDto;
  suggestedFollowUps: string[];
  tokensUsedToday: number;
  dailyLimit: number;
}

export async function sendMessage(
  conversationId: string,
  content: string,
): Promise<SendMessageResponse> {
  const { data } = await coreApiClient.post<SendMessageResponse>(
    `/tutor/conversations/${conversationId}/messages`,
    { content },
  );
  return data;
}

// ---------------------------------------------------------------------------
// DELETE /tutor/conversations/{id}
// ---------------------------------------------------------------------------

export async function deleteConversation(conversationId: string): Promise<void> {
  await coreApiClient.delete<void>(`/tutor/conversations/${conversationId}`);
}

// ---------------------------------------------------------------------------
// POST /tutor/conversations/{id}/bookmark
// ---------------------------------------------------------------------------

export interface ToggleBookmarkResponse {
  isBookmarked: boolean;
}

export async function toggleBookmark(conversationId: string): Promise<ToggleBookmarkResponse> {
  const { data } = await coreApiClient.post<ToggleBookmarkResponse>(
    `/tutor/conversations/${conversationId}/bookmark`,
  );
  return data;
}

// ---------------------------------------------------------------------------
// GET /tutor/usage
// ---------------------------------------------------------------------------

export interface AiUsageResponse {
  tokensUsedToday: number;
  dailyLimit: number;
  remaining: number;
}

export async function getAiUsage(): Promise<AiUsageResponse> {
  const { data } = await coreApiClient.get<AiUsageResponse>("/tutor/usage");
  return data;
}

// ---------------------------------------------------------------------------
// Error classification
// ---------------------------------------------------------------------------

/**
 * Best-effort check for "you've hit today's token cap" failures from
 * `POST /tutor/conversations/{id}/messages`. The exact shape of this error
 * isn't nailed down in the contract this client was built against (the
 * backend was being built in parallel), so this checks the conventional
 * signal (429 Too Many Requests) plus a keyword sniff of any server-provided
 * message/error string, rather than assuming one specific status code or
 * response shape. Update this once the real backend's cap-exceeded response
 * is confirmed.
 */
export function isDailyLimitError(error: unknown): boolean {
  if (!isAxiosError(error)) return false;
  if (error.response?.status === 429) return true;

  const data = error.response?.data as { message?: string; error?: string; code?: string } | undefined;
  const signal = `${data?.message ?? ""} ${data?.error ?? ""} ${data?.code ?? ""}`;
  return /daily.*limit|token.*limit|quota|usage.*cap/i.test(signal);
}

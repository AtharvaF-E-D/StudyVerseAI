import React, { useEffect, useState } from "react";
import { Alert, Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { TextField } from "../../../src/components/TextField";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { ListItem } from "../../../src/components/ListItem";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { formatRelativeTime } from "../../../src/lib/relativeTime";
import type { ConversationSummaryDto } from "../../../src/api/tutor";
import {
  useConversationsQuery,
  useCreateConversationMutation,
  useDeleteConversationMutation,
  useToggleBookmarkMutation,
} from "../../../src/hooks/useTutor";

const SEARCH_DEBOUNCE_MS = 300;

function ConversationListSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="flex-row items-center px-3 py-3">
          <Skeleton variant="circle" width={22} height={22} className="mr-3" />
          <View className="flex-1">
            <Skeleton variant="text" width="55%" className="mb-2" />
            <Skeleton variant="text" width="80%" />
          </View>
        </View>
      ))}
    </Card>
  );
}

/**
 * Conversation history/list screen — the tutor's entry point, reached from
 * the "Ask your AI tutor" card on the dashboard. Search filters server-side
 * via `useConversationsQuery(search)`; "New conversation" creates an empty
 * conversation and navigates straight into its chat screen.
 */
export default function TutorListScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [searchInput, setSearchInput] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");

  useEffect(() => {
    const timeout = setTimeout(() => setDebouncedSearch(searchInput.trim()), SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(timeout);
  }, [searchInput]);

  const conversationsQuery = useConversationsQuery(debouncedSearch || undefined);
  const createMutation = useCreateConversationMutation();
  const deleteMutation = useDeleteConversationMutation();
  const bookmarkMutation = useToggleBookmarkMutation();

  function openConversation(conversation: Pick<ConversationSummaryDto, "id" | "title">) {
    router.push({ pathname: `/(app)/tutor/${conversation.id}`, params: { title: conversation.title } });
  }

  function handleNewConversation() {
    if (createMutation.isPending) return;
    createMutation.mutate(undefined, {
      onSuccess: (conversation) => openConversation(conversation),
      onError: () => show("Couldn't start a new conversation. Please try again.", "danger"),
    });
  }

  function handleToggleBookmark(conversationId: string) {
    if (bookmarkMutation.isPending) return;
    bookmarkMutation.mutate(conversationId, {
      onError: () => show("Couldn't update the bookmark.", "danger"),
    });
  }

  function confirmDelete(conversation: ConversationSummaryDto) {
    Alert.alert("Delete conversation", `Delete "${conversation.title}"? This can't be undone.`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () =>
          deleteMutation.mutate(conversation.id, {
            onError: () => show("Couldn't delete that conversation.", "danger"),
          }),
      },
    ]);
  }

  const conversations = conversationsQuery.data ?? [];

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">AI Tutor</Text>
        <Button
          title="New conversation"
          onPress={handleNewConversation}
          loading={createMutation.isPending}
          fullWidth={false}
        />
      </View>

      <TextField
        label="Search"
        placeholder="Search conversations"
        value={searchInput}
        onChangeText={setSearchInput}
        autoCorrect={false}
      />

      {conversationsQuery.isLoading ? (
        <ConversationListSkeleton />
      ) : conversationsQuery.isError ? (
        <ErrorState
          title="Couldn't load your conversations"
          description="Check your connection and try again."
          onRetry={() => void conversationsQuery.refetch()}
        />
      ) : conversations.length === 0 ? (
        <EmptyState
          icon="chatbubbles-outline"
          title="No conversations yet"
          description="Ask your first question to start chatting with your AI tutor."
          actionLabel="Ask your first question"
          onAction={handleNewConversation}
        />
      ) : (
        <Card>
          {conversations.map((conversation, index) => (
            <React.Fragment key={conversation.id}>
              {index > 0 ? <Divider /> : null}
              {/* Bookmark/delete are siblings of ListItem, not inside its `trailing`
                  slot — ListItem renders as a <button> when `onPress` is set, and
                  nesting other pressables inside it produces invalid HTML (<button>
                  cannot contain a nested <button>) that real browsers warn about
                  and mishandle click-wise. */}
              <View className="flex-row items-center">
                <ListItem
                  leading={<Icon name="chatbubble-ellipses-outline" size={22} color={colors.textSecondary} />}
                  title={conversation.title}
                  subtitle={conversation.lastMessagePreview || "No messages yet"}
                  onPress={() => openConversation(conversation)}
                  trailing={
                    <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                      {formatRelativeTime(conversation.updatedAtUtc)}
                    </Text>
                  }
                  className="flex-1"
                />
                <Pressable
                  onPress={() => handleToggleBookmark(conversation.id)}
                  hitSlop={8}
                  accessibilityRole="button"
                  accessibilityLabel={conversation.isBookmarked ? "Remove bookmark" : "Bookmark conversation"}
                  className="ml-2"
                >
                  <Icon
                    name={conversation.isBookmarked ? "star" : "star-outline"}
                    size={19}
                    color={conversation.isBookmarked ? colors.warning : colors.textSecondary}
                  />
                </Pressable>
                <Pressable
                  onPress={() => confirmDelete(conversation)}
                  hitSlop={8}
                  accessibilityRole="button"
                  accessibilityLabel="Delete conversation"
                  className="ml-2 mr-3"
                >
                  <Icon name="trash-outline" size={19} color={colors.danger} />
                </Pressable>
              </View>
            </React.Fragment>
          ))}
        </Card>
      )}
    </ScreenContainer>
  );
}

import React, { useState } from "react";
import { Pressable, ScrollView, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { TextField } from "../../../src/components/TextField";
import { Button } from "../../../src/components/Button";
import { Chip } from "../../../src/components/Chip";
import { Icon } from "../../../src/components/Icon";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Skeleton } from "../../../src/components/Skeleton";
import { ChatBubble } from "../../../src/components/tutor/ChatBubble";
import { ThinkingBubble } from "../../../src/components/tutor/ThinkingBubble";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { isDailyLimitError } from "../../../src/api/tutor";
import { useAiUsageQuery, useConversationMessagesQuery, useSendMessageMutation } from "../../../src/hooks/useTutor";

function MessagesSkeleton() {
  return (
    <View>
      <View className="mb-3 max-w-[70%] self-start">
        <Skeleton variant="rect" width="100%" height={52} className="rounded-2xl" />
      </View>
      <View className="mb-3 max-w-[60%] self-end">
        <Skeleton variant="rect" width="100%" height={40} className="rounded-2xl" />
      </View>
      <View className="mb-3 max-w-[75%] self-start">
        <Skeleton variant="rect" width="100%" height={68} className="rounded-2xl" />
      </View>
    </View>
  );
}

/**
 * One conversation's chat screen. Reached either from the conversation list
 * (`app/(app)/tutor/index.tsx`, which passes `title` as a query param since
 * the backend contract has no "get one conversation" endpoint to fetch it
 * from here) or immediately after creating a new conversation.
 */
export default function TutorConversationScreen() {
  const params = useLocalSearchParams<{ id: string; title?: string }>();
  const conversationId = params.id ?? "";
  const screenTitle = params.title || "AI Tutor";

  const { colors } = useTheme();
  const { show } = useToast();
  const scrollRef = React.useRef<ScrollView>(null);

  const messagesQuery = useConversationMessagesQuery(conversationId);
  const sendMutation = useSendMessageMutation(conversationId);
  const usageQuery = useAiUsageQuery();

  const [draft, setDraft] = useState("");
  const [pendingUserText, setPendingUserText] = useState<string | null>(null);
  const [followUps, setFollowUps] = useState<string[]>([]);
  const [dailyLimitReached, setDailyLimitReached] = useState(false);

  function handleSend(overrideText?: string) {
    const text = (overrideText ?? draft).trim();
    if (!text || sendMutation.isPending) return;

    setDraft("");
    setFollowUps([]);
    setDailyLimitReached(false);
    setPendingUserText(text);

    sendMutation.mutate(text, {
      onSuccess: (result) => {
        setPendingUserText(null);
        setFollowUps(result.suggestedFollowUps ?? []);
      },
      onError: (error) => {
        setPendingUserText(null);
        setDraft(text); // don't make the user retype it
        if (isDailyLimitError(error)) {
          setDailyLimitReached(true);
          show("You've reached today's AI tutor limit.", "warning");
        } else {
          show("Couldn't get a reply. Please try again.", "danger");
        }
      },
    });
  }

  const messages = messagesQuery.data ?? [];
  const isEmpty = messages.length === 0 && !pendingUserText && !sendMutation.isPending;

  return (
    <ScreenContainer scrollable={false}>
      <View className="flex-1">
        {/* Header: back + title + usage indicator */}
        <View className="mb-4 flex-row items-center">
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
              {screenTitle}
            </Text>
            {usageQuery.data ? (
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                {usageQuery.data.tokensUsedToday.toLocaleString()} / {usageQuery.data.dailyLimit.toLocaleString()}{" "}
                tokens today
              </Text>
            ) : null}
          </View>
        </View>

        {/* Message list */}
        <ScrollView
          ref={scrollRef}
          className="flex-1"
          contentContainerClassName="pb-2"
          keyboardShouldPersistTaps="handled"
          onContentSizeChange={() => scrollRef.current?.scrollToEnd({ animated: true })}
        >
          {messagesQuery.isLoading ? (
            <MessagesSkeleton />
          ) : messagesQuery.isError ? (
            <ErrorState
              title="Couldn't load this conversation"
              description="Check your connection and try again."
              onRetry={() => void messagesQuery.refetch()}
            />
          ) : (
            <>
              {isEmpty ? (
                <EmptyState
                  icon="chatbubbles-outline"
                  title="Ask anything"
                  description="Send a question below to start this conversation."
                />
              ) : null}

              {messages.map((message) => (
                <ChatBubble key={message.id} message={message} />
              ))}

              {pendingUserText ? <ChatBubble message={{ role: "user", content: pendingUserText }} /> : null}
              {sendMutation.isPending ? <ThinkingBubble /> : null}

              {!sendMutation.isPending && followUps.length > 0 ? (
                <View className="mb-3 flex-row flex-wrap gap-2 self-start">
                  {followUps.map((followUp) => (
                    <Chip key={followUp} label={followUp} onPress={() => handleSend(followUp)} />
                  ))}
                </View>
              ) : null}
            </>
          )}
        </ScrollView>

        {/* Composer */}
        <View className="border-t border-border pt-3 dark:border-border-dark">
          {dailyLimitReached ? (
            <View className="mb-2 flex-row items-center rounded-lg bg-warning/10 px-3 py-2.5">
              <Icon name="time-outline" size={16} color={colors.warning} />
              <Text className="ml-2 flex-1 text-caption text-ink-primary dark:text-ink-primary-dark">
                You&apos;ve reached today&apos;s AI tutor limit. It resets tomorrow — nice work studying today!
              </Text>
            </View>
          ) : null}
          <View className="flex-row items-end">
            <View className="mr-2 flex-1">
              <TextField
                label="Message"
                placeholder="Ask your tutor anything..."
                value={draft}
                onChangeText={setDraft}
                onSubmitEditing={() => handleSend()}
                multiline
                containerClassName="mb-0"
                editable={!dailyLimitReached}
              />
            </View>
            <Button
              title="Send"
              onPress={() => handleSend()}
              loading={sendMutation.isPending}
              disabled={!draft.trim() || dailyLimitReached}
              fullWidth={false}
            />
          </View>
        </View>
      </View>
    </ScreenContainer>
  );
}

import React from "react";
import { View } from "react-native";

import { MessageContent } from "./MessageContent";
import { useTheme } from "../../theme/ThemeProvider";
import type { MessageDto } from "../../api/tutor";

export interface ChatBubbleProps {
  message: Pick<MessageDto, "role" | "content">;
}

/** One chat bubble — right-aligned brand-colored for the user, left-aligned surface-colored for the assistant. */
export function ChatBubble({ message }: ChatBubbleProps) {
  const { colors } = useTheme();
  const isUser = message.role === "user";

  return (
    <View className={["mb-3 max-w-[85%]", isUser ? "self-end" : "self-start"].join(" ")}>
      <View
        className={[
          "rounded-2xl px-4 py-3",
          isUser
            ? "rounded-br-sm bg-brand dark:bg-brand-dark"
            : "rounded-bl-sm border border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
        ].join(" ")}
      >
        <MessageContent
          content={message.content}
          textColor={isUser ? "#FFFFFF" : colors.textPrimary}
          textClassName={isUser ? "text-body text-white" : "text-body text-ink-primary dark:text-ink-primary-dark"}
        />
      </View>
    </View>
  );
}

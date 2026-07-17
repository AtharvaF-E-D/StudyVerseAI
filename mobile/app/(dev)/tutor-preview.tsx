import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { ListItem } from "../../src/components/ListItem";
import { Icon } from "../../src/components/Icon";
import { Chip } from "../../src/components/Chip";
import { ChatBubble } from "../../src/components/tutor/ChatBubble";
import { ThinkingBubble } from "../../src/components/tutor/ThinkingBubble";
import { useTheme } from "../../src/theme/ThemeProvider";
import { formatRelativeTime } from "../../src/lib/relativeTime";
import type { ConversationSummaryDto, MessageDto } from "../../src/api/tutor";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/tutor-preview`. The real backend has no `/api/v1/tutor/*`
// controller yet (it's being built in parallel with this mobile work), so
// this feeds hand-written fixture messages/conversations straight into the
// same rendering components the real screens use (`ChatBubble` →
// `MessageContent` → `MathRenderer`/`CodeBlock`) for manual/automated visual
// QA of math + code rendering in both light and dark mode. None of the data
// below comes from — or is wired to — the real tutor API. Delete this file
// once the real backend is reachable and the chat/list screens have been
// re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_MESSAGES: Pick<MessageDto, "id" | "role" | "content">[] = [
  { id: "m1", role: "user", content: "Can you explain the quadratic formula?" },
  {
    id: "m2",
    role: "assistant",
    content:
      "Sure! For any equation of the form $$ax^2 + bx + c = 0$$ the quadratic formula is $x = \\frac{-b \\pm \\sqrt{b^2-4ac}}{2a}$.",
  },
  { id: "m3", role: "user", content: "Great, now can you show me a bubble sort in Python?" },
  {
    id: "m4",
    role: "assistant",
    content:
      "Here's a simple bubble sort:\n\n```python\ndef bubble_sort(items):\n    n = len(items)\n    for i in range(n):\n        for j in range(0, n - i - 1):\n            if items[j] > items[j + 1]:\n                items[j], items[j + 1] = items[j + 1], items[j]\n    return items\n```\n\nIt repeatedly swaps adjacent out-of-order elements until the list is fully sorted.",
  },
  { id: "m5", role: "user", content: "Thanks, that's really clear!" },
  {
    id: "m6",
    role: "assistant",
    content: "You're welcome! Let me know if you'd like to practice with a few example problems.",
  },
];

const FIXTURE_FOLLOW_UPS = ["Show me a worked example", "What about complex roots?"];

const FIXTURE_CONVERSATIONS: ConversationSummaryDto[] = [
  {
    id: "c1",
    title: "Quadratic formula help",
    isBookmarked: true,
    updatedAtUtc: "2026-07-17T09:30:00Z",
    lastMessagePreview: "You're welcome! Let me know if you'd like to practice...",
  },
  {
    id: "c2",
    title: "Bubble sort in Python",
    isBookmarked: false,
    updatedAtUtc: "2026-07-16T18:00:00Z",
    lastMessagePreview: "Here's a simple bubble sort: def bubble_sort...",
  },
  {
    id: "c3",
    title: "Photosynthesis overview",
    isBookmarked: false,
    updatedAtUtc: "2026-07-10T08:00:00Z",
    lastMessagePreview: "Photosynthesis converts light energy into chemical energy...",
  },
];

function ChatFixtureSection() {
  const [showThinking, setShowThinking] = useState(false);

  return (
    <View className="mb-10">
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">
          Fixture: chat screen (math + code + plain text)
        </Text>
        <Button
          title={showThinking ? "Hide thinking" : "Show thinking"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setShowThinking((prev) => !prev)}
        />
      </View>
      <Card>
        {FIXTURE_MESSAGES.map((message) => (
          <ChatBubble key={message.id} message={message} />
        ))}
        {showThinking ? <ThinkingBubble /> : null}
        <View className="flex-row flex-wrap gap-2 self-start">
          {FIXTURE_FOLLOW_UPS.map((followUp) => (
            <Chip key={followUp} label={followUp} />
          ))}
        </View>
      </Card>
    </View>
  );
}

function ConversationListFixtureSection() {
  const { colors } = useTheme();

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: conversation list
      </Text>
      <Card>
        {FIXTURE_CONVERSATIONS.map((conversation, index) => (
          <React.Fragment key={conversation.id}>
            {index > 0 ? <Divider /> : null}
            <ListItem
              leading={<Icon name="chatbubble-ellipses-outline" size={22} color={colors.textSecondary} />}
              title={conversation.title}
              subtitle={conversation.lastMessagePreview}
              trailing={
                <View className="flex-row items-center">
                  <Text className="mr-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                    {formatRelativeTime(conversation.updatedAtUtc)}
                  </Text>
                  <Icon
                    name={conversation.isBookmarked ? "star" : "star-outline"}
                    size={19}
                    color={conversation.isBookmarked ? colors.warning : colors.textSecondary}
                  />
                </View>
              }
            />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Phase 4 AI tutor UI, mirroring
 * the pattern established by `app/(dev)/dashboard-preview.tsx`. Not linked
 * from any navigation — reached directly at `/(dev)/tutor-preview`.
 */
export default function TutorPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Tutor Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <ChatFixtureSection />
      <ConversationListFixtureSection />
    </ScreenContainer>
  );
}

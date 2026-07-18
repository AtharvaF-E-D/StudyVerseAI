import React from "react";
import { Alert, Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { DashboardContent } from "../../src/components/dashboard/DashboardContent";
import { useTheme } from "../../src/theme/ThemeProvider";
import type { DashboardResponse } from "../../src/api/dashboard";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/dashboard-preview`. Feeds `DashboardContent` (the same component
// `app/(app)/index.tsx` renders against real data) two hand-written mocked
// fixtures purely for manual/automated visual QA of the layout in both
// light and dark mode. None of the data below comes from — or is wired to —
// the real dashboard API; delete this whole file once Phase 3 is verified.
// ---------------------------------------------------------------------------

const MID_PROGRESS_FIXTURE: DashboardResponse = {
  xp: 1240,
  level: 6,
  coins: 350,
  streak: { currentDays: 4, longestDays: 9, studiedToday: true },
  todaysChallenges: [
    {
      id: "c1",
      title: "Finish Algebra quiz",
      description: "Complete 10 questions on linear equations",
      xpReward: 20,
      coinReward: 5,
      isCompleted: true,
    },
    {
      id: "c2",
      title: "Review flashcards",
      description: "Go through your Biology deck",
      xpReward: 15,
      coinReward: 5,
      isCompleted: false,
    },
    {
      id: "c3",
      title: "Study with the AI tutor",
      description: "Ask the tutor 3 questions",
      xpReward: 25,
      coinReward: 10,
      isCompleted: false,
    },
  ],
  weeklyActivity: [
    { date: "2026-07-11", xpEarned: 0 },
    { date: "2026-07-12", xpEarned: 20 },
    { date: "2026-07-13", xpEarned: 45 },
    { date: "2026-07-14", xpEarned: 10 },
    { date: "2026-07-15", xpEarned: 0 },
    { date: "2026-07-16", xpEarned: 60 },
    { date: "2026-07-17", xpEarned: 35 },
  ],
  leaderboard: {
    myRank: 4,
    top: [
      { userId: "u-alice", displayName: "Alice Chen", xp: 5200, rank: 1 },
      { userId: "u-ben", displayName: "Ben Osei", xp: 4100, rank: 2 },
      { userId: "u-carla", displayName: "Carla Diaz", xp: 3800, rank: 3 },
      { userId: "me", displayName: "Jordan Lee", xp: 1240, rank: 4 },
      { userId: "u-emeka", displayName: "Emeka Obi", xp: 1100, rank: 5 },
    ],
  },
  notifications: {
    unreadCount: 2,
    recent: [
      {
        id: "n1",
        title: "Streak reminder",
        body: "Study today to keep your 4-day streak alive!",
        createdAtUtc: "2026-07-17T08:00:00Z",
        readAtUtc: null,
      },
      {
        id: "n2",
        title: "New badge earned",
        body: "You unlocked the \"Quick Learner\" badge.",
        createdAtUtc: "2026-07-16T18:30:00Z",
        readAtUtc: null,
      },
      {
        id: "n3",
        title: "Weekly recap",
        body: "You earned 170 XP this week — nice work.",
        createdAtUtc: "2026-07-15T09:00:00Z",
        readAtUtc: "2026-07-16T10:00:00Z",
      },
    ],
  },
};

const EMPTY_FIXTURE: DashboardResponse = {
  xp: 0,
  level: 1,
  coins: 0,
  streak: { currentDays: 0, longestDays: 0, studiedToday: false },
  todaysChallenges: [
    {
      id: "c1",
      title: "Finish your first quiz",
      description: "Complete any quiz to earn XP",
      xpReward: 10,
      coinReward: 5,
      isCompleted: false,
    },
    {
      id: "c2",
      title: "Create a flashcard deck",
      description: "Add at least 5 cards to a new deck",
      xpReward: 10,
      coinReward: 5,
      isCompleted: false,
    },
    {
      id: "c3",
      title: "Chat with the AI tutor",
      description: "Ask your first question",
      xpReward: 15,
      coinReward: 5,
      isCompleted: false,
    },
  ],
  weeklyActivity: [
    { date: "2026-07-11", xpEarned: 0 },
    { date: "2026-07-12", xpEarned: 0 },
    { date: "2026-07-13", xpEarned: 0 },
    { date: "2026-07-14", xpEarned: 0 },
    { date: "2026-07-15", xpEarned: 0 },
    { date: "2026-07-16", xpEarned: 0 },
    { date: "2026-07-17", xpEarned: 0 },
  ],
  leaderboard: {
    // A brand new user's own stats are all zero, but the community
    // leaderboard already has active users on it — the empty state only
    // applies to *this* user's data, not the whole app.
    myRank: 342,
    top: [
      { userId: "u-alice", displayName: "Alice Chen", xp: 5200, rank: 1 },
      { userId: "u-ben", displayName: "Ben Osei", xp: 4100, rank: 2 },
      { userId: "u-carla", displayName: "Carla Diaz", xp: 3800, rank: 3 },
    ],
  },
  notifications: {
    unreadCount: 0,
    recent: [],
  },
};

function noop() {
  // Dev preview stub — no real mutations are wired up here.
}

function stubLogout() {
  Alert.alert("Dev preview", "Logout is not wired up in this preview screen.");
}

function FixtureSection({
  title,
  data,
  currentUserId,
  flashcardsDueToday,
}: {
  title: string;
  data: DashboardResponse;
  currentUserId: string;
  flashcardsDueToday: number;
}) {
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">{title}</Text>
      <DashboardContent
        displayName="Jordan"
        data={data}
        currentUserId={currentUserId}
        onLogout={stubLogout}
        onCompleteChallenge={noop}
        onMarkNotificationRead={noop}
        flashcardsDueToday={flashcardsDueToday}
      />
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Phase 3 dashboard, mirroring
 * the pattern established by `app/(dev)/components.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/dashboard-preview`.
 */
export default function DashboardPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Dashboard Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <FixtureSection
        title="Fixture: mid-progress user"
        data={MID_PROGRESS_FIXTURE}
        currentUserId="me"
        flashcardsDueToday={6}
      />
      <FixtureSection
        title="Fixture: empty / fresh user"
        data={EMPTY_FIXTURE}
        currentUserId="fresh-user"
        flashcardsDueToday={0}
      />
    </ScreenContainer>
  );
}

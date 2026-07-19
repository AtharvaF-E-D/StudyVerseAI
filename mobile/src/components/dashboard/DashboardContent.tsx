import React from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { Avatar } from "../Avatar";
import { Badge } from "../Badge";
import { Card } from "../Card";
import { Divider } from "../Divider";
import { EmptyState } from "../EmptyState";
import { Icon } from "../Icon";
import { ListItem } from "../ListItem";
import { useTheme } from "../../theme/ThemeProvider";
import { getTimeBasedGreeting } from "../../lib/greeting";
import type { DashboardResponse } from "../../api/dashboard";

export interface DashboardContentProps {
  displayName: string;
  data: DashboardResponse;
  /** The signed-in user's id, used to highlight their own row in the leaderboard preview. */
  currentUserId?: string;
  isLoggingOut?: boolean;
  onLogout: () => void;
  onCompleteChallenge: (challengeId: string) => void;
  /** Id of the challenge currently being completed, if any — disables that row and shows a spinner in place of its leading icon. */
  completingChallengeId?: string | null;
  onMarkNotificationRead: (notificationId: string) => void;
  /** Id of the notification currently being marked read, if any. */
  markingNotificationId?: string | null;
  /**
   * Count of flashcards due for review today, for the "Flashcards" entry
   * point's badge. Comes from a separate `useFlashcardStatsQuery()` call in
   * `app/(app)/index.tsx` rather than `DashboardResponse` itself — flashcards
   * is its own feature area/backend contract, not part of the dashboard
   * payload — so it's threaded in as a prop to keep this component purely
   * presentational (same reason every other field here arrives via props).
   */
  flashcardsDueToday?: number;
  /**
   * Active study plan snapshot for the "Study Planner" entry point, from a
   * separate `useActivePlanQuery()`/`useTodayTasksQuery()` pair in
   * `app/(app)/index.tsx` — same reasoning as `flashcardsDueToday` above.
   * `undefined` covers both "still loading" and "no active plan yet", both
   * of which render the same "create a plan" prompt.
   */
  studyPlanSummary?: { daysRemaining: number; todayTaskCount: number };
  /**
   * Coding Practice snapshot for its entry point, from a separate
   * `useCodingStatsQuery()` call in `app/(app)/index.tsx` — same reasoning
   * as `flashcardsDueToday`/`studyPlanSummary` above (its own feature
   * area/backend contract, not part of `DashboardResponse`). `undefined`
   * covers both "still loading" and "no problems solved yet", both of which
   * render the same generic subtitle.
   */
  codingStatsSummary?: { totalSolved: number; currentDailyStreak: number };
  /**
   * Gamification hub snapshot for its entry point, from a separate
   * `useGamificationSummaryQuery()` call in `app/(app)/index.tsx` — same
   * reasoning as `flashcardsDueToday`/`studyPlanSummary`/`codingStatsSummary`
   * above (its own feature area/backend contract, not part of
   * `DashboardResponse`). `undefined` covers both "still loading" and
   * "summary unavailable", both of which render a generic subtitle with no
   * badge/indicator.
   */
  gamificationSummary?: {
    badgesEarnedCount: number;
    totalBadgesCount: number;
    dailyRewardReady: boolean;
  };
}

function SectionTitle({ children, trailing }: { children: string; trailing?: string }) {
  return (
    <View className="mb-3 flex-row items-center justify-between">
      <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{children}</Text>
      {trailing ? (
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{trailing}</Text>
      ) : null}
    </View>
  );
}

function shortWeekdayLabel(dateStr: string): string {
  // `dateStr` is "yyyy-MM-dd"; anchoring to local midnight avoids the date
  // shifting a day backward/forward depending on the reader's timezone.
  const parsed = new Date(`${dateStr}T00:00:00`);
  return new Intl.DateTimeFormat("en-US", { weekday: "short" }).format(parsed).slice(0, 1);
}

const WEEK_BAR_MAX_HEIGHT = 56;
const WEEK_BAR_MIN_HEIGHT = 8;

/**
 * Pure presentational dashboard body — the home screen at `app/(app)/index.tsx`
 * wires it to real data/mutations, and `app/(dev)/dashboard-preview.tsx` feeds
 * it mocked fixtures for visual QA. Keeping all the layout here (rather than
 * inline in the route file) is what lets the dev preview reuse the exact
 * same rendering code without stubbing any hooks.
 */
export function DashboardContent({
  displayName,
  data,
  currentUserId,
  isLoggingOut = false,
  onLogout,
  onCompleteChallenge,
  completingChallengeId = null,
  onMarkNotificationRead,
  markingNotificationId = null,
  flashcardsDueToday = 0,
  studyPlanSummary,
  codingStatsSummary,
  gamificationSummary,
}: DashboardContentProps) {
  const { colors } = useTheme();

  const allChallengesDone =
    data.todaysChallenges.length > 0 && data.todaysChallenges.every((c) => c.isCompleted);

  const weekMaxXp = Math.max(0, ...data.weeklyActivity.map((d) => d.xpEarned));
  const weekHasActivity = weekMaxXp > 0;
  const weekTotalXp = data.weeklyActivity.reduce((sum, d) => sum + d.xpEarned, 0);

  const isCallerInTop = data.leaderboard.top.some((entry) => entry.userId === currentUserId);

  return (
    <View>
      {/* Header: greeting + logout */}
      <View className="mb-6 flex-row items-center justify-between">
        <View className="flex-1 pr-3">
          <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">
            {getTimeBasedGreeting()}, {displayName}
          </Text>
        </View>
        <Pressable
          accessibilityRole="button"
          accessibilityLabel="Log out"
          disabled={isLoggingOut}
          onPress={onLogout}
          className="h-10 w-10 items-center justify-center rounded-full bg-surface active:opacity-70 dark:bg-surface-dark"
        >
          {isLoggingOut ? (
            <ActivityIndicator size="small" color={colors.textSecondary} />
          ) : (
            <Icon name="log-out-outline" size={20} color={colors.textSecondary} />
          )}
        </Pressable>
      </View>

      {/* Streak / level / coins summary */}
      <Card className="mb-6">
        <View className="flex-row items-center justify-between">
          <View className="flex-1 items-center">
            <Icon name="flame" size={26} color={colors.warning} />
            <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
              {data.streak.currentDays}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              day streak
            </Text>
          </View>
          <View className="flex-1 items-center">
            <Icon name="star" size={26} color={colors.brand} />
            <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
              Lvl {data.level}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {data.xp} XP
            </Text>
          </View>
          <View className="flex-1 items-center">
            <Icon name="cash-outline" size={26} color={colors.accent} />
            <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
              {data.coins}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">coins</Text>
          </View>
        </View>
        <View className="mt-4 items-center">
          <Badge
            label={data.streak.studiedToday ? "Studied today" : "Study today to keep your streak"}
            variant={data.streak.studiedToday ? "success" : "warning"}
          />
        </View>
      </Card>

      {/* Today's challenges */}
      <View className="mb-6">
        <SectionTitle>Today&apos;s challenges</SectionTitle>
        <Card>
          {allChallengesDone ? (
            <EmptyState
              icon="checkmark-circle-outline"
              title="All done for today!"
              description="New challenges will be waiting for you tomorrow."
            />
          ) : (
            data.todaysChallenges.map((challenge, index) => {
              const isCompleting = completingChallengeId === challenge.id;
              return (
                <React.Fragment key={challenge.id}>
                  {index > 0 ? <Divider /> : null}
                  <ListItem
                    leading={
                      isCompleting ? (
                        <ActivityIndicator size="small" color={colors.brand} />
                      ) : (
                        <Icon
                          name={challenge.isCompleted ? "checkmark-circle" : "ellipse-outline"}
                          size={22}
                          color={challenge.isCompleted ? colors.success : colors.textSecondary}
                        />
                      )
                    }
                    title={challenge.title}
                    subtitle={challenge.description}
                    trailing={
                      <View className="items-end">
                        <Text className="text-caption font-medium text-brand dark:text-brand-light">
                          +{challenge.xpReward} XP
                        </Text>
                        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                          +{challenge.coinReward} coins
                        </Text>
                      </View>
                    }
                    onPress={
                      challenge.isCompleted || isCompleting
                        ? undefined
                        : () => onCompleteChallenge(challenge.id)
                    }
                  />
                </React.Fragment>
              );
            })
          )}
        </Card>
      </View>

      {/* Weekly activity */}
      <View className="mb-6">
        <SectionTitle trailing={`${weekTotalXp} XP this week`}>This week</SectionTitle>
        <Card>
          <View
            className="flex-row items-end justify-between"
            style={{ height: WEEK_BAR_MAX_HEIGHT }}
          >
            {data.weeklyActivity.map((day) => {
              const barHeight = weekHasActivity
                ? Math.round(
                    WEEK_BAR_MIN_HEIGHT +
                      (day.xpEarned / weekMaxXp) * (WEEK_BAR_MAX_HEIGHT - WEEK_BAR_MIN_HEIGHT),
                  )
                : WEEK_BAR_MIN_HEIGHT;
              return (
                <View key={day.date} className="flex-1 items-center px-1">
                  <View
                    className="w-full rounded-t-md bg-brand dark:bg-brand-light"
                    style={{ height: barHeight, opacity: day.xpEarned > 0 ? 1 : 0.3 }}
                  />
                </View>
              );
            })}
          </View>
          <View className="mt-2 flex-row justify-between">
            {data.weeklyActivity.map((day, index) => {
              const isToday = index === data.weeklyActivity.length - 1;
              return (
                <View key={day.date} className="flex-1 items-center">
                  <Text
                    className={
                      isToday
                        ? "text-caption font-semibold text-ink-primary dark:text-ink-primary-dark"
                        : "text-caption text-ink-secondary dark:text-ink-secondary-dark"
                    }
                  >
                    {shortWeekdayLabel(day.date)}
                  </Text>
                </View>
              );
            })}
          </View>
        </Card>
      </View>

      {/* Leaderboard preview */}
      <View className="mb-6">
        <SectionTitle>Leaderboard</SectionTitle>
        <Card>
          {data.leaderboard.top.length === 0 ? (
            <EmptyState icon="trophy-outline" title="No leaderboard data yet" />
          ) : (
            data.leaderboard.top.map((entry, index) => {
              const isMe = entry.userId === currentUserId;
              return (
                <React.Fragment key={entry.userId}>
                  {index > 0 ? <Divider /> : null}
                  <ListItem
                    leading={<Avatar name={entry.displayName} size="sm" />}
                    title={isMe ? `${entry.displayName} (you)` : entry.displayName}
                    subtitle={`${entry.xp} XP`}
                    trailing={<Badge label={`#${entry.rank}`} variant={isMe ? "brand" : "neutral"} />}
                    className={isMe ? "rounded-lg bg-brand/10 dark:bg-brand-light/10" : ""}
                  />
                </React.Fragment>
              );
            })
          )}
          {!isCallerInTop && data.leaderboard.myRank > 0 ? (
            <>
              <Divider />
              <View className="px-3 py-3">
                <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                  You&apos;re currently ranked{" "}
                  <Text className="font-semibold text-ink-primary dark:text-ink-primary-dark">
                    #{data.leaderboard.myRank}
                  </Text>
                </Text>
              </View>
            </>
          ) : null}
        </Card>
      </View>

      {/* Notifications */}
      <View className="mb-6">
        <SectionTitle
          trailing={
            data.notifications.unreadCount > 0 ? `${data.notifications.unreadCount} unread` : undefined
          }
        >
          Notifications
        </SectionTitle>
        <Card>
          {data.notifications.recent.length === 0 ? (
            <EmptyState icon="notifications-outline" title="No notifications yet" />
          ) : (
            data.notifications.recent.map((notification, index) => {
              const isUnread = notification.readAtUtc === null;
              const isMarking = markingNotificationId === notification.id;
              return (
                <React.Fragment key={notification.id}>
                  {index > 0 ? <Divider /> : null}
                  <ListItem
                    leading={
                      <View
                        className="h-2 w-2 rounded-full"
                        style={{ backgroundColor: isUnread ? colors.brand : "transparent" }}
                      />
                    }
                    title={notification.title}
                    subtitle={notification.body}
                    trailing={isMarking ? <ActivityIndicator size="small" color={colors.brand} /> : null}
                    onPress={isUnread ? () => onMarkNotificationRead(notification.id) : undefined}
                  />
                </React.Fragment>
              );
            })
          )}
        </Card>
      </View>

      {/* AI tutor entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="sparkles" size={22} color={colors.brand} />}
            title="Ask your AI tutor"
            subtitle="Get help with anything you're studying"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/tutor")}
          />
        </Card>
      </View>

      {/* Rapid Fire Quiz entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="flash" size={22} color={colors.warning} />}
            title="Rapid Fire Quiz"
            subtitle="Race the clock, build a streak, beat your best combo"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/quiz")}
          />
        </Card>
      </View>

      {/* AI Notes entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="document-text" size={22} color={colors.accent} />}
            title="AI Notes"
            subtitle="Turn a document or photo into a summary, flashcards, and more"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/notes")}
          />
        </Card>
      </View>

      {/* Flashcards entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="albums" size={22} color={colors.brand} />}
            title="Flashcards"
            subtitle="Create decks and review with spaced repetition"
            trailing={
              <View className="flex-row items-center">
                {flashcardsDueToday > 0 ? (
                  <Badge label={`${flashcardsDueToday} due`} variant="brand" className="mr-2" />
                ) : null}
                <Icon name="chevron-forward" size={18} color={colors.textSecondary} />
              </View>
            }
            onPress={() => router.push("/(app)/flashcards")}
          />
        </Card>
      </View>

      {/* Mock Tests entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="school" size={22} color={colors.success} />}
            title="Mock Tests"
            subtitle="Take a timed practice exam and see how you rank"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/mocktests")}
          />
        </Card>
      </View>

      {/* Coding Practice entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="code-slash" size={22} color={colors.brand} />}
            title="Coding Practice"
            subtitle={
              codingStatsSummary
                ? `${codingStatsSummary.totalSolved} solved · ${codingStatsSummary.currentDailyStreak} day streak`
                : "Solve real problems, graded instantly against real test cases"
            }
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/coding")}
          />
        </Card>
      </View>

      {/* Study Planner entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="calendar" size={22} color={colors.success} />}
            title="Study Planner"
            subtitle={
              studyPlanSummary
                ? `${studyPlanSummary.daysRemaining} day${studyPlanSummary.daysRemaining === 1 ? "" : "s"} left · ${studyPlanSummary.todayTaskCount} task${studyPlanSummary.todayTaskCount === 1 ? "" : "s"} today`
                : "Create an AI-generated day-by-day study plan"
            }
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/studyplanner")}
          />
        </Card>
      </View>

      {/* Interview Prep entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="briefcase" size={22} color={colors.brand} />}
            title="Interview Prep"
            subtitle="Practice real Q&A sessions and get real AI resume feedback"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/interview")}
          />
        </Card>
      </View>

      {/* Current Affairs entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="newspaper" size={22} color={colors.accent} />}
            title="Current Affairs"
            subtitle="Browse the latest news, then test your understanding"
            trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
            onPress={() => router.push("/(app)/currentaffairs")}
          />
        </Card>
      </View>

      {/* Gamification hub entry point */}
      <View className="mb-6">
        <Card>
          <ListItem
            leading={<Icon name="trophy" size={22} color={colors.warning} />}
            title="Rewards"
            subtitle={
              gamificationSummary
                ? `${gamificationSummary.badgesEarnedCount}/${gamificationSummary.totalBadgesCount} badges earned`
                : "Badges, missions, daily rewards, and the spin wheel"
            }
            trailing={
              <View className="flex-row items-center">
                {gamificationSummary?.dailyRewardReady ? (
                  <Badge label="Reward ready!" variant="success" className="mr-2" />
                ) : null}
                <Icon name="chevron-forward" size={18} color={colors.textSecondary} />
              </View>
            }
            onPress={() => router.push("/(app)/gamification")}
          />
        </Card>
      </View>

      {/* Continue learning / AI recommendations — intentionally honest: no feature produces this data yet. */}
      <View>
        <SectionTitle>Continue learning</SectionTitle>
        <Card>
          <EmptyState
            icon="book-outline"
            title="Nothing to continue yet"
            description="Check back here once you start studying — your recent quizzes, flashcards, and AI tutor sessions will show up in this space."
          />
        </Card>
      </View>
    </View>
  );
}

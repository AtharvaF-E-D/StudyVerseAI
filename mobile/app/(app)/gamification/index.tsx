import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { BadgeTile } from "../../../src/components/gamification/BadgeTile";
import { MissionRow } from "../../../src/components/gamification/MissionRow";
import { DailyRewardCard } from "../../../src/components/gamification/DailyRewardCard";
import { SpinWheelCard, type SpinPhase } from "../../../src/components/gamification/SpinWheelCard";
import { SPIN_WHEEL_SEGMENT_COUNT } from "../../../src/components/gamification/SpinWheel";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { ClaimDailyRewardResponse, SpinResponse } from "../../../src/api/gamification";
import {
  useBadgesQuery,
  useClaimDailyRewardMutation,
  useDailyRewardStatusQuery,
  useGamificationSummaryQuery,
  useMissionsQuery,
  useSpinMutation,
  useSpinStatusQuery,
} from "../../../src/hooks/useGamification";

function SectionHeader({ title, trailing }: { title: string; trailing?: string }) {
  return (
    <View className="mb-3 flex-row items-center justify-between">
      <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{title}</Text>
      {trailing ? (
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{trailing}</Text>
      ) : null}
    </View>
  );
}

function SummaryStripSkeleton() {
  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        {[0, 1, 2].map((i) => (
          <View key={i} className="flex-1 items-center">
            <Skeleton variant="circle" width={26} height={26} className="mb-2" />
            <Skeleton variant="text" width={40} />
          </View>
        ))}
      </View>
    </Card>
  );
}

/** Level/xp/coins/streak strip mirroring `DashboardContent`'s summary card, plus a quick badges/missions teaser row this hub screen adds on top. */
function SummaryStrip() {
  const { colors } = useTheme();
  const summaryQuery = useGamificationSummaryQuery();

  if (summaryQuery.isLoading) return <SummaryStripSkeleton />;
  if (summaryQuery.isError || !summaryQuery.data) return null;

  const summary = summaryQuery.data;

  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        <View className="flex-1 items-center">
          <Icon name="flame" size={26} color={colors.warning} />
          <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
            {summary.currentStreakDays}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">day streak</Text>
        </View>
        <View className="flex-1 items-center">
          <Icon name="star" size={26} color={colors.brand} />
          <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Lvl {summary.level}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{summary.xp} XP</Text>
        </View>
        <View className="flex-1 items-center">
          <Icon name="cash-outline" size={26} color={colors.accent} />
          <Text className="mt-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
            {summary.coins}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">coins</Text>
        </View>
      </View>
      <Divider className="my-4" />
      <View className="flex-row items-center justify-center gap-2">
        <Badge label={`${summary.badgesEarnedCount}/${summary.totalBadgesCount} badges`} variant="brand" />
        <Badge
          label={`${summary.missionsCompletedThisWeek}/${summary.totalMissionsThisWeek} missions this week`}
          variant="neutral"
        />
      </View>
    </Card>
  );
}

function BadgesSectionSkeleton() {
  return (
    <View className="flex-row flex-wrap">
      {[0, 1, 2, 3].map((i) => (
        <View key={i} className="w-1/2 p-1.5">
          <Card>
            <View className="items-center py-2">
              <Skeleton variant="circle" width={56} height={56} className="mb-2" />
              <Skeleton variant="text" width="70%" className="mb-1" />
              <Skeleton variant="text" width="50%" />
            </View>
          </Card>
        </View>
      ))}
    </View>
  );
}

function BadgesSection() {
  const badgesQuery = useBadgesQuery();
  const badges = badgesQuery.data ?? [];
  const earnedCount = badges.filter((b) => b.isEarned).length;

  return (
    <View className="mb-6">
      <SectionHeader
        title="Badges"
        trailing={badgesQuery.data ? `${earnedCount}/${badges.length} earned` : undefined}
      />
      {badgesQuery.isLoading ? (
        <BadgesSectionSkeleton />
      ) : badgesQuery.isError ? (
        <ErrorState
          title="Couldn't load your badges"
          description="Check your connection and try again."
          onRetry={() => void badgesQuery.refetch()}
        />
      ) : badges.length === 0 ? (
        <Card>
          <EmptyState icon="ribbon-outline" title="No badges yet" description="Keep studying to start earning badges." />
        </Card>
      ) : (
        <View className="-m-1.5 flex-row flex-wrap">
          {badges.map((badge) => (
            <BadgeTile key={badge.id} badge={badge} />
          ))}
        </View>
      )}
    </View>
  );
}

function MissionsSection() {
  const missionsQuery = useMissionsQuery();
  const missions = missionsQuery.data ?? [];

  return (
    <View className="mb-6">
      <SectionHeader title="Missions" />
      {missionsQuery.isLoading ? (
        <Card>
          {[0, 1, 2].map((i) => (
            <View key={i} className="px-3 py-3">
              <Skeleton variant="text" width="60%" className="mb-2" />
              <Skeleton variant="rect" width="100%" height={8} className="mb-2" />
              <Skeleton variant="text" width="35%" />
            </View>
          ))}
        </Card>
      ) : missionsQuery.isError ? (
        <ErrorState
          title="Couldn't load your missions"
          description="Check your connection and try again."
          onRetry={() => void missionsQuery.refetch()}
        />
      ) : missions.length === 0 ? (
        <Card>
          <EmptyState icon="flag-outline" title="No active missions" description="New missions will show up here soon." />
        </Card>
      ) : (
        <Card>
          {missions.map((mission, index) => (
            <React.Fragment key={mission.id}>
              {index > 0 ? <Divider /> : null}
              <MissionRow mission={mission} />
            </React.Fragment>
          ))}
        </Card>
      )}
    </View>
  );
}

/**
 * Gamification hub: summary strip, badges grid, missions list, a "Claim
 * daily reward" card, and a "Spin the wheel" card. Reached from the
 * "Rewards" card on the dashboard. Every section loads/errors/empties
 * independently (mirroring `InterviewPrepScreen`'s per-section handling) so
 * one slow/failing endpoint doesn't block the rest of the hub from
 * rendering.
 */
export default function GamificationScreen() {
  const { show } = useToast();

  const dailyRewardStatusQuery = useDailyRewardStatusQuery();
  const spinStatusQuery = useSpinStatusQuery();
  const claimMutation = useClaimDailyRewardMutation();
  const spinMutation = useSpinMutation();

  const [claimResult, setClaimResult] = useState<ClaimDailyRewardResponse | null>(null);

  const [spinPhase, setSpinPhase] = useState<SpinPhase>("idle");
  const [spinResult, setSpinResult] = useState<SpinResponse | null>(null);
  const [spinFailed, setSpinFailed] = useState(false);
  const [landingIndex, setLandingIndex] = useState<number | null>(null);

  function handleClaim() {
    if (claimMutation.isPending || dailyRewardStatusQuery.data?.claimedToday) return;
    claimMutation.mutate(undefined, {
      onSuccess: (result) => {
        setClaimResult(result);
        show(`+${result.xpAwarded} XP, +${result.coinsAwarded} coins!`, "success");
      },
      onError: () => {
        show("Couldn't claim your daily reward. Please try again.", "danger");
      },
    });
  }

  function handleSpin() {
    if (spinMutation.isPending || spinPhase === "spinning" || spinPhase === "landing") return;
    setSpinPhase("spinning");
    setSpinResult(null);
    setSpinFailed(false);
    setLandingIndex(null);
    spinMutation.mutate(undefined, {
      onSuccess: (result) => {
        setSpinResult(result);
        setLandingIndex(Math.floor(Math.random() * SPIN_WHEEL_SEGMENT_COUNT));
        setSpinPhase("landing");
      },
      onError: () => {
        setSpinFailed(true);
        setLandingIndex(Math.floor(Math.random() * SPIN_WHEEL_SEGMENT_COUNT));
        setSpinPhase("landing");
        show("Couldn't spin the wheel. Please try again.", "danger");
      },
    });
  }

  function handleLandingComplete() {
    setSpinPhase("revealed");
  }

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Gamification</Text>

      <SummaryStrip />
      <BadgesSection />
      <MissionsSection />

      <View className="mb-6">
        {dailyRewardStatusQuery.isLoading ? (
          <Card>
            <Skeleton variant="text" width="40%" className="mb-4" />
            <Skeleton variant="rect" width="100%" height={72} className="mb-4" />
            <Skeleton variant="rect" width="100%" height={44} />
          </Card>
        ) : dailyRewardStatusQuery.isError || !dailyRewardStatusQuery.data ? (
          <ErrorState
            title="Couldn't load today's daily reward"
            description="Check your connection and try again."
            onRetry={() => void dailyRewardStatusQuery.refetch()}
          />
        ) : (
          <DailyRewardCard
            status={dailyRewardStatusQuery.data}
            claimResult={claimResult}
            isClaiming={claimMutation.isPending}
            onClaim={handleClaim}
          />
        )}
      </View>

      <View>
        {spinStatusQuery.isLoading ? (
          <Card>
            <Skeleton variant="text" width="40%" className="mb-4" />
            <Skeleton variant="circle" width={240} height={240} className="mb-4 self-center" />
            <Skeleton variant="rect" width="100%" height={44} />
          </Card>
        ) : spinStatusQuery.isError || !spinStatusQuery.data ? (
          <ErrorState
            title="Couldn't load the spin wheel"
            description="Check your connection and try again."
            onRetry={() => void spinStatusQuery.refetch()}
          />
        ) : (
          <SpinWheelCard
            status={spinStatusQuery.data}
            phase={spinPhase}
            spinResult={spinResult}
            spinFailed={spinFailed}
            landingIndex={landingIndex}
            onSpin={handleSpin}
            onLandingComplete={handleLandingComplete}
          />
        )}
      </View>
    </ScreenContainer>
  );
}

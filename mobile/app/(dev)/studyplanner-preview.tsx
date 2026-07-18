import React, { useState } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Chip } from "../../src/components/Chip";
import { Divider } from "../../src/components/Divider";
import { EmptyState } from "../../src/components/EmptyState";
import { Icon } from "../../src/components/Icon";
import { TextField } from "../../src/components/TextField";
import { StudyPlanSummaryCard } from "../../src/components/studyplanner/StudyPlanSummaryCard";
import { StudyTaskRow } from "../../src/components/studyplanner/StudyTaskRow";
import { useTheme } from "../../src/theme/ThemeProvider";
import { formatDayHeading, formatWeekRangeLabel, isToday, shiftYmd, todayYmd } from "../../src/lib/studyPlannerDates";
import type { ActiveStudyPlanDto, StudyTaskDto } from "../../src/api/studyplanner";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/studyplanner-preview`. Mirrors `app/(dev)/mocktests-preview.tsx`'s
// approach: at the time this was written, the real backend had no
// `StudyPlanner` feature/controller committed yet and nothing was listening
// on `localhost:5221` at all, so this feeds hand-written fixtures straight
// into the same shared components the real setup/overview/weekly screens
// use (`StudyPlanSummaryCard`, `StudyTaskRow`) for manual/automated visual
// QA in both light and dark mode. None of the data below comes from — or is
// wired to — the real Study Planner API. Delete this file once the real
// backend is reachable and the Study Planner screens have been re-verified
// against it.
// ---------------------------------------------------------------------------

const MIN_HOURS_PER_DAY = 0.5;
const MAX_HOURS_PER_DAY = 12;
const HOURS_STEP = 0.5;

function formatHours(hours: number): string {
  return Number.isInteger(hours) ? `${hours}` : hours.toFixed(1);
}

function SetupFormFixtureSection() {
  const { colors } = useTheme();
  const [examDate] = useState("2026-09-15");
  const [subjects, setSubjects] = useState(["Biology", "Chemistry", "Mathematics"]);
  const [weakTopics, setWeakTopics] = useState(["Organic reactions", "Trigonometric identities"]);
  const [hoursPerDay, setHoursPerDay] = useState(2.5);
  const [showGenerating, setShowGenerating] = useState(false);

  return (
    <View className="mb-10">
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="flex-1 pr-3 text-heading text-ink-primary dark:text-ink-primary-dark">
          Fixture: plan setup form
        </Text>
        <Button
          title={showGenerating ? "Show form" : "Show generating"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setShowGenerating((prev) => !prev)}
        />
      </View>

      {showGenerating ? (
        <Card>
          <View className="items-center justify-center py-10">
            <ActivityIndicator size="large" color={colors.brand} />
            <Text className="mt-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
              Generating your plan…
            </Text>
            <Text className="mt-2 text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
              Our AI is building a day-by-day schedule around your exam date and weak topics. This can take a few
              seconds.
            </Text>
          </View>
        </Card>
      ) : (
        <>
          <TextField label="Exam date" placeholder="yyyy-mm-dd" value={examDate} editable={false} />

          <View className="mb-4">
            <TextField label="Add a subject" placeholder="e.g. Biology" value="" editable={false} containerClassName="mb-2" />
            <View className="flex-row flex-wrap gap-2">
              {subjects.map((subject) => (
                <Chip key={subject} label={subject} onDismiss={() => setSubjects((prev) => prev.filter((s) => s !== subject))} />
              ))}
            </View>
          </View>

          <View className="mb-4">
            <TextField
              label="Add a weak topic (optional)"
              placeholder="e.g. Quadratic equations"
              value=""
              editable={false}
              containerClassName="mb-2"
            />
            <View className="flex-row flex-wrap gap-2">
              {weakTopics.map((topic) => (
                <Chip key={topic} label={topic} onDismiss={() => setWeakTopics((prev) => prev.filter((t) => t !== topic))} />
              ))}
            </View>
          </View>

          <View className="mb-6">
            <Text className="mb-1.5 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
              Hours per day
            </Text>
            <View className="flex-row items-center justify-between rounded-md border border-border bg-surface px-3.5 py-2.5 dark:border-border-dark dark:bg-surface-dark">
              <Pressable
                onPress={() => setHoursPerDay((h) => Math.max(MIN_HOURS_PER_DAY, Number((h - HOURS_STEP).toFixed(1))))}
                hitSlop={8}
                accessibilityRole="button"
                accessibilityLabel="Decrease hours per day"
              >
                <Icon name="remove-circle-outline" size={28} color={colors.brand} />
              </Pressable>
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {formatHours(hoursPerDay)} {hoursPerDay === 1 ? "hour" : "hours"}
              </Text>
              <Pressable
                onPress={() => setHoursPerDay((h) => Math.min(MAX_HOURS_PER_DAY, Number((h + HOURS_STEP).toFixed(1))))}
                hitSlop={8}
                accessibilityRole="button"
                accessibilityLabel="Increase hours per day"
              >
                <Icon name="add-circle-outline" size={28} color={colors.brand} />
              </Pressable>
            </View>
          </View>

          <Button title="Generate my plan" onPress={() => setShowGenerating(true)} />
        </>
      )}
    </View>
  );
}

const FIXTURE_PLAN: ActiveStudyPlanDto = {
  planId: "plan-1",
  examDate: "2026-09-15",
  daysRemaining: 42,
  subjects: ["Biology", "Chemistry", "Mathematics"],
  weakTopics: ["Organic reactions", "Trigonometric identities"],
  hoursPerDayMinutes: 150,
  totalTasks: 84,
  completedTasks: 30,
  missedTasks: 3,
  progressPercent: 35.7,
};

const FIXTURE_TODAY_TASKS: StudyTaskDto[] = [
  {
    id: "t1",
    subject: "Biology",
    topic: "Cell membrane structure",
    durationMinutes: 45,
    isWeakTopic: false,
    status: "pending",
    scheduledDateUtc: todayYmd(),
    completedAtUtc: null,
  },
  {
    id: "t2",
    subject: "Chemistry",
    topic: "Organic reactions — nucleophilic substitution",
    durationMinutes: 60,
    isWeakTopic: true,
    status: "pending",
    scheduledDateUtc: todayYmd(),
    completedAtUtc: null,
  },
  {
    id: "t3",
    subject: "Mathematics",
    topic: "Trigonometric identities practice set",
    durationMinutes: 30,
    isWeakTopic: true,
    status: "completed",
    scheduledDateUtc: todayYmd(),
    completedAtUtc: `${todayYmd()}T08:35:00Z`,
  },
  {
    id: "t4",
    subject: "Biology",
    topic: "Photosynthesis review",
    durationMinutes: 30,
    isWeakTopic: false,
    status: "missed",
    scheduledDateUtc: shiftYmd(todayYmd(), -1),
    completedAtUtc: null,
  },
];

function PlanOverviewFixtureSection() {
  const [tasks, setTasks] = useState(FIXTURE_TODAY_TASKS);
  const [completingId, setCompletingId] = useState<string | null>(null);

  function handleComplete(taskId: string) {
    setCompletingId(taskId);
    setTimeout(() => {
      setTasks((prev) =>
        prev.map((task) =>
          task.id === taskId
            ? { ...task, status: "completed", completedAtUtc: new Date().toISOString() }
            : task,
        ),
      );
      setCompletingId(null);
    }, 400);
  }

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: plan overview (today)
      </Text>
      <StudyPlanSummaryCard plan={FIXTURE_PLAN} className="mb-5" />
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Today&apos;s tasks</Text>
      <Card>
        {tasks.map((task, index) => (
          <React.Fragment key={task.id}>
            {index > 0 ? <Divider /> : null}
            <StudyTaskRow task={task} onComplete={handleComplete} isCompleting={completingId === task.id} />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

const WEEK_START = todayYmd();

const FIXTURE_WEEK_TASKS: Record<string, StudyTaskDto[]> = {
  [WEEK_START]: [
    { id: "w1", subject: "Biology", topic: "Cell membrane structure", durationMinutes: 45, isWeakTopic: false, status: "pending", scheduledDateUtc: WEEK_START, completedAtUtc: null },
    { id: "w2", subject: "Chemistry", topic: "Organic reactions", durationMinutes: 60, isWeakTopic: true, status: "pending", scheduledDateUtc: WEEK_START, completedAtUtc: null },
  ],
  [shiftYmd(WEEK_START, 1)]: [
    { id: "w3", subject: "Mathematics", topic: "Trigonometric identities", durationMinutes: 40, isWeakTopic: true, status: "rescheduled", scheduledDateUtc: shiftYmd(WEEK_START, 1), completedAtUtc: null },
  ],
  [shiftYmd(WEEK_START, 2)]: [],
  [shiftYmd(WEEK_START, 3)]: [
    { id: "w4", subject: "Biology", topic: "Photosynthesis review", durationMinutes: 30, isWeakTopic: false, status: "missed", scheduledDateUtc: shiftYmd(WEEK_START, 3), completedAtUtc: null },
    { id: "w5", subject: "Chemistry", topic: "Periodic trends", durationMinutes: 45, isWeakTopic: false, status: "completed", scheduledDateUtc: shiftYmd(WEEK_START, 3), completedAtUtc: `${shiftYmd(WEEK_START, 3)}T14:40:00Z` },
  ],
  [shiftYmd(WEEK_START, 4)]: [
    { id: "w6", subject: "Mathematics", topic: "Integration by parts", durationMinutes: 50, isWeakTopic: false, status: "pending", scheduledDateUtc: shiftYmd(WEEK_START, 4), completedAtUtc: null },
  ],
  [shiftYmd(WEEK_START, 5)]: [],
  [shiftYmd(WEEK_START, 6)]: [
    { id: "w7", subject: "Biology", topic: "Genetics — Punnett squares", durationMinutes: 45, isWeakTopic: true, status: "pending", scheduledDateUtc: shiftYmd(WEEK_START, 6), completedAtUtc: null },
  ],
};

function WeeklyViewFixtureSection() {
  const { colors } = useTheme();
  const days = Array.from({ length: 7 }, (_, i) => shiftYmd(WEEK_START, i));

  return (
    <View className="mb-10">
      <Text className="mb-1 text-heading text-ink-primary dark:text-ink-primary-dark">Fixture: weekly view</Text>
      <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        {formatWeekRangeLabel(WEEK_START)}
      </Text>
      <View className="mb-5 flex-row items-center gap-3">
        <View className="flex-1">
          <Button title="Previous" variant="secondary" onPress={() => {}} />
        </View>
        <View className="flex-1">
          <Button title="Next" variant="secondary" onPress={() => {}} />
        </View>
      </View>
      <View className="gap-3">
        {days.map((date) => {
          const dayTasks = FIXTURE_WEEK_TASKS[date] ?? [];
          return (
            <Card key={date}>
              <View className="mb-2 flex-row items-center gap-2">
                {isToday(date) ? <Icon name="today-outline" size={16} color={colors.brand} /> : null}
                <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                  {isToday(date) ? `Today · ${formatDayHeading(date)}` : formatDayHeading(date)}
                </Text>
              </View>
              {dayTasks.length === 0 ? (
                <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Nothing scheduled.</Text>
              ) : (
                dayTasks.map((task, index) => (
                  <React.Fragment key={task.id}>
                    {index > 0 ? <Divider /> : null}
                    <StudyTaskRow task={task} />
                  </React.Fragment>
                ))
              )}
            </Card>
          );
        })}
      </View>
    </View>
  );
}

function EmptyStateFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">Fixture: no active plan</Text>
      <Card>
        <EmptyState
          icon="calendar-outline"
          title="No study plan yet"
          description="Tell us your exam date and subjects, and we'll build an AI day-by-day schedule to get you ready."
          actionLabel="Create a study plan"
          onAction={() => {}}
        />
      </Card>
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Study Planner UI, mirroring the
 * pattern established by `app/(dev)/mocktests-preview.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/studyplanner-preview`.
 */
export default function StudyPlannerPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Study Planner Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <SetupFormFixtureSection />
      <PlanOverviewFixtureSection />
      <WeeklyViewFixtureSection />
      <EmptyStateFixtureSection />
    </ScreenContainer>
  );
}

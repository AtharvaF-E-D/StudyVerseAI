import React, { useState } from "react";
import { ActivityIndicator, Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Chip } from "../../../src/components/Chip";
import { Icon } from "../../../src/components/Icon";
import { TextField } from "../../../src/components/TextField";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { isValidYmd, todayYmd } from "../../../src/lib/studyPlannerDates";
import { useCreateStudyPlanMutation } from "../../../src/hooks/useStudyPlanner";

// ---------------------------------------------------------------------------
// No date-picker pattern exists anywhere else in this app yet (checked: no
// `DateTimePicker`/`date-picker` usage in `src/` or `app/`), so the exam
// date field below is a plain `TextField` that accepts "yyyy-mm-dd" text
// directly, validated client-side by `isValidYmd`. This is a deliberate
// time-boxed simplification — a real native date picker (and the extra
// dependency/platform wiring it needs) would be a reasonable follow-up.
// ---------------------------------------------------------------------------

const MIN_HOURS_PER_DAY = 0.5;
const MAX_HOURS_PER_DAY = 12;
const HOURS_STEP = 0.5;
const DEFAULT_HOURS_PER_DAY = 2;

function formatHours(hours: number): string {
  return Number.isInteger(hours) ? `${hours}` : hours.toFixed(1);
}

interface ChipListFieldProps {
  label: string;
  placeholder: string;
  values: string[];
  onAddValue: (value: string) => void;
  onRemoveValue: (value: string) => void;
  emptyHint: string;
}

/**
 * Repeatable "type a value, tap Add, it becomes a dismissible chip" field —
 * used for both the subjects and weak-topics lists below. Kept local to
 * this screen (rather than promoted to `src/components`) since nothing else
 * in the app needs this exact add/remove-chip-list shape yet.
 */
function ChipListField({ label, placeholder, values, onAddValue, onRemoveValue, emptyHint }: ChipListFieldProps) {
  const [draft, setDraft] = useState("");

  function handleAdd() {
    const trimmed = draft.trim();
    if (!trimmed || values.includes(trimmed)) return;
    onAddValue(trimmed);
    setDraft("");
  }

  return (
    <View className="mb-4">
      <TextField
        label={label}
        placeholder={placeholder}
        value={draft}
        onChangeText={setDraft}
        onSubmitEditing={handleAdd}
        returnKeyType="done"
        containerClassName="mb-2"
      />
      <View className="mb-3 self-start">
        <Button title="Add" variant="secondary" fullWidth={false} disabled={!draft.trim()} onPress={handleAdd} />
      </View>
      {values.length > 0 ? (
        <View className="flex-row flex-wrap gap-2">
          {values.map((value) => (
            <Chip key={value} label={value} onDismiss={() => onRemoveValue(value)} />
          ))}
        </View>
      ) : (
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{emptyHint}</Text>
      )}
    </View>
  );
}

/**
 * Study plan creation form: exam date, subjects, weak topics, and a
 * hours-per-day stepper. Submitting calls the real AI-generation endpoint,
 * which the contract notes can take several seconds — this screen swaps to
 * a full "Generating your plan..." state for the duration rather than a
 * small inline spinner, so it's obvious something substantial is happening.
 */
export default function StudyPlanSetupScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [examDate, setExamDate] = useState("");
  const [subjects, setSubjects] = useState<string[]>([]);
  const [weakTopics, setWeakTopics] = useState<string[]>([]);
  const [hoursPerDay, setHoursPerDay] = useState(DEFAULT_HOURS_PER_DAY);

  const createPlanMutation = useCreateStudyPlanMutation();

  const trimmedExamDate = examDate.trim();
  const examDateValid = isValidYmd(trimmedExamDate) && trimmedExamDate >= todayYmd();
  const canSubmit = examDateValid && subjects.length > 0 && !createPlanMutation.isPending;

  function decreaseHours() {
    setHoursPerDay((prev) => Math.max(MIN_HOURS_PER_DAY, Number((prev - HOURS_STEP).toFixed(1))));
  }

  function increaseHours() {
    setHoursPerDay((prev) => Math.min(MAX_HOURS_PER_DAY, Number((prev + HOURS_STEP).toFixed(1))));
  }

  function handleSubmit() {
    if (!canSubmit) return;
    createPlanMutation.mutate(
      {
        examDate: trimmedExamDate,
        subjects,
        weakTopics,
        hoursPerDayMinutes: Math.round(hoursPerDay * 60),
      },
      {
        onSuccess: (result) => {
          show(`Your study plan is ready — ${result.totalTasks} tasks scheduled.`, "success");
          router.replace("/(app)/studyplanner");
        },
        onError: () => {
          show("Couldn't generate your study plan. Please try again.", "danger");
        },
      },
    );
  }

  if (createPlanMutation.isPending) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-6 py-20">
          <ActivityIndicator size="large" color={colors.brand} />
          <Text className="mt-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Generating your plan…
          </Text>
          <Text className="mt-2 text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            Our AI is building a day-by-day schedule around your exam date and weak topics. This can take a few
            seconds.
          </Text>
        </View>
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer>
      <Text className="mb-2 text-heading text-ink-primary dark:text-ink-primary-dark">Create a study plan</Text>
      <Text className="mb-6 text-body text-ink-secondary dark:text-ink-secondary-dark">
        Tell us about your exam and we&apos;ll generate an AI day-by-day schedule.
      </Text>

      <TextField
        label="Exam date"
        placeholder="yyyy-mm-dd"
        value={examDate}
        onChangeText={setExamDate}
        error={examDate.length > 0 && !examDateValid ? "Enter a valid, upcoming date as yyyy-mm-dd" : undefined}
        helperText={examDate.length === 0 ? "e.g. 2026-09-15" : undefined}
      />

      <ChipListField
        label="Add a subject"
        placeholder="e.g. Biology"
        values={subjects}
        onAddValue={(value) => setSubjects((prev) => [...prev, value])}
        onRemoveValue={(value) => setSubjects((prev) => prev.filter((s) => s !== value))}
        emptyHint="Add at least one subject to include in your plan."
      />

      <ChipListField
        label="Add a weak topic (optional)"
        placeholder="e.g. Quadratic equations"
        values={weakTopics}
        onAddValue={(value) => setWeakTopics((prev) => [...prev, value])}
        onRemoveValue={(value) => setWeakTopics((prev) => prev.filter((t) => t !== value))}
        emptyHint="Weak topics get extra focus in your schedule."
      />

      <View className="mb-6">
        <Text className="mb-1.5 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
          Hours per day
        </Text>
        <View className="flex-row items-center justify-between rounded-md border border-border bg-surface px-3.5 py-2.5 dark:border-border-dark dark:bg-surface-dark">
          <Pressable
            onPress={decreaseHours}
            disabled={hoursPerDay <= MIN_HOURS_PER_DAY}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Decrease hours per day"
          >
            <Icon
              name="remove-circle-outline"
              size={28}
              color={hoursPerDay <= MIN_HOURS_PER_DAY ? colors.border : colors.brand}
            />
          </Pressable>
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {formatHours(hoursPerDay)} {hoursPerDay === 1 ? "hour" : "hours"}
          </Text>
          <Pressable
            onPress={increaseHours}
            disabled={hoursPerDay >= MAX_HOURS_PER_DAY}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Increase hours per day"
          >
            <Icon
              name="add-circle-outline"
              size={28}
              color={hoursPerDay >= MAX_HOURS_PER_DAY ? colors.border : colors.brand}
            />
          </Pressable>
        </View>
      </View>

      <Button title="Generate my plan" onPress={handleSubmit} disabled={!canSubmit} />
    </ScreenContainer>
  );
}

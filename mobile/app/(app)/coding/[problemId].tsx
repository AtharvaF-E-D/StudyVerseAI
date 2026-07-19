import React, { useState } from "react";
import { Pressable, ScrollView, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Chip } from "../../../src/components/Chip";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { CodeEditor, MONOSPACE_FONT_FAMILY } from "../../../src/components/coding/CodeEditor";
import { TestResultsPanel } from "../../../src/components/coding/TestResultsPanel";
import { DIFFICULTY_BADGE_VARIANT, DIFFICULTY_LABELS } from "../../../src/components/coding/difficulty";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { SubmitSolutionResponse } from "../../../src/api/codingpractice";
import {
  useHintMutation,
  useLanguagesQuery,
  useProblemQuery,
  useSubmitSolutionMutation,
} from "../../../src/hooks/useCodingPractice";

/** Python 3.13.2 — matches the phase brief's documented Judge0 id and is the most common "just let me write code" default. */
const DEFAULT_LANGUAGE_ID = 109;

function DetailSkeleton() {
  return (
    <ScreenContainer>
      <Skeleton variant="text" width="70%" className="mb-3" />
      <Skeleton variant="rect" height={24} width={90} className="mb-5 rounded-full" />
      <Skeleton variant="text" width="100%" className="mb-2" />
      <Skeleton variant="text" width="90%" className="mb-2" />
      <Skeleton variant="text" width="80%" className="mb-6" />
      <Skeleton variant="rect" height={280} className="rounded-md" />
    </ScreenContainer>
  );
}

/** One sample test case shown as input -> expected output, monospace. */
function SampleTestCaseCard({ index, input, expectedOutput }: { index: number; input: string; expectedOutput: string }) {
  return (
    <Card className="mb-3">
      <Text className="mb-2 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
        Example {index + 1}
      </Text>
      <Text className="mb-1 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">Input</Text>
      <View className="mb-2 rounded-md border border-border bg-surface px-2.5 py-2 dark:border-border-dark dark:bg-surface-dark">
        <Text style={{ fontFamily: MONOSPACE_FONT_FAMILY }} className="text-caption text-ink-primary dark:text-ink-primary-dark">
          {input}
        </Text>
      </View>
      <Text className="mb-1 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
        Expected output
      </Text>
      <View className="rounded-md border border-border bg-surface px-2.5 py-2 dark:border-border-dark dark:bg-surface-dark">
        <Text style={{ fontFamily: MONOSPACE_FONT_FAMILY }} className="text-caption text-ink-primary dark:text-ink-primary-dark">
          {expectedOutput}
        </Text>
      </View>
    </Card>
  );
}

/**
 * Problem detail + editor screen: description, sample test cases, a
 * language picker that swaps starter code, the (plain-`TextInput`-based —
 * see `CodeEditor.tsx`'s header) code editor, an inline AI hint reveal, and
 * a Submit flow with a real "Running..." state and a color-coded results
 * panel once Judge0 grading comes back.
 */
export default function CodingProblemScreen() {
  const params = useLocalSearchParams<{ problemId: string }>();
  const problemId = params.problemId ?? "";

  const { colors } = useTheme();
  const { show } = useToast();

  const [languageId, setLanguageId] = useState(DEFAULT_LANGUAGE_ID);
  const [sourceCode, setSourceCode] = useState("");
  // Tracks the last starter-code string this screen has already synced
  // `sourceCode` from, so the "reset editor on a new starter code" logic
  // below (a render-time comparison, not an effect — see that comment) can
  // tell "the server sent a genuinely different starter snippet" apart from
  // "the same query just refetched the same text in the background".
  const [syncedStarterCode, setSyncedStarterCode] = useState<string | null>(null);
  const [hint, setHint] = useState<string | null>(null);
  const [result, setResult] = useState<SubmitSolutionResponse | null>(null);

  const languagesQuery = useLanguagesQuery();
  const problemQuery = useProblemQuery(problemId, languageId);
  const submitMutation = useSubmitSolutionMutation(problemId);
  const hintMutation = useHintMutation(problemId);

  // Falls back off the hardcoded default language id to whatever the server
  // actually supports, the first time the real language list loads (a no-op
  // if the default is already a valid id, which is the common case). Calling
  // `setState` directly in the render body (guarded so it converges
  // immediately, before paint) rather than in a `useEffect` — the
  // "adjusting state when a prop changes" pattern React's own docs recommend
  // (same pattern `app/(app)/flashcards/review.tsx` uses for its queue
  // snapshot) — since `languageId` only ever drifts from the real list
  // right after that list loads, this can't loop: once corrected, the
  // condition is false on every subsequent render.
  const availableLanguages = languagesQuery.data;
  if (availableLanguages && availableLanguages.length > 0) {
    const isCurrentLanguageValid = availableLanguages.some((language) => language.languageId === languageId);
    if (!isCurrentLanguageValid) {
      setLanguageId(availableLanguages[0]!.languageId);
    }
  }

  // Resets the editor to the freshly-fetched starter code whenever it
  // actually changes — on first load, and deliberately again on every
  // language switch (matches the phase brief: "swaps starter code when
  // changed"). Same render-time-comparison pattern as above, compared
  // against the starter code STRING (not the whole query object) so an
  // incidental background refetch that returns identical text is a no-op
  // and never clobbers in-progress edits.
  const fetchedStarterCode = problemQuery.data?.starterCode;
  if (fetchedStarterCode !== undefined && fetchedStarterCode !== syncedStarterCode) {
    setSyncedStarterCode(fetchedStarterCode);
    setSourceCode(fetchedStarterCode);
  }

  function handleSubmit() {
    if (submitMutation.isPending) return;
    setResult(null);
    submitMutation.mutate(
      { languageId, sourceCode },
      {
        onSuccess: (response) => setResult(response),
        onError: () => show("Couldn't submit right now. Please try again.", "danger"),
      },
    );
  }

  function handleGetHint() {
    if (hintMutation.isPending) return;
    hintMutation.mutate(
      { currentCode: sourceCode },
      {
        onSuccess: (response) => setHint(response.hint),
        onError: () => show("Couldn't get a hint right now. Please try again.", "danger"),
      },
    );
  }

  if (problemQuery.isLoading) {
    return <DetailSkeleton />;
  }

  if (problemQuery.isError || !problemQuery.data) {
    return (
      <ScreenContainer>
        <ErrorState
          title="Couldn't load this problem"
          description="Check your connection and try again."
          onRetry={() => void problemQuery.refetch()}
        />
      </ScreenContainer>
    );
  }

  const problem = problemQuery.data;
  const languages = languagesQuery.data ?? [];

  return (
    <ScreenContainer>
      <View className="mb-3 flex-row items-start justify-between">
        <Text className="mr-3 flex-1 text-heading text-ink-primary dark:text-ink-primary-dark">{problem.title}</Text>
      </View>

      <View className="mb-5 flex-row flex-wrap items-center gap-2">
        <Badge label={DIFFICULTY_LABELS[problem.difficulty]} variant={DIFFICULTY_BADGE_VARIANT[problem.difficulty]} />
        <Badge label={problem.category} variant="neutral" />
        {problem.isInterviewQuestion ? <Badge label="Interview question" variant="brand" /> : null}
      </View>

      <Text className="mb-6 text-body text-ink-primary dark:text-ink-primary-dark">{problem.description}</Text>

      {problem.sampleTestCases.length > 0 ? (
        <View className="mb-6">
          <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Examples</Text>
          {problem.sampleTestCases.map((testCase, index) => (
            <SampleTestCaseCard key={index} index={index} input={testCase.input} expectedOutput={testCase.expectedOutput} />
          ))}
        </View>
      ) : null}

      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Language</Text>
      {languagesQuery.isLoading ? (
        <View className="mb-5 flex-row gap-2">
          <Skeleton variant="rect" height={32} width={90} className="rounded-full" />
          <Skeleton variant="rect" height={32} width={90} className="rounded-full" />
        </View>
      ) : languagesQuery.isError || languages.length === 0 ? (
        <Text className="mb-5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Couldn&apos;t load the language list — continuing with the default language.
        </Text>
      ) : (
        <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerClassName="mb-5 flex-row gap-2 pr-2">
          {languages.map((language) => (
            <Chip
              key={language.languageId}
              label={language.name}
              selected={language.languageId === languageId}
              onPress={() => setLanguageId(language.languageId)}
            />
          ))}
        </ScrollView>
      )}

      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Your solution</Text>
      <CodeEditor value={sourceCode} onChangeText={setSourceCode} className="mb-4" />

      <View className="mb-4">
        <Button
          title={hintMutation.isPending ? "Getting hint..." : "Get a hint"}
          variant="secondary"
          loading={hintMutation.isPending}
          onPress={handleGetHint}
        />
      </View>

      {hint ? (
        <Card className="mb-4 border-brand dark:border-brand-light">
          <View className="mb-2 flex-row items-center justify-between">
            <View className="flex-row items-center">
              <Icon name="bulb" size={18} color={colors.brand} />
              <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Hint</Text>
            </View>
            <Pressable
              onPress={() => setHint(null)}
              hitSlop={8}
              accessibilityRole="button"
              accessibilityLabel="Dismiss hint"
            >
              <Icon name="close" size={18} color={colors.textSecondary} />
            </Pressable>
          </View>
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{hint}</Text>
        </Card>
      ) : null}

      <View className="mb-6">
        <Button
          title={submitMutation.isPending ? "Running..." : "Submit"}
          loading={submitMutation.isPending}
          onPress={handleSubmit}
        />
      </View>

      {submitMutation.isPending ? (
        <Card className="mb-6 items-center">
          <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
            Running your code against every test case — this can take a few seconds…
          </Text>
        </Card>
      ) : result ? (
        <TestResultsPanel result={result} />
      ) : null}

      <View className="mb-2">
        <Button title="Back to problems" variant="ghost" onPress={() => router.back()} />
      </View>
    </ScreenContainer>
  );
}

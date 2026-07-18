import React, { useEffect, useRef, useState } from "react";
import { Alert, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { ErrorState } from "../../../src/components/ErrorState";
import { QuizOptionButton, type QuizOptionState } from "../../../src/components/quiz/QuizOptionButton";
import { MockTestQuestionNavigator } from "../../../src/components/mocktests/MockTestQuestionNavigator";
import { MockTestTimerBanner } from "../../../src/components/mocktests/MockTestTimerBanner";
import { useToast } from "../../../src/lib/toast";
import { takeStartedMockTestAttempt, type StartedMockTestAttempt } from "../../../src/lib/mockTestAttemptCache";
import type { SubmitMockTestAttemptRequest } from "../../../src/api/mocktests";
import { useSubmitMockTestAttemptMutation } from "../../../src/hooks/useMockTests";

const TICK_MS = 1000;

function toSubmitAnswers(answers: Record<string, number>): SubmitMockTestAttemptRequest["answers"] {
  return Object.entries(answers).map(([questionId, selectedOptionIndex]) => ({ questionId, selectedOptionIndex }));
}

/**
 * Exam-taking screen for one mock test attempt. `POST /mocktests/attempts`
 * (the only endpoint that returns the full question list) is called once,
 * from the templates screen — this screen reads that payload back from
 * `mockTestAttemptCache` rather than re-fetching it (see that file's header
 * for why there's no server-side resume fallback here, unlike the quiz).
 */
export default function MockTestExamScreen() {
  const params = useLocalSearchParams<{ attemptId: string }>();
  const attemptId = params.attemptId ?? "";

  // Lazy `useState` initializer (not a ref) so this one-time cache read
  // happens exactly once and is safe to read during render.
  const [started] = useState(() => takeStartedMockTestAttempt(attemptId));

  if (!started) {
    return (
      <ScreenContainer>
        <ErrorState
          title="This test session couldn't be resumed"
          description="Your progress wasn't saved on this device (e.g. after a page reload). Head back and start a new attempt."
          retryLabel="Back to mock tests"
          onRetry={() => router.replace("/(app)/mocktests")}
        />
      </ScreenContainer>
    );
  }

  return <MockTestExamSession attemptId={attemptId} started={started} />;
}

interface MockTestExamSessionProps {
  attemptId: string;
  started: StartedMockTestAttempt;
}

/**
 * Owns the actual exam loop: a single overall countdown (not per-question),
 * free navigation between questions, and locally-held answers until the
 * final submit. Mounted exactly once per `MockTestExamScreen` render, so
 * `started` only ever needs to seed state lazily.
 */
function MockTestExamSession({ attemptId, started }: MockTestExamSessionProps) {
  const { show } = useToast();
  const submitMutation = useSubmitMockTestAttemptMutation(attemptId);

  const questions = started.questions;
  const totalMs = started.durationMinutes * 60 * 1000;

  // The deadline is computed once from the server's `startedAtUtc` + the
  // template's duration, so the countdown reflects wall-clock time rather
  // than resetting if this component ever re-renders.
  const [deadline] = useState(() => new Date(started.startedAtUtc).getTime() + totalMs);
  const [remainingMs, setRemainingMs] = useState(() => Math.max(0, deadline - Date.now()));
  const [currentIndex, setCurrentIndex] = useState(0);
  const [answers, setAnswers] = useState<Record<string, number>>({});
  // A ref (not state) so setting it doesn't itself trigger the
  // set-state-in-effect that React's rules-of-hooks lint flags — it only
  // needs to gate a one-time side effect, never to drive a re-render.
  const hasAutoSubmittedRef = useRef(false);

  const currentQuestion = questions[currentIndex];
  const answeredCount = Object.keys(answers).length;
  const answeredFlags = questions.map((question) => answers[question.id] !== undefined);

  // --- Countdown ticker: recomputes `remainingMs` from wall-clock time every second. ---
  useEffect(() => {
    const intervalId = setInterval(() => {
      setRemainingMs(Math.max(0, deadline - Date.now()));
    }, TICK_MS);
    return () => clearInterval(intervalId);
  }, [deadline]);

  // --- Time's up: auto-submit whatever's answered so far, exactly once. ---
  useEffect(() => {
    if (remainingMs > 0 || hasAutoSubmittedRef.current || submitMutation.isPending) return;
    hasAutoSubmittedRef.current = true;
    show("Time's up! Submitting your answers...", "warning");
    submitMutation.mutate(
      { answers: toSubmitAnswers(answers) },
      {
        onSuccess: () => router.replace(`/(app)/mocktests/${attemptId}/results`),
        onError: () => show("Couldn't submit your test. Check your connection and try again.", "danger"),
      },
    );
  }, [remainingMs, submitMutation, answers, attemptId, show]);

  function handleSelectOption(optionIndex: number) {
    if (!currentQuestion) return;
    setAnswers((prev) => ({ ...prev, [currentQuestion.id]: optionIndex }));
  }

  function submitNow() {
    if (submitMutation.isPending) return;
    submitMutation.mutate(
      { answers: toSubmitAnswers(answers) },
      {
        onSuccess: () => router.replace(`/(app)/mocktests/${attemptId}/results`),
        onError: () => show("Couldn't submit your test. Check your connection and try again.", "danger"),
      },
    );
  }

  function handleSubmitPress() {
    const unansweredCount = questions.length - answeredCount;
    if (unansweredCount > 0) {
      Alert.alert(
        "Submit test?",
        `You have ${unansweredCount} unanswered question${unansweredCount === 1 ? "" : "s"}. Submit anyway?`,
        [
          { text: "Keep going", style: "cancel" },
          { text: "Submit", style: "destructive", onPress: submitNow },
        ],
      );
      return;
    }
    submitNow();
  }

  function optionState(optionIndex: number): QuizOptionState {
    if (!currentQuestion) return "default";
    return answers[currentQuestion.id] === optionIndex ? "selected" : "default";
  }

  if (!currentQuestion) {
    return (
      <ScreenContainer>
        <ErrorState
          title="This test has no questions"
          retryLabel="Back to mock tests"
          onRetry={() => router.replace("/(app)/mocktests")}
        />
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer>
      <View className="mb-4 flex-row items-start justify-between">
        <Text className="flex-1 pr-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {started.templateTitle}
        </Text>
        <Button
          title="Submit test"
          variant="secondary"
          fullWidth={false}
          loading={submitMutation.isPending}
          onPress={handleSubmitPress}
        />
      </View>

      <MockTestTimerBanner remainingMs={remainingMs} totalMs={totalMs} className="mb-5" />

      <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        Question {currentIndex + 1} of {questions.length} · {answeredCount} answered
      </Text>
      <MockTestQuestionNavigator
        totalQuestions={questions.length}
        currentIndex={currentIndex}
        answeredFlags={answeredFlags}
        onSelect={setCurrentIndex}
        className="mb-5"
      />

      <Card className="mb-5">
        <Text className="mb-4 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {currentQuestion.questionText}
        </Text>
        {currentQuestion.options.map((option, optionIndex) => (
          <QuizOptionButton
            key={optionIndex}
            label={option}
            state={optionState(optionIndex)}
            onPress={submitMutation.isPending ? undefined : () => handleSelectOption(optionIndex)}
          />
        ))}
      </Card>

      <View className="mb-5 flex-row gap-3">
        <View className="flex-1">
          <Button
            title="Previous"
            variant="secondary"
            disabled={currentIndex === 0}
            onPress={() => setCurrentIndex((i) => Math.max(0, i - 1))}
          />
        </View>
        <View className="flex-1">
          <Button
            title="Next"
            variant="secondary"
            disabled={currentIndex === questions.length - 1}
            onPress={() => setCurrentIndex((i) => Math.min(questions.length - 1, i + 1))}
          />
        </View>
      </View>

      <Button title="Submit test" loading={submitMutation.isPending} onPress={handleSubmitPress} />
    </ScreenContainer>
  );
}

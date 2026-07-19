import React, { useState } from "react";
import { ActivityIndicator, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { TextField } from "../../../src/components/TextField";
import { INTERVIEW_CATEGORY_LABELS } from "../../../src/components/interview/category";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { takeStartedInterviewSession } from "../../../src/lib/interviewSessionCache";
import type { InterviewAnswerRecordDto, InterviewCategory, InterviewQuestionDto } from "../../../src/api/interviewprep";
import { useCompleteInterviewSessionMutation, useInterviewSessionQuery, useSubmitInterviewAnswerMutation } from "../../../src/hooks/useInterviewPrep";

interface GradedAnswer {
  answerText: string;
  score: number;
  feedback: string;
}

function buildGradedMap(answers: InterviewAnswerRecordDto[]): Record<string, GradedAnswer> {
  const map: Record<string, GradedAnswer> = {};
  for (const answer of answers) {
    map[answer.questionId] = { answerText: answer.answerText, score: answer.score, feedback: answer.feedback };
  }
  return map;
}

function SessionLoadingSkeleton() {
  return (
    <ScreenContainer>
      <Skeleton variant="rect" height={10} className="mb-6 rounded-full" />
      <Skeleton variant="text" width="40%" className="mb-3" />
      <Skeleton variant="rect" height={90} className="mb-5 rounded-xl" />
      <Skeleton variant="rect" height={140} className="rounded-xl" />
    </ScreenContainer>
  );
}

interface InterviewSessionPlayerProps {
  sessionId: string;
  type: InterviewCategory;
  questions: InterviewQuestionDto[];
  existingAnswers: InterviewAnswerRecordDto[];
  initialOverallScore: number | null;
  initialImprovementPlan: string | null;
}

/**
 * Owns the actual practice-session loop: one question at a time, "Grading…"
 * while the real per-answer AI call is in flight, inline score/feedback
 * reveal, then — on the last question — a "Finalizing…" state while the real
 * whole-session AI call runs, ending in the overall score + improvement
 * plan. Mounted exactly once per session id (see the screen component
 * below), so `questions`/`existingAnswers` only ever need to seed state
 * once — no effect has to sync server data into local state after the fact.
 */
function InterviewSessionPlayer({
  sessionId,
  type,
  questions,
  existingAnswers,
  initialOverallScore,
  initialImprovementPlan,
}: InterviewSessionPlayerProps) {
  const { colors } = useTheme();
  const { show } = useToast();

  const initialGradedAnswers = buildGradedMap(existingAnswers);
  const firstUnansweredIndex = questions.findIndex((question) => !(question.questionId in initialGradedAnswers));
  const initialIndex =
    initialOverallScore !== null || firstUnansweredIndex === -1
      ? Math.max(0, questions.length - 1)
      : firstUnansweredIndex;

  const [gradedAnswers, setGradedAnswers] = useState<Record<string, GradedAnswer>>(initialGradedAnswers);
  const [currentIndex, setCurrentIndex] = useState(initialIndex);
  const [answerText, setAnswerText] = useState("");
  const [isComplete, setIsComplete] = useState(initialOverallScore !== null);
  const [overallScore, setOverallScore] = useState<number | null>(initialOverallScore);
  const [improvementPlan, setImprovementPlan] = useState<string | null>(initialImprovementPlan);

  const submitAnswerMutation = useSubmitInterviewAnswerMutation(sessionId);
  const completeMutation = useCompleteInterviewSessionMutation(sessionId);

  const currentQuestion = questions[currentIndex] as InterviewQuestionDto | undefined;
  const currentGraded = currentQuestion ? gradedAnswers[currentQuestion.questionId] : undefined;
  const isLastQuestion = currentIndex === questions.length - 1;
  const answeredCount = Object.keys(gradedAnswers).length;

  function handleSubmitAnswer() {
    if (!currentQuestion || submitAnswerMutation.isPending) return;
    const trimmed = answerText.trim();
    if (!trimmed) return;

    submitAnswerMutation.mutate(
      { questionId: currentQuestion.questionId, answerText: trimmed },
      {
        // The graded score/feedback is what actually drives the inline
        // reveal below — without this `onSuccess` updating local state, a
        // correct backend response would silently never reach the UI (the
        // exact bug class flagged for this phase).
        onSuccess: (result) => {
          setGradedAnswers((prev) => ({
            ...prev,
            [currentQuestion.questionId]: { answerText: trimmed, score: result.score, feedback: result.feedback },
          }));
        },
        onError: () => show("Couldn't grade that answer. Please try again.", "danger"),
      },
    );
  }

  function handleNextQuestion() {
    if (isLastQuestion) return;
    setCurrentIndex((index) => index + 1);
    setAnswerText("");
  }

  function handleCompleteSession() {
    if (completeMutation.isPending) return;
    completeMutation.mutate(undefined, {
      // Same reasoning as the answer mutation above: the real overall
      // score/plan only ever reaches the screen if `onSuccess` actually
      // writes it into state, not just guards against `onError`.
      onSuccess: (result) => {
        setOverallScore(result.overallScore);
        setImprovementPlan(result.improvementPlan);
        setIsComplete(true);
      },
      onError: () => show("Couldn't finalize your session. Please try again.", "danger"),
    });
  }

  if (completeMutation.isPending) {
    return (
      <ScreenContainer scrollable={false}>
        <View className="flex-1 items-center justify-center px-6 py-20">
          <ActivityIndicator size="large" color={colors.brand} />
          <Text className="mt-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Finalizing your results…
          </Text>
          <Text className="mt-2 text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
            Our AI is reviewing every answer to score this session and build your improvement plan. This can take a
            little while.
          </Text>
        </View>
      </ScreenContainer>
    );
  }

  if (isComplete) {
    return (
      <ScreenContainer>
        <Card className="mb-5 items-center">
          <Icon name="trophy" size={32} color={colors.warning} />
          <Text className="mt-2 text-display text-ink-primary dark:text-ink-primary-dark">
            {overallScore !== null ? Math.round(overallScore) : "--"}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {INTERVIEW_CATEGORY_LABELS[type]} · overall score
          </Text>
        </Card>

        <Card className="mb-6 border-brand dark:border-brand-light">
          <View className="mb-2 flex-row items-center">
            <Icon name="bulb" size={20} color={colors.brand} />
            <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
              Your improvement plan
            </Text>
          </View>
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">
            {improvementPlan ?? "No improvement plan was returned for this session."}
          </Text>
        </Card>

        <Button title="Back to Interview Prep" onPress={() => router.replace("/(app)/interview")} />
      </ScreenContainer>
    );
  }

  if (!currentQuestion) {
    return (
      <ScreenContainer>
        <ErrorState
          title="This session has no questions"
          retryLabel="Back to Interview Prep"
          onRetry={() => router.replace("/(app)/interview")}
        />
      </ScreenContainer>
    );
  }

  const progressPercent = Math.round(((currentIndex + (currentGraded ? 1 : 0)) / questions.length) * 100);

  return (
    <ScreenContainer>
      <Text className="mb-1 text-heading text-ink-primary dark:text-ink-primary-dark">
        {INTERVIEW_CATEGORY_LABELS[type]} practice
      </Text>
      <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        Question {currentIndex + 1} of {questions.length} · {answeredCount} answered
      </Text>

      <View className="mb-5 h-2 w-full overflow-hidden rounded-full bg-border dark:bg-border-dark">
        <View
          className="h-full rounded-full bg-brand dark:bg-brand-light"
          style={{ width: `${progressPercent}%` }}
        />
      </View>

      <Card className="mb-5">
        <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
          {currentQuestion.questionText}
        </Text>
      </Card>

      {currentGraded ? (
        <>
          <Card className="mb-5 border-brand dark:border-brand-light">
            <View className="mb-2 flex-row items-center justify-between">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">Your score</Text>
              <Text className="text-subheading text-brand dark:text-brand-light">
                {Math.round(currentGraded.score)}
              </Text>
            </View>
            <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{currentGraded.feedback}</Text>
          </Card>
          <Button
            title={isLastQuestion ? "Complete session" : "Next question"}
            loading={completeMutation.isPending}
            onPress={isLastQuestion ? handleCompleteSession : handleNextQuestion}
          />
        </>
      ) : (
        <>
          <TextField
            label="Your answer"
            placeholder="Type your answer here…"
            value={answerText}
            onChangeText={setAnswerText}
            multiline
            numberOfLines={6}
            textAlignVertical="top"
            autoCapitalize="sentences"
            editable={!submitAnswerMutation.isPending}
            containerClassName="mb-2"
          />
          {submitAnswerMutation.isPending ? (
            <Card className="mb-4 items-center">
              <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
                Grading your answer — this can take a few seconds…
              </Text>
            </Card>
          ) : null}
          <Button
            title={submitAnswerMutation.isPending ? "Grading…" : "Submit answer"}
            loading={submitAnswerMutation.isPending}
            disabled={answerText.trim().length === 0}
            onPress={handleSubmitAnswer}
          />
        </>
      )}
    </ScreenContainer>
  );
}

/**
 * Practice session screen. `POST /interview/sessions` (the only endpoint
 * that returns the full question list on the fresh-start path) is called
 * once, from the category picker screen — this screen normally reads that
 * payload back from `interviewSessionCache` rather than re-fetching it.
 *
 * If there's no cached payload (a page reload on web, a deep link straight
 * into an existing session id, or opening a past session from the history
 * list), this falls back to `useInterviewSessionQuery` to resume from the
 * server's authoritative state — which, unlike the quiz's resume endpoint,
 * returns the full question list AND every already-graded answer, so
 * `InterviewSessionPlayer` below can pick up exactly where the session left
 * off (or show the completed summary directly) either way.
 */
export default function InterviewSessionScreen() {
  const params = useLocalSearchParams<{ sessionId: string }>();
  const sessionId = params.sessionId ?? "";

  const [started] = useState(() => takeStartedInterviewSession(sessionId));
  const hasLocalQuestions = started !== undefined;

  const resumeQuery = useInterviewSessionQuery(sessionId, { enabled: !hasLocalQuestions });

  if (hasLocalQuestions) {
    return (
      <InterviewSessionPlayer
        sessionId={sessionId}
        type={started.type}
        questions={started.questions}
        existingAnswers={[]}
        initialOverallScore={null}
        initialImprovementPlan={null}
      />
    );
  }

  if (resumeQuery.isLoading) {
    return <SessionLoadingSkeleton />;
  }

  if (resumeQuery.isError || !resumeQuery.data) {
    return (
      <ScreenContainer>
        <ErrorState
          title="Couldn't load this session"
          description="This session may not exist, or your connection may be down."
          retryLabel="Back to Interview Prep"
          onRetry={() => router.replace("/(app)/interview")}
        />
      </ScreenContainer>
    );
  }

  const state = resumeQuery.data;
  return (
    <InterviewSessionPlayer
      sessionId={sessionId}
      type={state.type}
      questions={state.questions}
      existingAnswers={state.answers}
      initialOverallScore={state.overallScore}
      initialImprovementPlan={state.improvementPlan}
    />
  );
}

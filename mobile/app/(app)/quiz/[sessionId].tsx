import React, { useEffect, useRef, useState } from "react";
import { Modal, Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { LivesRow } from "../../../src/components/quiz/LivesRow";
import { ComboBadge } from "../../../src/components/quiz/ComboBadge";
import { QuizTimerBar } from "../../../src/components/quiz/QuizTimerBar";
import { QuizOptionButton, type QuizOptionState } from "../../../src/components/quiz/QuizOptionButton";
import { QuizSessionSummaryCard } from "../../../src/components/quiz/QuizSessionSummaryCard";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { takeStartedQuizSession } from "../../../src/lib/quizSessionCache";
import { getQuizSession, type QuizPowerUpsAvailableDto, type QuizQuestionDto, type QuizSessionSummaryDto } from "../../../src/api/quiz";
import {
  useAbandonSessionMutation,
  useExtraTimeMutation,
  useFiftyFiftyMutation,
  useQuizSessionQuery,
  useSubmitAnswerMutation,
} from "../../../src/hooks/useQuiz";

const MAX_LIVES = 3;
const QUESTION_TIME_LIMIT_MS = 15000;
const EXTRA_TIME_BONUS_MS = 10000;
const TIMER_TICK_MS = 100;
const ANSWER_REVEAL_DELAY_MS = 1600;
/**
 * `selectedOptionIndex` submitted when the timer runs out with nothing
 * picked. There's no "no answer" sentinel here: the backend's
 * `SubmitAnswerCommandValidator` requires `InclusiveBetween(0, 3)`, so a
 * timeout has to submit *some* real option index to keep the session's
 * `CurrentQuestionIndex` advancing in lockstep with the client. Always
 * guessing index 0 means a timeout is only "free" when 0 happens to be
 * correct (no worse odds than any other fixed or random guess, since the
 * client never sees the correct index up front) — this is the closest
 * honest approximation of "treat it as incorrect" the real contract allows.
 */
const TIMEOUT_FALLBACK_OPTION_INDEX = 0;

interface QuizPlayInitialState {
  totalQuestions: number;
  currentIndex: number;
  currentQuestion: QuizQuestionDto;
  /** Full question list, only available on the normal start->play path (see the cache comment below); null when cold-resumed. */
  questions: QuizQuestionDto[] | null;
  livesRemaining: number;
  comboCount: number;
  powerUpsAvailable: QuizPowerUpsAvailableDto;
}

/**
 * Play screen for one quiz session. `POST /quiz/sessions` (the only endpoint
 * that returns the full question list) is called once, from the picker
 * screen — this screen normally reads that payload back from
 * `quizSessionCache` rather than re-fetching it, since the answers/resume
 * endpoints never return more than the current question.
 *
 * If there's no cached payload (a page reload on web, or a deep link
 * straight into an in-progress session id), this falls back to
 * `useQuizSessionQuery` to resume from the server's authoritative state —
 * `QuizPlaySession` below then works identically either way, since gameplay
 * only ever needs "the current question" plus (when available) the rest of
 * the list to advance through without a round trip.
 */
export default function QuizPlayScreen() {
  const params = useLocalSearchParams<{ sessionId: string }>();
  const sessionId = params.sessionId ?? "";

  // Lazy `useState` initializer (not a ref) so this one-time cache read
  // happens exactly once and is safe to read during render.
  const [startedSession] = useState(() => takeStartedQuizSession(sessionId));
  const hasLocalQuestions = startedSession !== undefined;

  const resumeQuery = useQuizSessionQuery(sessionId, { enabled: !hasLocalQuestions });

  if (hasLocalQuestions) {
    const started = startedSession;
    const initial: QuizPlayInitialState = {
      totalQuestions: started.totalQuestions,
      currentIndex: 0,
      currentQuestion: started.questions[0],
      questions: started.questions,
      livesRemaining: started.livesRemaining,
      comboCount: 0,
      powerUpsAvailable: started.powerUpsAvailable,
    };
    return <QuizPlaySession sessionId={sessionId} initial={initial} />;
  }

  if (resumeQuery.isLoading) {
    return (
      <ScreenContainer>
        <Skeleton variant="rect" height={10} className="mb-6 rounded-full" />
        <Skeleton variant="text" width="80%" className="mb-3" />
        <Skeleton variant="text" width="60%" className="mb-6" />
        {[0, 1, 2, 3].map((i) => (
          <Skeleton key={i} variant="rect" height={56} className="mb-3 rounded-xl" />
        ))}
      </ScreenContainer>
    );
  }

  const state = resumeQuery.data;

  if (resumeQuery.isError || !state || !state.currentQuestion) {
    return (
      <ScreenContainer>
        <ErrorState
          title="Couldn't resume this quiz"
          description="This session may have already ended. Head back and start a new one."
          retryLabel="Back to quizzes"
          onRetry={() => router.replace("/(app)/quiz")}
        />
      </ScreenContainer>
    );
  }

  const initial: QuizPlayInitialState = {
    totalQuestions: state.totalQuestions,
    currentIndex: state.currentQuestionIndex,
    currentQuestion: state.currentQuestion,
    questions: null,
    livesRemaining: state.livesRemaining,
    comboCount: state.comboCount,
    powerUpsAvailable: state.powerUpsAvailable,
  };
  return <QuizPlaySession sessionId={sessionId} initial={initial} />;
}

interface QuizPlaySessionProps {
  sessionId: string;
  initial: QuizPlayInitialState;
}

/**
 * Owns the actual game loop: timer, lives, combo, power-ups, answer
 * reveal/advance. Mounted exactly once per `QuizPlayScreen` render branch
 * above, so `initial` only ever needs to seed state lazily — no effect has
 * to sync server data into local state after the fact.
 */
function QuizPlaySession({ sessionId, initial }: QuizPlaySessionProps) {
  const { colors } = useTheme();
  const { show } = useToast();

  const questionsRef = useRef(initial.questions);

  const [totalQuestions] = useState(initial.totalQuestions);
  const [currentIndex, setCurrentIndex] = useState(initial.currentIndex);
  const [currentQuestion, setCurrentQuestion] = useState<QuizQuestionDto | null>(initial.currentQuestion);
  const [livesRemaining, setLivesRemaining] = useState(initial.livesRemaining);
  const [comboCount, setComboCount] = useState(initial.comboCount);
  const [fiftyFiftyAvailable, setFiftyFiftyAvailable] = useState(initial.powerUpsAvailable.fiftyFifty);
  const [extraTimeAvailable, setExtraTimeAvailable] = useState(initial.powerUpsAvailable.extraTime);

  const [timeLimitMs, setTimeLimitMs] = useState(QUESTION_TIME_LIMIT_MS);
  const [remainingMs, setRemainingMs] = useState(QUESTION_TIME_LIMIT_MS);
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const [eliminatedIndexes, setEliminatedIndexes] = useState<number[]>([]);
  const [revealed, setRevealed] = useState(false);
  const [correctIndex, setCorrectIndex] = useState<number | null>(null);
  const [explanation, setExplanation] = useState<string | null>(null);
  const [isPaused, setIsPaused] = useState(false);
  const [isComplete, setIsComplete] = useState(false);
  const [summary, setSummary] = useState<QuizSessionSummaryDto | null>(null);

  const submitAnswerMutation = useSubmitAnswerMutation(sessionId);
  const fiftyFiftyMutation = useFiftyFiftyMutation(sessionId);
  const extraTimeMutation = useExtraTimeMutation(sessionId);
  const abandonMutation = useAbandonSessionMutation(sessionId);

  // --- Countdown ticker: decrements `remainingMs` while the question is live. ---
  useEffect(() => {
    if (isPaused || revealed || isComplete || !currentQuestion) return;
    const intervalId = setInterval(() => {
      setRemainingMs((prev) => (prev > TIMER_TICK_MS ? prev - TIMER_TICK_MS : 0));
    }, TIMER_TICK_MS);
    return () => clearInterval(intervalId);
  }, [isPaused, revealed, isComplete, currentQuestion]);

  // --- Time's up: auto-submit a "no answer" once the countdown hits zero. ---
  useEffect(() => {
    if (remainingMs > 0 || revealed || isPaused || isComplete || !currentQuestion) return;
    if (submitAnswerMutation.isPending) return;
    submitAnswerMutation.mutate(
      { questionId: currentQuestion.id, selectedOptionIndex: TIMEOUT_FALLBACK_OPTION_INDEX, timeTakenMs: timeLimitMs },
      {
        onSuccess: (result) => {
          setCorrectIndex(result.correctOptionIndex);
          setExplanation(result.explanation);
          setRevealed(true);
          setLivesRemaining(result.livesRemaining);
          setComboCount(result.comboCount);
          if (result.isSessionComplete) {
            setIsComplete(true);
            setSummary(result.sessionSummary ?? null);
          }
        },
        onError: () => show("Couldn't submit in time — check your connection.", "danger"),
      },
    );
  }, [remainingMs, revealed, isPaused, isComplete, currentQuestion, timeLimitMs, submitAnswerMutation, show]);

  // --- After the reveal delay, advance to the next question (or leave the "complete" state showing). ---
  useEffect(() => {
    if (!revealed) return;
    const timeout = setTimeout(() => {
      if (isComplete) return;
      const nextIndex = currentIndex + 1;
      const localQuestions = questionsRef.current;

      function resetForNextQuestion(question: QuizQuestionDto, index: number) {
        setCurrentQuestion(question);
        setCurrentIndex(index);
        setSelectedIndex(null);
        setEliminatedIndexes([]);
        setRevealed(false);
        setCorrectIndex(null);
        setExplanation(null);
        setTimeLimitMs(QUESTION_TIME_LIMIT_MS);
        setRemainingMs(QUESTION_TIME_LIMIT_MS);
      }

      if (localQuestions && nextIndex < localQuestions.length) {
        resetForNextQuestion(localQuestions[nextIndex], nextIndex);
      } else if (!localQuestions) {
        // Cold-resume path with no cached question list: ask the server for
        // the next current question directly (the index already advanced
        // server-side as a result of the answer we just submitted).
        void getQuizSession(sessionId).then((state) => {
          if (state.currentQuestion) {
            resetForNextQuestion(state.currentQuestion, state.currentQuestionIndex);
          }
        });
      }
    }, ANSWER_REVEAL_DELAY_MS);
    return () => clearTimeout(timeout);
  }, [revealed, isComplete, currentIndex, sessionId]);

  function handleSelectOption(optionIndex: number) {
    if (revealed || isPaused || isComplete || !currentQuestion || submitAnswerMutation.isPending) return;
    setSelectedIndex(optionIndex);
    const timeTakenMs = Math.max(0, timeLimitMs - remainingMs);
    submitAnswerMutation.mutate(
      { questionId: currentQuestion.id, selectedOptionIndex: optionIndex, timeTakenMs },
      {
        onSuccess: (result) => {
          setCorrectIndex(result.correctOptionIndex);
          setExplanation(result.explanation);
          setRevealed(true);
          setLivesRemaining(result.livesRemaining);
          setComboCount(result.comboCount);
          if (result.isSessionComplete) {
            setIsComplete(true);
            setSummary(result.sessionSummary ?? null);
          }
        },
        onError: () => {
          setSelectedIndex(null);
          show("Couldn't submit that answer. Please try again.", "danger");
        },
      },
    );
  }

  function handleFiftyFifty() {
    if (!fiftyFiftyAvailable || revealed || isPaused || fiftyFiftyMutation.isPending) return;
    fiftyFiftyMutation.mutate(undefined, {
      onSuccess: (result) => {
        setEliminatedIndexes(result.hiddenOptionIndexes);
        setFiftyFiftyAvailable(false);
      },
      onError: () => show("Couldn't use 50-50 right now.", "danger"),
    });
  }

  function handleExtraTime() {
    if (!extraTimeAvailable || revealed || isPaused || extraTimeMutation.isPending) return;
    extraTimeMutation.mutate(undefined, {
      onSuccess: () => {
        setTimeLimitMs((prev) => prev + EXTRA_TIME_BONUS_MS);
        setRemainingMs((prev) => prev + EXTRA_TIME_BONUS_MS);
        setExtraTimeAvailable(false);
      },
      onError: () => show("Couldn't use extra time right now.", "danger"),
    });
  }

  function handlePause() {
    if (revealed || isComplete) return;
    setIsPaused(true);
  }

  function handleResume() {
    setIsPaused(false);
  }

  function handleQuit() {
    abandonMutation.mutate(undefined, {
      onSuccess: () => router.replace("/(app)/quiz"),
      onError: () => {
        show("Couldn't reach the server, but you're out.", "warning");
        router.replace("/(app)/quiz");
      },
    });
  }

  function optionState(optionIndex: number): QuizOptionState {
    if (eliminatedIndexes.includes(optionIndex)) return "eliminated";
    if (revealed) {
      if (optionIndex === correctIndex) return "correct";
      if (optionIndex === selectedIndex) return "incorrect";
      return "dimmed";
    }
    return optionIndex === selectedIndex ? "selected" : "default";
  }

  if (isComplete) {
    return (
      <ScreenContainer>
        {summary ? (
          <QuizSessionSummaryCard summary={summary} className="mb-6" />
        ) : (
          <Card className="mb-6">
            <Text className="text-center text-heading text-ink-primary dark:text-ink-primary-dark">
              Session complete!
            </Text>
          </Card>
        )}
        <View className="mb-3">
          <Button title="Review answers" onPress={() => router.push(`/(app)/quiz/${sessionId}/review`)} />
        </View>
        <Button title="Done" variant="secondary" onPress={() => router.replace("/(app)/quiz")} />
      </ScreenContainer>
    );
  }

  if (!currentQuestion) {
    return (
      <ScreenContainer>
        <ErrorState
          title="This question is unavailable"
          description="Head back and start a new quiz."
          retryLabel="Back to quizzes"
          onRetry={() => router.replace("/(app)/quiz")}
        />
      </ScreenContainer>
    );
  }

  return (
    <ScreenContainer scrollable={false}>
      <View className="flex-1">
        {/* Header: pause + question progress + lives */}
        <View className="mb-4 flex-row items-center justify-between">
          <Pressable
            onPress={handlePause}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Pause"
            className="h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon name="pause" size={20} color={colors.textPrimary} />
          </Pressable>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Question {currentIndex + 1} / {totalQuestions}
          </Text>
          <LivesRow lives={livesRemaining} maxLives={MAX_LIVES} />
        </View>

        {comboCount >= 2 ? <ComboBadge combo={comboCount} className="mb-3" /> : null}

        <QuizTimerBar remainingMs={remainingMs} totalMs={timeLimitMs} className="mb-5" />

        <Text className="mb-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {currentQuestion.questionText}
        </Text>

        {currentQuestion.options.map((option, optionIndex) => (
          <QuizOptionButton
            key={optionIndex}
            label={option}
            state={optionState(optionIndex)}
            onPress={
              revealed || eliminatedIndexes.includes(optionIndex)
                ? undefined
                : () => handleSelectOption(optionIndex)
            }
          />
        ))}

        {revealed && explanation ? (
          <Card className="mb-4">
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{explanation}</Text>
          </Card>
        ) : null}

        <View className="mt-auto flex-row gap-3">
          <View className="flex-1">
            <Button
              title="50-50"
              variant="secondary"
              disabled={!fiftyFiftyAvailable || revealed}
              loading={fiftyFiftyMutation.isPending}
              onPress={handleFiftyFifty}
            />
          </View>
          <View className="flex-1">
            <Button
              title="+10s"
              variant="secondary"
              disabled={!extraTimeAvailable || revealed}
              loading={extraTimeMutation.isPending}
              onPress={handleExtraTime}
            />
          </View>
        </View>
      </View>

      <Modal visible={isPaused} transparent animationType="fade" onRequestClose={handleResume}>
        <View
          style={{ backgroundColor: colors.overlay }}
          className="flex-1 items-center justify-center px-8"
        >
          <Card elevation="raised" className="w-full max-w-sm items-center">
            <Icon name="pause-circle-outline" size={40} color={colors.textSecondary} />
            <Text className="mb-1 mt-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
              Paused
            </Text>
            <Text className="mb-5 text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Your timer is stopped. Resume when you&apos;re ready, or quit to leave this quiz.
            </Text>
            <View className="mb-3 w-full">
              <Button title="Resume" onPress={handleResume} />
            </View>
            <Button
              title="Quit quiz"
              variant="danger"
              loading={abandonMutation.isPending}
              onPress={handleQuit}
            />
          </Card>
        </View>
      </Modal>
    </ScreenContainer>
  );
}

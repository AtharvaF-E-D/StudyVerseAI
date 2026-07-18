import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Badge } from "../../src/components/Badge";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { Icon } from "../../src/components/Icon";
import { ListItem } from "../../src/components/ListItem";
import { QuizOptionButton, type QuizOptionState } from "../../src/components/quiz/QuizOptionButton";
import { QuizReviewItem } from "../../src/components/quiz/QuizReviewItem";
import { MockTestQuestionNavigator } from "../../src/components/mocktests/MockTestQuestionNavigator";
import { MockTestResultSummaryCard } from "../../src/components/mocktests/MockTestResultSummaryCard";
import { MockTestTimerBanner } from "../../src/components/mocktests/MockTestTimerBanner";
import { useTheme } from "../../src/theme/ThemeProvider";
import { formatRelativeTime } from "../../src/lib/relativeTime";
import { formatPercentileRank } from "../../src/lib/percentile";
import type { MockTestQuestionDto, MockTestTemplateDto } from "../../src/api/mocktests";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/mocktests-preview`. Mirrors `app/(dev)/quiz-preview.tsx`'s approach:
// the real backend has no `/api/v1/mocktests/*` controller reachable yet (it
// was being built in parallel with this mobile work), so this feeds
// hand-written fixtures straight into the same shared components the real
// templates/exam/results/review screens use (`MockTestTimerBanner`,
// `MockTestQuestionNavigator`, `MockTestResultSummaryCard`, `QuizReviewItem`)
// for manual/automated visual QA in both light and dark mode. None of the
// data below comes from — or is wired to — the real mock tests API. Delete
// this file once the real backend is reachable and the mock tests screens
// have been re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_TEMPLATES: MockTestTemplateDto[] = [
  {
    id: "t1",
    title: "Algebra Fundamentals Midterm",
    description: "Covers linear equations, inequalities, and graphing.",
    category: "Mathematics",
    questionCount: 20,
    durationMinutes: 30,
  },
  {
    id: "t2",
    title: "Cell Biology Full Practice Exam",
    description: "A comprehensive review of cell structure and function.",
    category: "Biology",
    questionCount: 40,
    durationMinutes: 60,
  },
];

const FIXTURE_ATTEMPTS = [
  { attemptId: "a1", templateTitle: "Algebra Fundamentals Midterm", score: 82, percentileRank: 72, submittedAtUtc: "2026-07-17T14:00:00Z" },
  { attemptId: "a2", templateTitle: "Cell Biology Full Practice Exam", score: 91, percentileRank: 88, submittedAtUtc: "2026-07-15T09:30:00Z" },
];

function TemplatesFixtureSection() {
  const { colors } = useTheme();
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: templates &amp; history
      </Text>
      <View className="mb-4 gap-3">
        {FIXTURE_TEMPLATES.map((template) => (
          <Card key={template.id}>
            <View className="mb-2 flex-row items-start justify-between">
              <Text className="mr-3 flex-1 text-subheading text-ink-primary dark:text-ink-primary-dark">
                {template.title}
              </Text>
              <Badge label={template.category} variant="brand" />
            </View>
            <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {template.description}
            </Text>
            <View className="mb-3 flex-row items-center gap-4">
              <View className="flex-row items-center">
                <Icon name="list-outline" size={16} color={colors.textSecondary} />
                <Text className="ml-1.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                  {template.questionCount} questions
                </Text>
              </View>
              <View className="flex-row items-center">
                <Icon name="time-outline" size={16} color={colors.textSecondary} />
                <Text className="ml-1.5 text-caption text-ink-secondary dark:text-ink-secondary-dark">
                  {template.durationMinutes} min
                </Text>
              </View>
            </View>
            <Button title="Start test" onPress={() => {}} />
          </Card>
        ))}
      </View>
      <Card>
        {FIXTURE_ATTEMPTS.map((attempt, index) => (
          <React.Fragment key={attempt.attemptId}>
            {index > 0 ? <Divider /> : null}
            <ListItem
              leading={<Icon name="document-text-outline" size={22} color={colors.brand} />}
              title={attempt.templateTitle}
              subtitle={`${attempt.score} pts · ${formatPercentileRank(attempt.percentileRank)} · ${formatRelativeTime(attempt.submittedAtUtc)}`}
              trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
              onPress={() => {}}
            />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

const FIXTURE_EXAM_QUESTIONS: MockTestQuestionDto[] = [
  { id: "q1", questionText: "What is the derivative of x²?", options: ["x", "2x", "x²", "2"] },
  { id: "q2", questionText: "Solve for x: 2x + 4 = 10", options: ["2", "3", "4", "5"] },
  { id: "q3", questionText: "Which planet is known as the Red Planet?", options: ["Venus", "Mars", "Jupiter", "Saturn"] },
  { id: "q4", questionText: "What is the chemical symbol for gold?", options: ["Ag", "Au", "Gd", "Go"] },
  { id: "q5", questionText: "How many continents are there on Earth?", options: ["5", "6", "7", "8"] },
];
const FIXTURE_TOTAL_MS = 30 * 60 * 1000;

function MidExamFixtureSection() {
  const [currentIndex, setCurrentIndex] = useState(2);
  const [answers, setAnswers] = useState<Record<string, number>>({ q1: 1, q2: 3, q4: 1 });
  const [remainingMs, setRemainingMs] = useState(6 * 60 * 1000 + 42 * 1000);

  const currentQuestion = FIXTURE_EXAM_QUESTIONS[currentIndex];
  const answeredFlags = FIXTURE_EXAM_QUESTIONS.map((q) => answers[q.id] !== undefined);

  function optionState(optionIndex: number): QuizOptionState {
    return answers[currentQuestion.id] === optionIndex ? "selected" : "default";
  }

  return (
    <View className="mb-10">
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="flex-1 pr-3 text-heading text-ink-primary dark:text-ink-primary-dark">
          Fixture: mid-exam (mixed answered/unanswered)
        </Text>
        <Button
          title="Low time"
          variant="secondary"
          fullWidth={false}
          onPress={() => setRemainingMs((prev) => (prev > 5 * 60 * 1000 ? 90 * 1000 : 6 * 60 * 1000 + 42 * 1000))}
        />
      </View>
      <View className="mb-4 flex-row items-start justify-between">
        <Text className="flex-1 pr-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
          Algebra Fundamentals Midterm
        </Text>
        <Button title="Submit test" variant="secondary" fullWidth={false} onPress={() => {}} />
      </View>
      <MockTestTimerBanner remainingMs={remainingMs} totalMs={FIXTURE_TOTAL_MS} className="mb-5" />
      <Text className="mb-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        Question {currentIndex + 1} of {FIXTURE_EXAM_QUESTIONS.length} · {Object.keys(answers).length} answered
      </Text>
      <MockTestQuestionNavigator
        totalQuestions={FIXTURE_EXAM_QUESTIONS.length}
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
            onPress={() => setAnswers((prev) => ({ ...prev, [currentQuestion.id]: optionIndex }))}
          />
        ))}
      </Card>
      <View className="flex-row gap-3">
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
            disabled={currentIndex === FIXTURE_EXAM_QUESTIONS.length - 1}
            onPress={() => setCurrentIndex((i) => Math.min(FIXTURE_EXAM_QUESTIONS.length - 1, i + 1))}
          />
        </View>
      </View>
    </View>
  );
}

const FIXTURE_RESULT = {
  score: 82,
  correctCount: 16,
  totalQuestions: 20,
  percentileRank: 72,
};
const FIXTURE_AI_ANALYSIS =
  "You did well on linear equations and graphing, but missed most questions involving quadratic inequalities and word problems that require setting up a system of equations. Review factoring techniques and practice translating word problems into algebraic expressions before your next attempt.";

function ResultsFixtureSection() {
  const { colors } = useTheme();
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">Fixture: results</Text>
      <MockTestResultSummaryCard result={FIXTURE_RESULT} className="mb-5" />
      <Card className="mb-5">
        <View className="mb-2 flex-row items-center">
          <Icon name="bulb-outline" size={20} color={colors.brand} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Where to focus next
          </Text>
        </View>
        <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">{FIXTURE_AI_ANALYSIS}</Text>
      </Card>
      <View className="mb-3">
        <Button title="Review answers" onPress={() => {}} />
      </View>
      <Button title="Back to mock tests" variant="secondary" onPress={() => {}} />
    </View>
  );
}

interface FixtureReviewItem {
  questionId: string;
  questionText: string;
  options: string[];
  selectedOptionIndex: number | null;
  correctOptionIndex: number;
  explanation: string;
}

const FIXTURE_REVIEW_ITEMS: FixtureReviewItem[] = [
  {
    questionId: "q1",
    questionText: "What is the derivative of x²?",
    options: ["x", "2x", "x²", "2"],
    selectedOptionIndex: 1,
    correctOptionIndex: 1,
    explanation: "The power rule gives d/dx[xⁿ] = n·xⁿ⁻¹, so d/dx[x²] = 2x.",
  },
  {
    questionId: "q2",
    questionText: "Solve for x: 2x + 4 = 10",
    options: ["2", "3", "4", "5"],
    selectedOptionIndex: 3,
    correctOptionIndex: 2,
    explanation: "2x + 4 = 10 → 2x = 6 → x = 3.",
  },
  {
    questionId: "q3",
    questionText: "Which planet is known as the Red Planet?",
    options: ["Venus", "Mars", "Jupiter", "Saturn"],
    selectedOptionIndex: null,
    correctOptionIndex: 1,
    explanation: "Mars appears red due to iron oxide (rust) covering its surface.",
  },
];

function ReviewFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: review screen (mix of correct / incorrect / unanswered)
      </Text>
      {FIXTURE_REVIEW_ITEMS.map((item, index) => (
        <QuizReviewItem
          key={item.questionId}
          index={index}
          questionText={item.questionText}
          options={item.options}
          selectedOptionIndex={item.selectedOptionIndex}
          correctOptionIndex={item.correctOptionIndex}
          explanation={item.explanation}
          unansweredLabel="Unanswered"
          className="mb-4"
        />
      ))}
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Mock Tests UI, mirroring the
 * pattern established by `app/(dev)/quiz-preview.tsx`. Not linked from any
 * navigation — reached directly at `/(dev)/mocktests-preview`.
 */
export default function MockTestsPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Mock Tests Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <TemplatesFixtureSection />
      <MidExamFixtureSection />
      <ResultsFixtureSection />
      <ReviewFixtureSection />
    </ScreenContainer>
  );
}

import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { LivesRow } from "../../src/components/quiz/LivesRow";
import { ComboBadge } from "../../src/components/quiz/ComboBadge";
import { QuizTimerBar } from "../../src/components/quiz/QuizTimerBar";
import { QuizOptionButton, type QuizOptionState } from "../../src/components/quiz/QuizOptionButton";
import { QuizSessionSummaryCard } from "../../src/components/quiz/QuizSessionSummaryCard";
import { QuizReviewItem } from "../../src/components/quiz/QuizReviewItem";
import { useTheme } from "../../src/theme/ThemeProvider";
import type { QuizSessionSummaryDto } from "../../src/api/quiz";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/quiz-preview`. Mirrors `app/(dev)/tutor-preview.tsx`'s approach: the
// real backend has no `/api/v1/quiz/*` controller reachable yet (it was being
// built in parallel with this mobile work), so this feeds hand-written
// fixtures straight into the same shared components the real picker/play/
// review screens use (`LivesRow`, `ComboBadge`, `QuizTimerBar`,
// `QuizOptionButton`, `QuizSessionSummaryCard`, `QuizReviewItem`) for manual/
// automated visual QA in both light and dark mode. None of the data below
// comes from — or is wired to — the real quiz API. Delete this file once the
// real backend is reachable and the quiz screens have been re-verified
// against it.
// ---------------------------------------------------------------------------

const FIXTURE_QUESTION = {
  questionText: "Which planet is known as the Red Planet?",
  options: ["Venus", "Mars", "Jupiter", "Saturn"],
};
const FIXTURE_CORRECT_INDEX = 1;

function MidQuizFixtureSection() {
  const [revealed, setRevealed] = useState(false);
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);

  function optionState(optionIndex: number): QuizOptionState {
    if (revealed) {
      if (optionIndex === FIXTURE_CORRECT_INDEX) return "correct";
      if (optionIndex === selectedIndex) return "incorrect";
      return "dimmed";
    }
    return optionIndex === selectedIndex ? "selected" : "default";
  }

  function handleToggle() {
    if (revealed) {
      setRevealed(false);
      setSelectedIndex(null);
    } else {
      setSelectedIndex(0); // simulate an incorrect pick, for the reveal-coloring screenshot
      setRevealed(true);
    }
  }

  return (
    <View className="mb-10">
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="flex-1 pr-3 text-heading text-ink-primary dark:text-ink-primary-dark">
          Fixture: mid-quiz (2 lives, 3x combo)
        </Text>
        <Button
          title={revealed ? "Reset" : "Reveal answer"}
          variant="secondary"
          fullWidth={false}
          onPress={handleToggle}
        />
      </View>
      <Card>
        <View className="mb-4 flex-row items-center justify-between">
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Question 4 / 10</Text>
          <LivesRow lives={2} />
        </View>
        <ComboBadge combo={3} className="mb-3" />
        <QuizTimerBar remainingMs={6000} totalMs={15000} className="mb-5" />
        <Text className="mb-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {FIXTURE_QUESTION.questionText}
        </Text>
        {FIXTURE_QUESTION.options.map((option, index) => (
          <QuizOptionButton
            key={option}
            label={option}
            state={optionState(index)}
            onPress={revealed ? undefined : () => setSelectedIndex(index)}
          />
        ))}
        <View className="mt-2 flex-row gap-3">
          <View className="flex-1">
            <Button title="50-50" variant="secondary" onPress={() => {}} />
          </View>
          <View className="flex-1">
            <Button title="+10s" variant="secondary" disabled onPress={() => {}} />
          </View>
        </View>
      </Card>
    </View>
  );
}

const FIXTURE_SUMMARY: QuizSessionSummaryDto = {
  totalQuestions: 10,
  correctAnswers: 8,
  score: 240,
  xpEarned: 240,
  coinsEarned: 40,
  bestCombo: 5,
  completedAllQuestions: true,
  ranOutOfLives: false,
  dailyChallengeBonusXp: 20,
  dailyChallengeBonusCoins: 5,
};

function SessionCompleteFixtureSection() {
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: session complete
      </Text>
      <Card className="mb-4">
        <QuizSessionSummaryCard summary={FIXTURE_SUMMARY} />
      </Card>
      <View className="mb-3">
        <Button title="Review answers" onPress={() => {}} />
      </View>
      <Button title="Done" variant="secondary" onPress={() => {}} />
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
    questionText: "Which planet is known as the Red Planet?",
    options: ["Venus", "Mars", "Jupiter", "Saturn"],
    selectedOptionIndex: 1,
    correctOptionIndex: 1,
    explanation: "Mars appears red due to iron oxide (rust) covering its surface.",
  },
  {
    questionId: "q2",
    questionText: "What is the chemical symbol for gold?",
    options: ["Ag", "Au", "Gd", "Go"],
    selectedOptionIndex: 0,
    correctOptionIndex: 1,
    explanation: 'Gold\'s symbol, Au, comes from its Latin name "aurum".',
  },
  {
    questionId: "q3",
    questionText: "How many continents are there on Earth?",
    options: ["5", "6", "7", "8"],
    selectedOptionIndex: null,
    correctOptionIndex: 2,
    explanation:
      "There are 7 continents: Africa, Antarctica, Asia, Australia, Europe, North America, and South America.",
  },
];

function ReviewFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: review screen (mix of correct / incorrect / timed-out)
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
          className="mb-4"
        />
      ))}
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Rapid Fire Quiz UI, mirroring
 * the pattern established by `app/(dev)/tutor-preview.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/quiz-preview`.
 */
export default function QuizPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Quiz Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <MidQuizFixtureSection />
      <SessionCompleteFixtureSection />
      <ReviewFixtureSection />
    </ScreenContainer>
  );
}

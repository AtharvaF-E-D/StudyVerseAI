import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Badge } from "../../src/components/Badge";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Chip } from "../../src/components/Chip";
import { Divider } from "../../src/components/Divider";
import { Switch } from "../../src/components/Switch";
import { ListItem } from "../../src/components/ListItem";
import { Icon } from "../../src/components/Icon";
import { CodeEditor } from "../../src/components/coding/CodeEditor";
import { ProblemRow } from "../../src/components/coding/ProblemRow";
import { TestResultsPanel } from "../../src/components/coding/TestResultsPanel";
import { useTheme } from "../../src/theme/ThemeProvider";
import type { ProblemSummaryDto, SubmitSolutionResponse } from "../../src/api/codingpractice";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/coding-preview`. Mirrors `app/(dev)/mocktests-preview.tsx`'s
// approach: Coding Practice's real backend is further from reachable than
// any prior phase's — `StudyVerse.Application/Features/CodingPractice/**`,
// its API controller, its Judge0 provider, and its EF configuration/seed
// data all don't exist yet in this checkout (only the Domain entities/enums
// do; see `src/api/codingpractice.ts`'s header for the full audit). So this
// feeds hand-written fixtures straight into the exact shared components the
// real problems-list/editor screens use (`ProblemRow`, `CodeEditor`,
// `TestResultsPanel`) for manual/automated visual QA in both light and dark
// mode. None of the data below comes from — or is wired to — the real
// coding practice API. Delete this file once the real backend is reachable
// and the real screens have been re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_PROBLEMS: ProblemSummaryDto[] = [
  { id: "p1", title: "Two Sum", difficulty: "easy", category: "Arrays", isInterviewQuestion: true, isSolved: true },
  { id: "p2", title: "Valid Parentheses", difficulty: "easy", category: "Strings", isInterviewQuestion: true, isSolved: false },
  { id: "p3", title: "Longest Substring Without Repeats", difficulty: "medium", category: "Strings", isInterviewQuestion: true, isSolved: false },
  { id: "p4", title: "Merge Intervals", difficulty: "medium", category: "Arrays", isInterviewQuestion: false, isSolved: false },
  { id: "p5", title: "Median of Two Sorted Arrays", difficulty: "hard", category: "Arrays", isInterviewQuestion: true, isSolved: false },
  { id: "p6", title: "Binary Tree Level Order Traversal", difficulty: "medium", category: "Trees", isInterviewQuestion: false, isSolved: true },
];

function ProblemsListFixtureSection() {
  const { colors } = useTheme();
  const [interviewOnly, setInterviewOnly] = useState(false);

  const visibleProblems = interviewOnly ? FIXTURE_PROBLEMS.filter((p) => p.isInterviewQuestion) : FIXTURE_PROBLEMS;

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: problems list
      </Text>
      <Card className="mb-4">
        <View className="flex-row items-center justify-between">
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">12</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Solved</Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">4</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Day streak</Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">27</Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Submissions</Text>
          </View>
        </View>
      </Card>
      <Card className="mb-4 border-brand dark:border-brand-light">
        <View className="mb-1 flex-row items-center justify-between">
          <View className="flex-row items-center">
            <Icon name="trophy" size={18} color={colors.warning} />
            <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
              Today&apos;s Daily Challenge
            </Text>
          </View>
          <Badge label="Medium" variant="brand" />
        </View>
        <Text className="mb-3 text-body text-ink-primary dark:text-ink-primary-dark">Merge Intervals</Text>
        <Button title="Solve today's challenge" onPress={() => {}} />
      </Card>
      <View className="mb-4 flex-row flex-wrap gap-2">
        {(["All", "Easy", "Medium", "Hard"] as const).map((label, index) => (
          <Chip key={label} label={label} selected={index === 0} onPress={() => {}} />
        ))}
      </View>
      <Card className="mb-4">
        <ListItem
          leading={<Icon name="briefcase-outline" size={20} color={colors.textSecondary} />}
          title="Interview questions only"
          subtitle="Show only classic technical-interview staples"
          trailing={
            <Switch value={interviewOnly} onValueChange={setInterviewOnly} accessibilityLabel="Toggle interview questions only" />
          }
        />
      </Card>
      <Card>
        {visibleProblems.map((problem, index) => (
          <React.Fragment key={problem.id}>
            {index > 0 ? <Divider /> : null}
            <ProblemRow problem={problem} onPress={() => {}} />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

const STARTER_CODE_BY_LANGUAGE: Record<number, string> = {
  109: "def two_sum(nums, target):\n    seen = {}\n    for i, n in enumerate(nums):\n        complement = target - n\n        if complement in seen:\n            return [seen[complement], i]\n        seen[n] = i\n    return []\n",
  102: "function twoSum(nums, target) {\n  const seen = new Map();\n  for (let i = 0; i < nums.length; i++) {\n    const complement = target - nums[i];\n    if (seen.has(complement)) return [seen.get(complement), i];\n    seen.set(nums[i], i);\n  }\n  return [];\n}\n",
};
const FIXTURE_LANGUAGES = [
  { languageId: 109, name: "Python (3.13.2)" },
  { languageId: 102, name: "JavaScript (Node 22)" },
  { languageId: 91, name: "Java (17)" },
];

function EditorFixtureSection() {
  const { colors } = useTheme();
  const [languageId, setLanguageId] = useState(109);
  const [code, setCode] = useState(STARTER_CODE_BY_LANGUAGE[109]!);
  const [hint, setHint] = useState<string | null>(null);

  function handleSelectLanguage(id: number) {
    setLanguageId(id);
    setCode(STARTER_CODE_BY_LANGUAGE[id] ?? "");
  }

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: problem detail + editor
      </Text>
      <Text className="mb-3 text-heading text-ink-primary dark:text-ink-primary-dark">Two Sum</Text>
      <View className="mb-5 flex-row flex-wrap items-center gap-2">
        <Badge label="Easy" variant="success" />
        <Badge label="Arrays" variant="neutral" />
        <Badge label="Interview question" variant="brand" />
      </View>
      <Text className="mb-6 text-body text-ink-primary dark:text-ink-primary-dark">
        Given an array of integers `nums` and an integer `target`, read a line of space-separated
        integers followed by the target on the next line, and print the two 0-indexed positions
        whose values add up to `target`, space-separated.
      </Text>
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Examples</Text>
      <Card className="mb-6">
        <Text className="mb-2 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
          Example 1
        </Text>
        <Text className="mb-1 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">Input</Text>
        <View className="mb-2 rounded-md border border-border bg-surface px-2.5 py-2 dark:border-border-dark dark:bg-surface-dark">
          <Text className="text-caption text-ink-primary dark:text-ink-primary-dark">2 7 11 15{"\n"}9</Text>
        </View>
        <Text className="mb-1 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
          Expected output
        </Text>
        <View className="rounded-md border border-border bg-surface px-2.5 py-2 dark:border-border-dark dark:bg-surface-dark">
          <Text className="text-caption text-ink-primary dark:text-ink-primary-dark">0 1</Text>
        </View>
      </Card>
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Language</Text>
      <View className="mb-5 flex-row flex-wrap gap-2">
        {FIXTURE_LANGUAGES.map((language) => (
          <Chip
            key={language.languageId}
            label={language.name}
            selected={language.languageId === languageId}
            onPress={() => handleSelectLanguage(language.languageId)}
          />
        ))}
      </View>
      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Your solution</Text>
      <CodeEditor value={code} onChangeText={setCode} className="mb-4" />
      <View className="mb-4">
        <Button title="Get a hint" variant="secondary" onPress={() => setHint("Try using a hash map to store each number's index as you scan the array once — you can find the complement in O(1) instead of scanning again.")} />
      </View>
      {hint ? (
        <Card className="mb-4 border-brand dark:border-brand-light">
          <View className="mb-2 flex-row items-center justify-between">
            <View className="flex-row items-center">
              <Icon name="bulb" size={18} color={colors.brand} />
              <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Hint</Text>
            </View>
            <Button title="Dismiss" variant="ghost" fullWidth={false} onPress={() => setHint(null)} />
          </View>
          <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{hint}</Text>
        </Card>
      ) : null}
      <Button title="Submit" onPress={() => {}} />
    </View>
  );
}

const FIXTURE_RUNNING_STATE = (
  <Card className="items-center">
    <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
      Running your code against every test case — this can take a few seconds…
    </Text>
  </Card>
);

const FIXTURE_ACCEPTED_RESULT: SubmitSolutionResponse = {
  status: "accepted",
  testsPassed: 6,
  totalTests: 6,
  xpAwarded: 15,
  coinsAwarded: 3,
  alreadySolved: false,
  results: [
    { input: "2 7 11 15\n9", expectedOutput: "0 1", actualOutput: "0 1", passed: true, isSample: true },
    { input: "3 2 4\n6", expectedOutput: "1 2", actualOutput: "1 2", passed: true, isSample: true },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
  ],
};

const FIXTURE_WRONG_ANSWER_RESULT: SubmitSolutionResponse = {
  status: "wrongAnswer",
  testsPassed: 3,
  totalTests: 6,
  xpAwarded: 0,
  coinsAwarded: 0,
  alreadySolved: false,
  results: [
    { input: "2 7 11 15\n9", expectedOutput: "0 1", actualOutput: "0 1", passed: true, isSample: true },
    { input: "3 2 4\n6", expectedOutput: "1 2", actualOutput: "2 1", passed: false, isSample: true },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: false, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: true, isSample: false },
    { input: null, expectedOutput: null, actualOutput: null, passed: false, isSample: false },
  ],
};

function ResultsFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: submission results
      </Text>
      <Text className="mb-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Running state</Text>
      <View className="mb-6">{FIXTURE_RUNNING_STATE}</View>
      <Text className="mb-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Accepted</Text>
      <TestResultsPanel result={FIXTURE_ACCEPTED_RESULT} className="mb-6" />
      <Text className="mb-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Wrong answer</Text>
      <TestResultsPanel result={FIXTURE_WRONG_ANSWER_RESULT} />
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Coding Practice UI, mirroring
 * the pattern established by `app/(dev)/quiz-preview.tsx` and
 * `app/(dev)/mocktests-preview.tsx`. Not linked from any navigation —
 * reached directly at `/(dev)/coding-preview`.
 */
export default function CodingPracticePreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Coding Practice Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <ProblemsListFixtureSection />
      <EditorFixtureSection />
      <ResultsFixtureSection />
    </ScreenContainer>
  );
}

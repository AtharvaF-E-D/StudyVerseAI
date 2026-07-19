import React, { useState } from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { Icon } from "../../src/components/Icon";
import { TextField } from "../../src/components/TextField";
import { InterviewCategoryStartCard } from "../../src/components/interview/InterviewCategoryStartCard";
import { INTERVIEW_CATEGORY_LABELS } from "../../src/components/interview/category";
import { ResumeAnalysisSections } from "../../src/components/interview/ResumeAnalysisSections";
import { useTheme } from "../../src/theme/ThemeProvider";
import { INTERVIEW_CATEGORIES } from "../../src/api/interviewprep";
import type { ResumeAnalysisDto } from "../../src/api/interviewprep";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/interview-preview`. Mirrors `app/(dev)/coding-preview.tsx`'s
// approach: NO trace of an Interview Prep feature exists anywhere in
// `backend/src` yet (confirmed by grepping the whole backend tree for
// "interview" — see `src/api/interviewprep.ts`'s header for the full audit),
// and `http://localhost:5221` refuses connections in this environment. So
// this feeds hand-written fixtures straight into the exact shared components
// the real screens use (`InterviewCategoryStartCard`, `ResumeAnalysisSections`)
// for manual/automated visual QA in both light and dark mode. None of the
// data below comes from — or is wired to — the real interview prep API.
// Delete this file once the real backend is reachable and the real screens
// have been re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_STATS = {
  sessionsCompleted: 4,
  averageScoreByType: { hr: 82, technical: 71, behavioral: 76 },
  resumeAnalysesCount: 2,
};

function StatsAndCategoriesFixtureSection() {
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: stats + category picker
      </Text>
      <Card className="mb-6">
        <View className="mb-4 flex-row items-center justify-between">
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
              {FIXTURE_STATS.sessionsCompleted}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Sessions completed
            </Text>
          </View>
          <View className="flex-1 items-center">
            <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
              {FIXTURE_STATS.resumeAnalysesCount}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Resume analyses</Text>
          </View>
        </View>
        <Divider className="mb-4" />
        <Text className="mb-2 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
          Average score by type
        </Text>
        <View className="flex-row items-center justify-between">
          {INTERVIEW_CATEGORIES.map((category) => (
            <View key={category} className="flex-1 items-center">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
                {FIXTURE_STATS.averageScoreByType[category]}
              </Text>
              <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
                {INTERVIEW_CATEGORY_LABELS[category]}
              </Text>
            </View>
          ))}
        </View>
      </Card>

      <View className="gap-3">
        {INTERVIEW_CATEGORIES.map((category) => (
          <InterviewCategoryStartCard
            key={category}
            category={category}
            averageScore={FIXTURE_STATS.averageScoreByType[category]}
            starting={false}
            onStart={() => {}}
          />
        ))}
      </View>
    </View>
  );
}

const FIXTURE_QUESTIONS = [
  "Tell me about a time you disagreed with a teammate's technical decision. How did you handle it?",
  "How would you design a rate limiter for a public API?",
  "Why do you want to work here specifically, rather than at a competitor?",
];

function MidSessionFixtureSection() {
  const { colors } = useTheme();
  const [answerText, setAnswerText] = useState(
    "I once disagreed with a teammate about caching strategy for a high-traffic endpoint. Instead of pushing back in the PR thread, I set up a quick pairing session, we benchmarked both approaches against real traffic data, and picked the one with better p99 latency. We ended up documenting the decision so future contributors wouldn't re-litigate it.",
  );
  const [graded, setGraded] = useState<{ score: number; feedback: string } | null>({
    score: 88,
    feedback:
      "Strong answer — you gave a concrete example, showed you resolved disagreement with data rather than authority, and mentioned documenting the outcome for the team. Consider naming the specific metric or business impact next time for extra weight.",
  });

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: mid-session Q&amp;A
      </Text>
      <Text className="mb-1 text-heading text-ink-primary dark:text-ink-primary-dark">Behavioral practice</Text>
      <Text className="mb-3 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        Question 1 of 3 · 1 answered
      </Text>
      <View className="mb-5 h-2 w-full overflow-hidden rounded-full bg-border dark:bg-border-dark">
        <View className="h-full rounded-full bg-brand dark:bg-brand-light" style={{ width: "33%" }} />
      </View>
      <Card className="mb-5">
        <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">{FIXTURE_QUESTIONS[0]}</Text>
      </Card>

      {graded ? (
        <>
          <Card className="mb-5 border-brand dark:border-brand-light">
            <View className="mb-2 flex-row items-center justify-between">
              <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">Your score</Text>
              <Text className="text-subheading text-brand dark:text-brand-light">{graded.score}</Text>
            </View>
            <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{graded.feedback}</Text>
          </Card>
          <Button title="Next question" onPress={() => setGraded(null)} />
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
            containerClassName="mb-2"
          />
          <Button
            title="Submit answer"
            onPress={() => setGraded({ score: 88, feedback: "Strong, specific answer with a clear resolution and outcome." })}
          />
        </>
      )}

      <Text className="mb-2 mt-8 text-subheading text-ink-primary dark:text-ink-primary-dark">
        Grading… state
      </Text>
      <Card className="items-center">
        <Icon name="hourglass-outline" size={20} color={colors.textSecondary} />
        <Text className="mt-2 text-body text-ink-secondary dark:text-ink-secondary-dark">
          Grading your answer — this can take a few seconds…
        </Text>
      </Card>
    </View>
  );
}

function CompletedSessionFixtureSection() {
  const { colors } = useTheme();
  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: completed session
      </Text>
      <Card className="mb-5 items-center">
        <Icon name="trophy" size={32} color={colors.warning} />
        <Text className="mt-2 text-display text-ink-primary dark:text-ink-primary-dark">79</Text>
        <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Behavioral · overall score
        </Text>
      </Card>
      <Card className="border-brand dark:border-brand-light">
        <View className="mb-2 flex-row items-center">
          <Icon name="bulb" size={20} color={colors.brand} />
          <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Your improvement plan
          </Text>
        </View>
        <Text className="text-body text-ink-primary dark:text-ink-primary-dark">
          You consistently give concrete, well-structured examples — keep that up. Work on quantifying impact (metrics,
          time saved, revenue) in at least one sentence per answer, and practice tightening your responses to under
          90 seconds so interviewers don&apos;t lose the thread of the STAR structure.
        </Text>
      </Card>
    </View>
  );
}

const FIXTURE_RESUME_ANALYSIS: ResumeAnalysisDto = {
  id: "fixture-analysis-1",
  overallScore: 74,
  strengths: [
    "Clear reverse-chronological work history with consistent formatting",
    "Quantified impact in most bullet points (e.g. \"reduced page load time by 38%\")",
    "Skills section is well-organized and relevant to the target role",
  ],
  weaknesses: [
    "No summary/objective statement at the top to frame the rest of the resume",
    "A few bullet points describe responsibilities rather than outcomes",
    "Resume is two pages for under five years of experience",
  ],
  suggestions: [
    "Add a 2-3 sentence summary tailored to the roles you're applying for",
    "Rewrite responsibility-style bullets ('Responsible for...') as outcome-style ('Reduced...', 'Led...', 'Shipped...')",
    "Trim to one page by cutting older, less-relevant roles or condensing bullet counts",
  ],
  createdAtUtc: new Date().toISOString(),
};

function ResumeAnalysisFixtureSection() {
  return (
    <View>
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: resume analysis result
      </Text>
      <ResumeAnalysisSections analysis={FIXTURE_RESUME_ANALYSIS} />
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Interview Prep UI, mirroring the
 * pattern established by `app/(dev)/coding-preview.tsx` and
 * `app/(dev)/mocktests-preview.tsx`. Not linked from any navigation — reached
 * directly at `/(dev)/interview-preview`.
 */
export default function InterviewPracticePreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">
          Interview Prep Preview (dev)
        </Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <StatsAndCategoriesFixtureSection />
      <MidSessionFixtureSection />
      <CompletedSessionFixtureSection />
      <ResumeAnalysisFixtureSection />
    </ScreenContainer>
  );
}

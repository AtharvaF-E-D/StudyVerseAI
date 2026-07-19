import React from "react";
import { Text, View } from "react-native";

import { Badge, type BadgeVariant } from "../Badge";
import { Card } from "../Card";
import { Icon } from "../Icon";
import { MONOSPACE_FONT_FAMILY } from "./CodeEditor";
import { useTheme } from "../../theme/ThemeProvider";
import type { CodeSubmissionStatus, SubmissionTestResultDto, SubmitSolutionResponse } from "../../api/codingpractice";

const STATUS_LABELS: Record<CodeSubmissionStatus, string> = {
  accepted: "Accepted",
  wrongAnswer: "Wrong Answer",
  compileError: "Compile Error",
  runtimeError: "Runtime Error",
  error: "Error",
};

const STATUS_BADGE_VARIANT: Record<CodeSubmissionStatus, BadgeVariant> = {
  accepted: "success",
  wrongAnswer: "danger",
  compileError: "danger",
  runtimeError: "danger",
  error: "danger",
};

export interface TestResultsPanelProps {
  result: SubmitSolutionResponse;
  className?: string;
}

function MonoBlock({ label, content, tone }: { label: string; content: string; tone?: "danger" | "success" }) {
  return (
    <View className="mb-2">
      <Text className="mb-1 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
        {label}
      </Text>
      <View
        className={[
          "rounded-md border px-2.5 py-2",
          tone === "danger"
            ? "border-danger/40 bg-danger/5"
            : tone === "success"
              ? "border-success/40 bg-success/5"
              : "border-border bg-surface dark:border-border-dark dark:bg-surface-dark",
        ].join(" ")}
      >
        <Text style={{ fontFamily: MONOSPACE_FONT_FAMILY }} className="text-caption text-ink-primary dark:text-ink-primary-dark">
          {content.length > 0 ? content : "(empty)"}
        </Text>
      </View>
    </View>
  );
}

function SampleTestCard({ index, testCase }: { index: number; testCase: SubmissionTestResultDto }) {
  const { colors } = useTheme();

  return (
    <Card className="mb-3">
      <View className="mb-2 flex-row items-center justify-between">
        <Text className="text-body font-medium text-ink-primary dark:text-ink-primary-dark">
          Sample test {index + 1}
        </Text>
        <View className="flex-row items-center">
          <Icon
            name={testCase.passed ? "checkmark-circle" : "close-circle"}
            size={18}
            color={testCase.passed ? colors.success : colors.danger}
          />
          <Text className={["ml-1 text-caption font-medium", testCase.passed ? "text-success" : "text-danger"].join(" ")}>
            {testCase.passed ? "Passed" : "Failed"}
          </Text>
        </View>
      </View>
      <MonoBlock label="Input" content={testCase.input ?? ""} />
      <MonoBlock label="Expected output" content={testCase.expectedOutput ?? ""} />
      <MonoBlock
        label="Your output"
        content={testCase.actualOutput ?? ""}
        tone={testCase.passed ? "success" : "danger"}
      />
    </Card>
  );
}

/**
 * Hidden (non-sample) test results — deliberately renders NOTHING but a
 * pass/fail dot per test, mirroring the backend's anti-leak design honestly:
 * `SubmissionTestResultDto.input`/`expectedOutput`/`actualOutput` must never
 * be displayed here even if a field happens to be populated, since real
 * hidden-test content is exactly what StudyVerse never leaks to the client
 * (same reasoning as quiz answers never including the correct index).
 */
function HiddenTestsRow({ items }: { items: SubmissionTestResultDto[] }) {
  const { colors } = useTheme();

  if (items.length === 0) return null;

  const passedCount = items.filter((item) => item.passed).length;

  return (
    <View className="mb-1">
      <Text className="mb-2 text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
        Hidden tests ({passedCount}/{items.length} passed)
      </Text>
      <View className="flex-row flex-wrap gap-2">
        {items.map((item, index) => (
          <View
            key={index}
            accessibilityLabel={`Hidden test ${index + 1}: ${item.passed ? "passed" : "failed"}`}
            className="h-3 w-3 rounded-full"
            style={{ backgroundColor: item.passed ? colors.success : colors.danger }}
          />
        ))}
      </View>
    </View>
  );
}

/**
 * Submission results panel: color-coded overall status, an X/Y passed
 * summary, XP/coins earned (first-ever Accepted only), every SAMPLE test's
 * full input/expected/actual, and — honestly — nothing but a pass/fail dot
 * per hidden test.
 */
export function TestResultsPanel({ result, className = "" }: TestResultsPanelProps) {
  const { colors } = useTheme();
  const isAccepted = result.status === "accepted";
  const sampleResults = result.results.filter((r) => r.isSample);
  const hiddenResults = result.results.filter((r) => !r.isSample);

  return (
    <View className={className}>
      <Card className={["mb-4", isAccepted ? "border-success" : "border-danger"].join(" ")}>
        <View className="mb-2 flex-row items-center justify-between">
          <View className="flex-row items-center">
            <Icon
              name={isAccepted ? "checkmark-circle" : "close-circle"}
              size={22}
              color={isAccepted ? colors.success : colors.danger}
            />
            <Text className={["ml-2 text-subheading", isAccepted ? "text-success" : "text-danger"].join(" ")}>
              {STATUS_LABELS[result.status]}
            </Text>
          </View>
          <Badge
            label={`${result.testsPassed}/${result.totalTests} passed`}
            variant={STATUS_BADGE_VARIANT[result.status]}
          />
        </View>
        {isAccepted ? (
          result.alreadySolved ? (
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              Already solved before — no additional XP/coins awarded.
            </Text>
          ) : (
            <Text className="text-caption font-semibold text-brand dark:text-brand-light">
              +{result.xpAwarded} XP · +{result.coinsAwarded} coins
            </Text>
          )
        ) : null}
      </Card>

      {sampleResults.map((testCase, index) => (
        <SampleTestCard key={index} index={index} testCase={testCase} />
      ))}

      <HiddenTestsRow items={hiddenResults} />
    </View>
  );
}

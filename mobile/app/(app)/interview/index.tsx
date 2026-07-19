import React, { useState } from "react";
import { Text, View } from "react-native";
import { router } from "expo-router";
import * as DocumentPicker from "expo-document-picker";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Button } from "../../../src/components/Button";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { ListItem } from "../../../src/components/ListItem";
import { Skeleton } from "../../../src/components/Skeleton";
import { InterviewCategoryStartCard } from "../../../src/components/interview/InterviewCategoryStartCard";
import { INTERVIEW_CATEGORY_LABELS } from "../../../src/components/interview/category";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { formatRelativeTime } from "../../../src/lib/relativeTime";
import { stashStartedInterviewSession } from "../../../src/lib/interviewSessionCache";
import {
  INTERVIEW_CATEGORIES,
  MAX_RESUME_FILE_SIZE_BYTES,
  SUPPORTED_RESUME_DOCUMENT_MIME_TYPES,
  type InterviewCategory,
  type InterviewSessionHistoryItemDto,
  type ResumeAnalysisDto,
} from "../../../src/api/interviewprep";
import {
  useCreateInterviewSessionMutation,
  useInterviewSessionsQuery,
  useInterviewStatsQuery,
  useResumeHistoryQuery,
  useUploadResumeMutation,
} from "../../../src/hooks/useInterviewPrep";

function StatsStripSkeleton() {
  return (
    <Card className="mb-6">
      <View className="flex-row items-center justify-between">
        {[0, 1].map((i) => (
          <View key={i} className="flex-1 items-center">
            <Skeleton variant="text" width={32} className="mb-2" />
            <Skeleton variant="text" width={90} />
          </View>
        ))}
      </View>
    </Card>
  );
}

/** Sessions completed, resume analyses count, and average score per category — the three things the phase brief calls out for the stats strip. */
function StatsStrip() {
  const statsQuery = useInterviewStatsQuery();

  if (statsQuery.isLoading) return <StatsStripSkeleton />;
  if (statsQuery.isError || !statsQuery.data) return null;

  const stats = statsQuery.data;

  return (
    <Card className="mb-6">
      <View className="mb-4 flex-row items-center justify-between">
        <View className="flex-1 items-center">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {stats.sessionsCompleted}
          </Text>
          <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">Sessions completed</Text>
        </View>
        <View className="flex-1 items-center">
          <Text className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {stats.resumeAnalysesCount}
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
              {Math.round(stats.averageScoreByType[category])}
            </Text>
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">
              {INTERVIEW_CATEGORY_LABELS[category]}
            </Text>
          </View>
        ))}
      </View>
    </Card>
  );
}

function ResumeHistoryRow({ analysis, onPress }: { analysis: ResumeAnalysisDto; onPress: () => void }) {
  const { colors } = useTheme();
  return (
    <ListItem
      leading={<Icon name="document-text-outline" size={22} color={colors.brand} />}
      title={`Resume score: ${Math.round(analysis.overallScore)}`}
      subtitle={formatRelativeTime(analysis.createdAtUtc)}
      trailing={<Icon name="chevron-forward" size={18} color={colors.textSecondary} />}
      onPress={onPress}
    />
  );
}

function SessionHistoryRow({ session, onPress }: { session: InterviewSessionHistoryItemDto; onPress: () => void }) {
  const { colors } = useTheme();
  const isComplete = session.overallScore !== null;
  return (
    <ListItem
      leading={
        <Icon
          name={isComplete ? "checkmark-circle" : "ellipse-outline"}
          size={22}
          color={isComplete ? colors.success : colors.textSecondary}
        />
      }
      title={`${INTERVIEW_CATEGORY_LABELS[session.type]} · ${session.questionCount} question${session.questionCount === 1 ? "" : "s"}`}
      subtitle={formatRelativeTime(session.createdAtUtc)}
      trailing={
        isComplete ? (
          <Badge label={`Score ${Math.round(session.overallScore!)}`} variant="brand" />
        ) : (
          <Badge label="In progress" variant="warning" />
        )
      }
      onPress={onPress}
    />
  );
}

/**
 * Interview Prep entry point: stats strip, three category cards that each
 * start a new practice session, a Resume Review section (upload + past
 * analyses), and a session history list below. Reached from the "Interview
 * Prep" card on the dashboard.
 */
export default function InterviewPrepScreen() {
  const { show } = useToast();

  const [startingCategory, setStartingCategory] = useState<InterviewCategory | null>(null);
  const [isUploadingResume, setIsUploadingResume] = useState(false);

  const statsQuery = useInterviewStatsQuery();
  const sessionsQuery = useInterviewSessionsQuery();
  const resumeHistoryQuery = useResumeHistoryQuery();
  const createSessionMutation = useCreateInterviewSessionMutation();
  const uploadResumeMutation = useUploadResumeMutation();

  function handleStartSession(category: InterviewCategory) {
    if (createSessionMutation.isPending) return;
    setStartingCategory(category);
    createSessionMutation.mutate(
      { type: category },
      {
        onSuccess: (result) => {
          stashStartedInterviewSession(result.id, result);
          setStartingCategory(null);
          router.push(`/(app)/interview/${result.id}`);
        },
        onError: () => {
          setStartingCategory(null);
          show("Couldn't start a new session. Please try again.", "danger");
        },
      },
    );
  }

  async function handleUploadResume() {
    if (uploadResumeMutation.isPending) return;
    const result = await DocumentPicker.getDocumentAsync({
      type: SUPPORTED_RESUME_DOCUMENT_MIME_TYPES,
      copyToCacheDirectory: true,
    });
    if (result.canceled || result.assets.length === 0) return;

    const asset = result.assets[0];
    if (asset.size !== undefined && asset.size > MAX_RESUME_FILE_SIZE_BYTES) {
      show("That file is too large. Files must be 10MB or smaller.", "danger");
      return;
    }

    setIsUploadingResume(true);
    uploadResumeMutation.mutate(
      { uri: asset.uri, name: asset.name, mimeType: asset.mimeType ?? "application/octet-stream", webFile: asset.file },
      {
        onSuccess: (analysis) => {
          setIsUploadingResume(false);
          router.push(`/(app)/interview/resume/${analysis.id}`);
        },
        onError: () => {
          setIsUploadingResume(false);
          show("Couldn't analyze that resume. Please try again.", "danger");
        },
      },
    );
  }

  function openSession(sessionId: string) {
    router.push(`/(app)/interview/${sessionId}`);
  }

  function openResumeAnalysis(analysisId: string) {
    router.push(`/(app)/interview/resume/${analysisId}`);
  }

  const sessions = sessionsQuery.data ?? [];
  const resumeHistory = resumeHistoryQuery.data ?? [];

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">Interview Prep</Text>

      <StatsStrip />

      <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Start a session</Text>
      <View className="mb-6 gap-3">
        {INTERVIEW_CATEGORIES.map((category) => (
          <InterviewCategoryStartCard
            key={category}
            category={category}
            averageScore={statsQuery.data?.averageScoreByType[category]}
            starting={startingCategory === category}
            onStart={handleStartSession}
          />
        ))}
      </View>

      <View className="mb-6">
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Resume Review</Text>
        <Card className="mb-4">
          <Text className="mb-4 text-body text-ink-secondary dark:text-ink-secondary-dark">
            Upload your resume and get a real AI-generated score, strengths, weaknesses, and suggestions.
          </Text>
          <Button
            title="Upload resume"
            loading={isUploadingResume}
            disabled={isUploadingResume}
            onPress={() => void handleUploadResume()}
          />
        </Card>

        {resumeHistoryQuery.isLoading ? (
          <Card>
            {[0, 1].map((i) => (
              <View key={i} className="px-3 py-3">
                <Skeleton variant="text" width="45%" className="mb-2" />
                <Skeleton variant="text" width="30%" />
              </View>
            ))}
          </Card>
        ) : resumeHistoryQuery.isError ? (
          <ErrorState
            title="Couldn't load your resume analyses"
            description="Check your connection and try again."
            onRetry={() => void resumeHistoryQuery.refetch()}
          />
        ) : resumeHistory.length === 0 ? (
          <EmptyState
            icon="document-text-outline"
            title="No resume analyses yet"
            description="Upload your resume above to get real AI feedback."
          />
        ) : (
          <Card>
            {resumeHistory.map((analysis, index) => (
              <React.Fragment key={analysis.id}>
                {index > 0 ? <Divider /> : null}
                <ResumeHistoryRow analysis={analysis} onPress={() => openResumeAnalysis(analysis.id)} />
              </React.Fragment>
            ))}
          </Card>
        )}
      </View>

      <View>
        <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">Session history</Text>
        {sessionsQuery.isLoading ? (
          <Card>
            {[0, 1, 2].map((i) => (
              <View key={i} className="px-3 py-3">
                <Skeleton variant="text" width="55%" className="mb-2" />
                <Skeleton variant="text" width="35%" />
              </View>
            ))}
          </Card>
        ) : sessionsQuery.isError ? (
          <ErrorState
            title="Couldn't load your session history"
            description="Check your connection and try again."
            onRetry={() => void sessionsQuery.refetch()}
          />
        ) : sessions.length === 0 ? (
          <EmptyState
            icon="briefcase-outline"
            title="No practice sessions yet"
            description="Start a session above to begin practicing real interview questions."
          />
        ) : (
          <Card>
            {sessions.map((session, index) => (
              <React.Fragment key={session.id}>
                {index > 0 ? <Divider /> : null}
                <SessionHistoryRow session={session} onPress={() => openSession(session.id)} />
              </React.Fragment>
            ))}
          </Card>
        )}
      </View>
    </ScreenContainer>
  );
}

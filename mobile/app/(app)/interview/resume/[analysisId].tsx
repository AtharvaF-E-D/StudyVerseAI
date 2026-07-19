import React from "react";
import { View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../../src/components/ScreenContainer";
import { Button } from "../../../../src/components/Button";
import { Card } from "../../../../src/components/Card";
import { ErrorState } from "../../../../src/components/ErrorState";
import { Skeleton } from "../../../../src/components/Skeleton";
import { ResumeAnalysisSections } from "../../../../src/components/interview/ResumeAnalysisSections";
import { useResumeHistoryQuery } from "../../../../src/hooks/useInterviewPrep";

function ResumeAnalysisSkeleton() {
  return (
    <View>
      <Card className="mb-5 items-center">
        <Skeleton variant="circle" className="mb-3" />
        <Skeleton variant="text" width={60} className="mb-2" />
        <Skeleton variant="text" width={140} />
      </Card>
      <Card className="mb-5">
        <Skeleton variant="text" width="40%" className="mb-3" />
        <Skeleton variant="text" width="90%" className="mb-2" />
        <Skeleton variant="text" width="70%" />
      </Card>
      <Card>
        <Skeleton variant="text" width="40%" className="mb-3" />
        <Skeleton variant="text" width="90%" className="mb-2" />
        <Skeleton variant="text" width="60%" />
      </Card>
    </View>
  );
}

/**
 * Resume analysis result screen — reached either right after a successful
 * upload from the Interview Prep home screen, or by tapping a past analysis
 * in its "Resume Review" history list. Reads the specific analysis out of
 * `useResumeHistoryQuery`'s cache rather than fetching it individually: the
 * contract has no dedicated `GET /resume/{id}` detail endpoint, and
 * `GET /resume/history` already returns every analysis in full (see
 * `src/api/interviewprep.ts`'s header) — `useUploadResumeMutation` also
 * seeds a fresh upload straight into that same cache, so this screen usually
 * renders instantly with no extra round trip at all.
 */
export default function ResumeAnalysisScreen() {
  const params = useLocalSearchParams<{ analysisId: string }>();
  const analysisId = params.analysisId ?? "";

  const historyQuery = useResumeHistoryQuery();
  const analysis = historyQuery.data?.find((item) => item.id === analysisId);

  return (
    <ScreenContainer>
      {historyQuery.isLoading ? (
        <ResumeAnalysisSkeleton />
      ) : historyQuery.isError ? (
        <ErrorState
          title="Couldn't load this analysis"
          description="Check your connection and try again."
          onRetry={() => void historyQuery.refetch()}
        />
      ) : !analysis ? (
        <ErrorState
          icon="document-text-outline"
          title="Analysis not found"
          description="This resume analysis may no longer be available."
          retryLabel="Back to Interview Prep"
          onRetry={() => router.replace("/(app)/interview")}
        />
      ) : (
        <>
          <ResumeAnalysisSections analysis={analysis} className="mb-6" />
          <Button title="Back to Interview Prep" variant="secondary" onPress={() => router.replace("/(app)/interview")} />
        </>
      )}
    </ScreenContainer>
  );
}

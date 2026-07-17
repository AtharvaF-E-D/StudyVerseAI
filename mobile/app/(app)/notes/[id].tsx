import React from "react";
import { ActivityIndicator, Alert, Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Badge } from "../../../src/components/Badge";
import { Card } from "../../../src/components/Card";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { NoteDetailContent } from "../../../src/components/notes/NoteDetailContent";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { NOTE_STATUS_BADGE_VARIANTS, NOTE_STATUS_LABELS } from "../../../src/lib/noteStatus";
import { useDeleteNoteMutation, useNoteQuery } from "../../../src/hooks/useNotes";

function DetailSkeleton() {
  return (
    <View>
      <Skeleton variant="rect" width="100%" height={36} className="mb-4 rounded-full" />
      <Card>
        <Skeleton variant="text" width="90%" className="mb-2" />
        <Skeleton variant="text" width="75%" className="mb-2" />
        <Skeleton variant="text" width="60%" />
      </Card>
    </View>
  );
}

/**
 * One note's detail screen — a segmented view (`NoteDetailContent`) across
 * Summary, Key Points, Flashcards, MCQs, Mind Map, Revision Sheet,
 * Vocabulary, and Formulas. While the note is still `"processing"`,
 * `useNoteQuery`'s `refetchInterval` polls until the backend flips it to
 * `"ready"`/`"failed"` — this screen just renders whichever of those three
 * states the query currently reflects.
 */
export default function NoteDetailScreen() {
  const params = useLocalSearchParams<{ id: string }>();
  const noteId = params.id ?? "";

  const { colors } = useTheme();
  const { show } = useToast();

  const noteQuery = useNoteQuery(noteId);
  const deleteMutation = useDeleteNoteMutation();

  function confirmDelete() {
    if (!noteQuery.data) return;
    Alert.alert("Delete note", `Delete "${noteQuery.data.title}"? This can't be undone.`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () =>
          deleteMutation.mutate(noteId, {
            onSuccess: () => router.back(),
            onError: () => show("Couldn't delete that note.", "danger"),
          }),
      },
    ]);
  }

  const note = noteQuery.data;

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center">
        <Pressable
          onPress={() => router.back()}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back"
          className="mr-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        <View className="flex-1">
          <Text numberOfLines={1} className="text-subheading text-ink-primary dark:text-ink-primary-dark">
            {note?.title ?? "Note"}
          </Text>
          {note ? (
            <Badge
              label={NOTE_STATUS_LABELS[note.status]}
              variant={NOTE_STATUS_BADGE_VARIANTS[note.status]}
              className="mt-1"
            />
          ) : null}
        </View>
        {note ? (
          <Pressable
            onPress={confirmDelete}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel="Delete note"
            className="ml-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon name="trash-outline" size={20} color={colors.danger} />
          </Pressable>
        ) : null}
      </View>

      {noteQuery.isLoading ? (
        <DetailSkeleton />
      ) : noteQuery.isError ? (
        <ErrorState
          title="Couldn't load this note"
          description="Check your connection and try again."
          onRetry={() => void noteQuery.refetch()}
        />
      ) : !note ? null : note.status === "processing" ? (
        <Card>
          <View className="items-center py-6">
            <ActivityIndicator size="large" color={colors.brand} />
            <Text className="mt-4 text-center text-body text-ink-secondary dark:text-ink-secondary-dark">
              StudyVerse is generating your summary, flashcards, and more. This usually only takes a moment.
            </Text>
          </View>
        </Card>
      ) : note.status === "failed" ? (
        <ErrorState
          icon="alert-circle-outline"
          title="Couldn't generate this note"
          description={note.errorMessage || "Something went wrong while processing this file. Try uploading it again."}
          onRetry={() => void noteQuery.refetch()}
          retryLabel="Check again"
        />
      ) : note.content ? (
        <NoteDetailContent content={note.content} />
      ) : (
        // Defensive fallback only — the backend never actually sends `status: "ready"` with a
        // null `content` (upload is synchronous and only flips to Ready once content is saved),
        // but nothing here assumes that invariant holds forever.
        <EmptyState icon="document-text-outline" title="No content available for this note" />
      )}
    </ScreenContainer>
  );
}

import React, { useState } from "react";
import { Alert, Pressable, Text, View } from "react-native";
import { router } from "expo-router";
import * as DocumentPicker from "expo-document-picker";
import * as ImagePicker from "expo-image-picker";

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
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import { formatRelativeTime } from "../../../src/lib/relativeTime";
import { NOTE_STATUS_BADGE_VARIANTS, NOTE_STATUS_LABELS } from "../../../src/lib/noteStatus";
import {
  MAX_NOTE_FILE_SIZE_BYTES,
  SUPPORTED_NOTE_DOCUMENT_MIME_TYPES,
  type NoteSummaryDto,
  type UploadNoteFile,
} from "../../../src/api/notes";
import { useDeleteNoteMutation, useNotesQuery, useUploadNoteMutation } from "../../../src/hooks/useNotes";

type PickerSource = "document" | "photo";

function NotesListSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="flex-row items-center px-3 py-3">
          <Skeleton variant="circle" width={22} height={22} className="mr-3" />
          <View className="flex-1">
            <Skeleton variant="text" width="55%" className="mb-2" />
            <Skeleton variant="text" width="35%" />
          </View>
        </View>
      ))}
    </Card>
  );
}

/**
 * "AI Notes" list/upload screen — the entry point reached from the "AI
 * Notes" card on the dashboard. Uploading (document or photo) blocks on the
 * backend's synchronous generation pipeline, then navigates straight to the
 * new note's detail screen once the request resolves to `"ready"`/`"failed"`.
 * Existing notes in the list also poll (via `useNotesQuery`'s
 * `refetchInterval`) while any of them are still `"processing"`, so the UI
 * still handles that window gracefully if a note is ever left mid-generation.
 */
export default function NotesListScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [pickerBusy, setPickerBusy] = useState<PickerSource | null>(null);

  const notesQuery = useNotesQuery();
  const uploadMutation = useUploadNoteMutation();
  const deleteMutation = useDeleteNoteMutation();

  function startUpload(file: UploadNoteFile, source: PickerSource) {
    setPickerBusy(source);
    uploadMutation.mutate(file, {
      onSuccess: (result) => {
        setPickerBusy(null);
        router.push(`/(app)/notes/${result.id}`);
      },
      onError: () => {
        setPickerBusy(null);
        show("Couldn't upload that file. Please try again.", "danger");
      },
    });
  }

  async function handlePickDocument() {
    if (uploadMutation.isPending) return;
    const result = await DocumentPicker.getDocumentAsync({
      type: SUPPORTED_NOTE_DOCUMENT_MIME_TYPES,
      copyToCacheDirectory: true,
    });
    if (result.canceled || result.assets.length === 0) return;

    const asset = result.assets[0];
    if (asset.size !== undefined && asset.size > MAX_NOTE_FILE_SIZE_BYTES) {
      show("That file is too large. Files must be 10MB or smaller.", "danger");
      return;
    }

    startUpload(
      { uri: asset.uri, name: asset.name, mimeType: asset.mimeType ?? "application/octet-stream", webFile: asset.file },
      "document",
    );
  }

  async function handlePickPhoto() {
    if (uploadMutation.isPending) return;
    const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permission.granted) {
      show("Photo library permission is required to upload a photo.", "warning");
      return;
    }
    // Only the photo-library picker is offered (no in-app camera capture) — mediaTypes is scoped
    // to images since the backend only accepts pdf/docx/jpg/png (see `NoteFileTypeResolver`).
    const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: ["images"], quality: 0.8 });
    if (result.canceled || result.assets.length === 0) return;

    const asset = result.assets[0];
    if (asset.fileSize !== undefined && asset.fileSize > MAX_NOTE_FILE_SIZE_BYTES) {
      show("That photo is too large. Files must be 10MB or smaller.", "danger");
      return;
    }

    const name = asset.fileName ?? `photo-${Date.now()}.jpg`;
    startUpload({ uri: asset.uri, name, mimeType: asset.mimeType ?? "image/jpeg", webFile: asset.file }, "photo");
  }

  function confirmDelete(note: NoteSummaryDto) {
    Alert.alert("Delete note", `Delete "${note.title}"? This can't be undone.`, [
      { text: "Cancel", style: "cancel" },
      {
        text: "Delete",
        style: "destructive",
        onPress: () =>
          deleteMutation.mutate(note.id, {
            onError: () => show("Couldn't delete that note.", "danger"),
          }),
      },
    ]);
  }

  function openNote(note: NoteSummaryDto) {
    if (note.status !== "ready") return;
    router.push(`/(app)/notes/${note.id}`);
  }

  const notes = notesQuery.data ?? [];

  return (
    <ScreenContainer>
      <Text className="mb-6 text-heading text-ink-primary dark:text-ink-primary-dark">AI Notes</Text>

      <View className="mb-6 flex-row gap-3">
        <View className="flex-1">
          <Button
            title="Upload document"
            onPress={() => void handlePickDocument()}
            loading={pickerBusy === "document"}
            disabled={uploadMutation.isPending && pickerBusy !== "document"}
          />
        </View>
        <View className="flex-1">
          <Button
            title="Upload photo"
            variant="secondary"
            onPress={() => void handlePickPhoto()}
            loading={pickerBusy === "photo"}
            disabled={uploadMutation.isPending && pickerBusy !== "photo"}
          />
        </View>
      </View>

      {notesQuery.isLoading ? (
        <NotesListSkeleton />
      ) : notesQuery.isError ? (
        <ErrorState
          title="Couldn't load your notes"
          description="Check your connection and try again."
          onRetry={() => void notesQuery.refetch()}
        />
      ) : notes.length === 0 ? (
        <EmptyState
          icon="document-text-outline"
          title="No notes yet"
          description="Upload a document or photo of your notes and StudyVerse will turn it into a summary, flashcards, practice questions, and more."
        />
      ) : (
        <Card>
          {notes.map((note, index) => (
            <React.Fragment key={note.id}>
              {index > 0 ? <Divider /> : null}
              {/* The delete button is a sibling of ListItem, not inside its `trailing`
                  slot — ListItem itself renders as a <button> when `onPress` is set,
                  and nesting another pressable inside it produces invalid HTML
                  (<button> cannot contain a nested <button>) that real browsers warn
                  about and mishandle click-wise. */}
              <View className="flex-row items-center">
                <ListItem
                  leading={<Icon name="document-text-outline" size={22} color={colors.brand} />}
                  title={note.title}
                  subtitle={formatRelativeTime(note.createdAtUtc)}
                  onPress={note.status === "ready" ? () => openNote(note) : undefined}
                  trailing={
                    <Badge
                      label={NOTE_STATUS_LABELS[note.status]}
                      variant={NOTE_STATUS_BADGE_VARIANTS[note.status]}
                    />
                  }
                  className="flex-1"
                />
                <Pressable
                  onPress={() => confirmDelete(note)}
                  hitSlop={8}
                  accessibilityRole="button"
                  accessibilityLabel={`Delete ${note.title}`}
                  className="ml-2 mr-3"
                >
                  <Icon name="trash-outline" size={19} color={colors.danger} />
                </Pressable>
              </View>
            </React.Fragment>
          ))}
        </Card>
      )}
    </ScreenContainer>
  );
}

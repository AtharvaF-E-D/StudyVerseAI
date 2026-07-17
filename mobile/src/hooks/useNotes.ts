import { useMutation, useQuery, useQueryClient, type Query } from "@tanstack/react-query";

import {
  deleteNote,
  getNote,
  getNotes,
  uploadNote,
  type NoteDetailDto,
  type NoteSummaryDto,
  type UploadNoteFile,
} from "../api/notes";

export const notesListQueryKey = ["notes"] as const;

export function noteDetailQueryKey(noteId: string) {
  return ["notes", noteId] as const;
}

/**
 * The backend generates a note's content synchronously today, so `status`
 * should flip from `"processing"` to `"ready"`/`"failed"` almost
 * immediately — but the UI still polls briefly to cover that window
 * gracefully rather than assuming it's instant.
 */
const PROCESSING_POLL_INTERVAL_MS = 2500;

/** Fetches the signed-in user's notes, polling every few seconds while any of them are still `"processing"`. */
export function useNotesQuery() {
  return useQuery<NoteSummaryDto[]>({
    queryKey: notesListQueryKey,
    // Wrapped rather than passed as a bare reference: `getNotes` takes an
    // optional `take`, and React Query calls a bare `queryFn` with its
    // `QueryFunctionContext` as the first argument — which would otherwise
    // land in that param.
    queryFn: () => getNotes(),
    refetchInterval: (query: Query<NoteSummaryDto[]>) =>
      query.state.data?.some((note) => note.status === "processing") ? PROCESSING_POLL_INTERVAL_MS : false,
  });
}

/** Fetches one note's full detail (including generated content once ready), polling while it's still `"processing"`. */
export function useNoteQuery(noteId: string) {
  return useQuery<NoteDetailDto>({
    queryKey: noteDetailQueryKey(noteId),
    queryFn: () => getNote(noteId),
    enabled: noteId.length > 0,
    refetchInterval: (query: Query<NoteDetailDto>) =>
      query.state.data?.status === "processing" ? PROCESSING_POLL_INTERVAL_MS : false,
  });
}

/** Uploads a picked document/photo as a new note (the request itself blocks until the backend has resolved it to `"ready"`/`"failed"`), then invalidates the notes list so it appears there. */
export function useUploadNoteMutation() {
  const queryClient = useQueryClient();

  return useMutation<NoteSummaryDto, unknown, UploadNoteFile>({
    mutationFn: (file) => uploadNote(file),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notesListQueryKey });
    },
  });
}

/** Deletes a note, then refetches the notes list so it disappears. */
export function useDeleteNoteMutation() {
  const queryClient = useQueryClient();

  return useMutation<void, unknown, string>({
    mutationFn: (noteId) => deleteNote(noteId),
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: notesListQueryKey });
    },
  });
}

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "AI Notes" backend contract (base path
// `api/v1/notes`). Its shapes were cross-checked directly against the real
// backend source once it landed mid-session (`NotesController.cs`,
// `StudyVerse.Application/Features/Notes/**`, `Domain/Entities/Note.cs` +
// `NoteContent.cs`) — same as `quiz.ts`. Two things worth flagging that
// aren't obvious from field names alone:
//
// 1. Enums serialize as camelCase strings (`Program.cs` registers
//    `JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`), so
//    `NoteStatus.Processing` is the wire value `"processing"`, and
//    `NoteSourceFileType.Pdf` is `"pdf"`.
// 2. Upload is fully SYNCHRONOUS server-side: `POST /notes` doesn't return
//    until the note has already reached `"ready"` or `"failed"` (no
//    background job queue for this pass — see `NoteStatus`'s doc comment on
//    the backend). So in practice the response is never `"processing"`, and
//    neither list/detail polling below will usually have anything to do —
//    it's just there to handle that window gracefully if it's ever not
//    instant, per the phase spec.
// ---------------------------------------------------------------------------

export type NoteStatus = "processing" | "ready" | "failed";

export type NoteSourceFileType = "pdf" | "docx" | "image";

/** Mirrors `NoteFileTypeResolver`'s supported extensions/size cap on the backend. */
export const MAX_NOTE_FILE_SIZE_BYTES = 10 * 1024 * 1024;

export const SUPPORTED_NOTE_DOCUMENT_MIME_TYPES = [
  "application/pdf",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  "image/jpeg",
  "image/png",
];

// ---------------------------------------------------------------------------
// POST /notes  (multipart/form-data, field name `file`)  →  NoteSummaryDto
// GET  /notes?take=  →  NoteSummaryDto[]
// ---------------------------------------------------------------------------

export interface NoteSummaryDto {
  id: string;
  title: string;
  status: NoteStatus;
  createdAtUtc: string;
}

export interface UploadNoteFile {
  /** Local file URI (native) — ignored when `webFile` is present. */
  uri: string;
  name: string;
  mimeType: string;
  /**
   * Present only on web, where `expo-document-picker` / `expo-image-picker`
   * hand back a real `File` (see each asset's `.file` field). React Native's
   * `{ uri, name, type }` FormData convention has no meaning to a browser's
   * real `fetch`/`FormData`, so on web the actual `File` must be appended
   * instead — this is the documented way to upload a picked asset on web
   * per both packages' own type docs.
   */
  webFile?: File;
}

export async function uploadNote(file: UploadNoteFile): Promise<NoteSummaryDto> {
  const formData = new FormData();
  if (file.webFile) {
    formData.append("file", file.webFile, file.name);
  } else {
    // Not a real `Blob` — this object literal is React Native's own
    // documented FormData file-part convention, so the cast just satisfies
    // the DOM-lib-shaped `FormData.append` signature TypeScript sees.
    formData.append("file", { uri: file.uri, name: file.name, type: file.mimeType } as unknown as Blob);
  }

  const { data } = await coreApiClient.post<NoteSummaryDto>("/notes", formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}

/** `take` defaults to 20 server-side (`NotesController.GetNotes`) when omitted or `<= 0`. */
export async function getNotes(take?: number): Promise<NoteSummaryDto[]> {
  const { data } = await coreApiClient.get<NoteSummaryDto[]>("/notes", { params: { take } });
  return data;
}

// ---------------------------------------------------------------------------
// GET /notes/{id}  →  NoteDetailDto
// ---------------------------------------------------------------------------

export interface FlashcardDto {
  question: string;
  answer: string;
}

export interface McqDto {
  question: string;
  /** Always exactly 4 entries — the backend pads/truncates to 4 (`NoteAiResponseMapper.NormalizeOptions`). */
  options: string[];
  correctOptionIndex: number;
  explanation: string;
}

export interface MindMapNodeDto {
  topic: string;
  children: MindMapNodeDto[];
}

export interface VocabularyTermDto {
  term: string;
  definition: string;
}

export interface FormulaDto {
  name: string;
  formula: string;
  explanation: string;
}

/** The note's seven (well, eight-with-summary) pieces of AI-generated study content — `NoteContentDto` on the backend. */
export interface NoteContentDto {
  summary: string;
  keyPoints: string[];
  flashcards: FlashcardDto[];
  mcqs: McqDto[];
  mindMap: MindMapNodeDto;
  revisionSheet: string;
  vocabulary: VocabularyTermDto[];
  formulas: FormulaDto[];
}

/**
 * Full note record. `content` is `null` until (and unless) `status` is
 * `"ready"`; `errorMessage` is populated only when `status` is `"failed"`,
 * giving the UI something concrete to show instead of a generic message.
 */
export interface NoteDetailDto {
  id: string;
  title: string;
  sourceFileName: string;
  sourceFileType: NoteSourceFileType;
  status: NoteStatus;
  extractedText: string;
  errorMessage: string | null;
  createdAtUtc: string;
  content: NoteContentDto | null;
}

export async function getNote(noteId: string): Promise<NoteDetailDto> {
  const { data } = await coreApiClient.get<NoteDetailDto>(`/notes/${noteId}`);
  return data;
}

// ---------------------------------------------------------------------------
// DELETE /notes/{id}  →  204 No Content
// ---------------------------------------------------------------------------

export async function deleteNote(noteId: string): Promise<void> {
  await coreApiClient.delete<void>(`/notes/${noteId}`);
}

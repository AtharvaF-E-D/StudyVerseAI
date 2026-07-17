import type { BadgeVariant } from "../components/Badge";
import type { NoteStatus } from "../api/notes";

/** Shared status → display label / `Badge` variant mapping, used by both the notes list and detail screens. */
export const NOTE_STATUS_LABELS: Record<NoteStatus, string> = {
  processing: "Processing",
  ready: "Ready",
  failed: "Failed",
};

export const NOTE_STATUS_BADGE_VARIANTS: Record<NoteStatus, BadgeVariant> = {
  processing: "warning",
  ready: "success",
  failed: "danger",
};

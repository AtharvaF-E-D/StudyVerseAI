import type { BadgeVariant } from "../Badge";
import type { CodingDifficulty } from "../../api/codingpractice";

/** The wire value is lowercase (`"easy"`) per the backend's camelCase enum serialization — see `codingpractice.ts`'s header comment; this is purely for display. */
export const DIFFICULTY_LABELS: Record<CodingDifficulty, string> = {
  easy: "Easy",
  medium: "Medium",
  hard: "Hard",
};

export const DIFFICULTY_BADGE_VARIANT: Record<CodingDifficulty, BadgeVariant> = {
  easy: "success",
  medium: "warning",
  hard: "danger",
};

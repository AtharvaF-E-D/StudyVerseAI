import type { IconName } from "../Icon";
import type { InterviewCategory } from "../../api/interviewprep";

/** The wire value is lowercase (`"hr"`) per the backend's camelCase enum serialization — see `interviewprep.ts`'s header; this is purely for display. */
export const INTERVIEW_CATEGORY_LABELS: Record<InterviewCategory, string> = {
  hr: "HR",
  technical: "Technical",
  behavioral: "Behavioral",
};

export const INTERVIEW_CATEGORY_DESCRIPTIONS: Record<InterviewCategory, string> = {
  hr: "Company fit, motivation, and general career questions",
  technical: "Core concepts and problem-solving questions for your field",
  behavioral: "Past-experience questions answered using the STAR method",
};

export const INTERVIEW_CATEGORY_ICONS: Record<InterviewCategory, IconName> = {
  hr: "people-outline",
  technical: "hardware-chip-outline",
  behavioral: "chatbubbles-outline",
};

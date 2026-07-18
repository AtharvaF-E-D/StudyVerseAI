/**
 * Small "yyyy-MM-dd" date-string helpers for the Study Planner feature.
 * Every date the backend contract deals with (`examDate`, `weekStartDate`,
 * `scheduledDateUtc`'s date portion) is a plain calendar date rather than a
 * point in time, so these all operate on local-midnight `Date`s (same
 * anchoring trick `DashboardContent.tsx`'s `shortWeekdayLabel` uses) to
 * avoid the date shifting a day backward/forward depending on the reader's
 * timezone.
 */

function parseYmd(dateStr: string): Date {
  return new Date(`${dateStr}T00:00:00`);
}

function toYmd(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

/** Today's date as "yyyy-MM-dd", in the device's local timezone. */
export function todayYmd(): string {
  return toYmd(new Date());
}

/** Adds (or, with a negative count, subtracts) whole days to a "yyyy-MM-dd" string. */
export function shiftYmd(dateStr: string, days: number): string {
  const date = parseYmd(dateStr);
  date.setDate(date.getDate() + days);
  return toYmd(date);
}

/** True when `dateStr` ("yyyy-MM-dd") is the device's current local date. */
export function isToday(dateStr: string): boolean {
  return dateStr === todayYmd();
}

/** e.g. "Mon, Jul 21" — used for weekly-view day headings and the plan setup form's date field helper text. */
export function formatDayHeading(dateStr: string): string {
  return new Intl.DateTimeFormat("en-US", { weekday: "short", month: "short", day: "numeric" }).format(
    parseYmd(dateStr),
  );
}

/** e.g. "Jul 21, 2026" — used for the exam date summary on the plan overview screen. */
export function formatLongDate(dateStr: string): string {
  return new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric", year: "numeric" }).format(
    parseYmd(dateStr),
  );
}

/** e.g. "Jul 21 – Jul 27" — the weekly view's header range, spanning `weekStartDate` through 6 days later. */
export function formatWeekRangeLabel(weekStartDate: string): string {
  const start = new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric" }).format(
    parseYmd(weekStartDate),
  );
  const end = new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric" }).format(
    parseYmd(shiftYmd(weekStartDate, 6)),
  );
  return `${start} – ${end}`;
}

/** The 7 "yyyy-MM-dd" dates in the window starting at `weekStartDate`, in order. */
export function weekDates(weekStartDate: string): string[] {
  return Array.from({ length: 7 }, (_, i) => shiftYmd(weekStartDate, i));
}

/** Basic "yyyy-MM-dd" shape check for the plan setup form's exam-date field, which is a plain `TextField` (see that screen's header comment). */
export function isValidYmd(value: string): boolean {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return false;
  const date = parseYmd(value);
  return !Number.isNaN(date.getTime()) && toYmd(date) === value;
}

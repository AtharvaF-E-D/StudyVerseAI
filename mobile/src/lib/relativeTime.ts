const MINUTE = 60 * 1000;
const HOUR = 60 * MINUTE;
const DAY = 24 * HOUR;
const WEEK = 7 * DAY;

/**
 * Short "time ago" label for conversation list rows (e.g. "5m ago", "3h
 * ago", "2d ago"). Falls back to a short date once it's been over a week,
 * where a relative label stops being useful at a glance.
 */
export function formatRelativeTime(isoUtc: string, now: Date = new Date()): string {
  const then = new Date(isoUtc);
  const diffMs = now.getTime() - then.getTime();

  if (diffMs < MINUTE) return "just now";
  if (diffMs < HOUR) return `${Math.floor(diffMs / MINUTE)}m ago`;
  if (diffMs < DAY) return `${Math.floor(diffMs / HOUR)}h ago`;
  if (diffMs < WEEK) return `${Math.floor(diffMs / DAY)}d ago`;

  return new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric" }).format(then);
}

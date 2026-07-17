/**
 * Client-side time-of-day greeting for the dashboard header. Deliberately
 * computed from the device's local clock rather than the server — the
 * backend has no notion of the user's current wall-clock time/timezone, so
 * asking it for a greeting would either be wrong half the time or require a
 * timezone round-trip for zero benefit.
 */
export function getTimeBasedGreeting(date: Date = new Date()): string {
  const hour = date.getHours();
  if (hour < 12) return "Good morning";
  if (hour < 18) return "Good afternoon";
  return "Good evening";
}

function ordinalSuffix(n: number): string {
  const rem100 = n % 100;
  if (rem100 >= 11 && rem100 <= 13) return "th";
  switch (n % 10) {
    case 1:
      return "st";
    case 2:
      return "nd";
    case 3:
      return "rd";
    default:
      return "th";
  }
}

/** Formats a `percentileRank` (0-100, already a percentage) as e.g. "72nd percentile", for compact list rows. */
export function formatPercentileRank(percentileRank: number): string {
  const rounded = Math.round(percentileRank);
  return `${rounded}${ordinalSuffix(rounded)} percentile`;
}

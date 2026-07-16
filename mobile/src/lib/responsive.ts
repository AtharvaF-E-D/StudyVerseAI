import { useWindowDimensions } from "react-native";

export type Breakpoint = "phone" | "tablet";

/** Width, in dp, at and above which the layout switches to `"tablet"`. */
const TABLET_MIN_WIDTH = 768;

/**
 * Max width applied to primary screen content on wide viewports (tablet,
 * web) so text/forms don't stretch edge-to-edge — see `ScreenContainer`,
 * which centers its content within this width once the viewport exceeds it.
 */
export const MAX_CONTENT_WIDTH = 480;

/** Resolves to `"tablet"` at/above 768dp width, `"phone"` below it. */
export function useBreakpoint(): Breakpoint {
  const { width } = useWindowDimensions();
  return width >= TABLET_MIN_WIDTH ? "tablet" : "phone";
}

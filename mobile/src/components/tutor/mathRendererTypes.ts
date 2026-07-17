/** One `$...$` or `$$...$$` expression to typeset. */
export interface MathSegmentInput {
  id: string;
  tex: string;
  /** `true` for `$$block$$` math (centered, larger); `false` for `$inline$` math. */
  displayMode: boolean;
}

export interface MathRendererProps {
  segments: MathSegmentInput[];
  /** Themed ink color so rendered glyphs match the surrounding bubble's text color in both light/dark mode. */
  textColor: string;
}

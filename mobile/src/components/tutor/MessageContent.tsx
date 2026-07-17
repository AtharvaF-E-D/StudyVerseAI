import React from "react";
import { Text, View } from "react-native";

import { CodeBlock } from "./CodeBlock";
import { MathRenderer } from "./MathRenderer";
import type { MathSegmentInput } from "./mathRendererTypes";
import { parseMessageContent } from "./messageParsing";

export interface MessageContentProps {
  content: string;
  /** Hex color matching the bubble's text so KaTeX-rendered glyphs (drawn inside a WebView/DOM node, not a themed `Text`) still match. */
  textColor: string;
  /** NativeWind classes applied to every plain-text run; defaults to the standard themed body text. */
  textClassName?: string;
}

/**
 * Renders a chat message's text, routing each parsed segment (see
 * `messageParsing.ts`) to the right renderer: plain runs of text as themed
 * `Text`, fenced code blocks through `CodeBlock`
 * (react-native-syntax-highlighter), and `$inline$` / `$$block$$` LaTeX math
 * through `MathRenderer` (locally bundled, offline KaTeX).
 *
 * Segments render in reading order as stacked blocks. This is a deliberate
 * simplification: a native WebView (and, on web, the DOM node `MathRenderer`
 * mounts into) is a single rectangular child in the view tree, so it can't
 * literally flow mid-sentence between words the way inline math does on the
 * web page you're reading right now. Block vs. inline math is still
 * rendered distinctly (KaTeX `displayMode` + sizing), just each on its own
 * line rather than interleaved with surrounding words — e.g. "The quadratic
 * formula is" / [rendered formula] as two stacked lines instead of one. A
 * custom rich-text layout engine could achieve true inline flow but was out
 * of scope for this pass.
 *
 * All of a message's math segments that fall between the same two
 * non-math segments share one `MathRenderer` (one WebView/DOM mount) rather
 * than allocating one per formula — see `MathRenderer.tsx` for why that's
 * the "single WebView per message" the spec asks for in the common case.
 */
export function MessageContent({
  content,
  textColor,
  textClassName = "text-body text-ink-primary dark:text-ink-primary-dark",
}: MessageContentProps) {
  const segments = parseMessageContent(content);

  if (segments.length === 0) return null;

  const blocks: React.ReactNode[] = [];
  let mathRun: MathSegmentInput[] = [];
  let key = 0;

  function flushMathRun() {
    if (mathRun.length === 0) return;
    const run = mathRun;
    mathRun = [];
    blocks.push(<MathRenderer key={`math-${key++}`} segments={run} textColor={textColor} />);
  }

  for (const segment of segments) {
    if (segment.type === "text") {
      flushMathRun();
      blocks.push(
        <Text key={`text-${key++}`} className={textClassName}>
          {segment.text}
        </Text>,
      );
    } else if (segment.type === "code") {
      flushMathRun();
      blocks.push(<CodeBlock key={`code-${key++}`} language={segment.language} code={segment.code} />);
    } else {
      mathRun.push({
        id: `${key++}`,
        tex: segment.tex,
        displayMode: segment.type === "block-math",
      });
    }
  }
  flushMathRun();

  return <View>{blocks}</View>;
}

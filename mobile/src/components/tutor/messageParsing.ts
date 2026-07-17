// ---------------------------------------------------------------------------
// Lightweight, targeted parser for exactly two constructs the tutor backend's
// system prompt is instructed to use: `$inline$` / `$$block$$` LaTeX math and
// fenced ```lang code blocks. This is intentionally NOT a general markdown
// parser (no lists/headings/bold/links) — just enough structure to route
// each run of text to the right renderer in `MessageContent`.
// ---------------------------------------------------------------------------

export type MessageSegment =
  | { type: "text"; text: string }
  | { type: "inline-math"; tex: string }
  | { type: "block-math"; tex: string }
  | { type: "code"; language: string; code: string };

const CODE_BLOCK_REGEX = /```([a-zA-Z0-9_+-]*)\n?([\s\S]*?)```/g;
const BLOCK_MATH_REGEX = /\$\$([\s\S]+?)\$\$/g;
// Requires non-whitespace immediately inside both `$` delimiters and forbids
// newlines, which keeps this from misfiring on stray currency signs like
// "$5 and $10" (no closing `$` immediately follows a digit-space run there)
// while still matching real inline math such as `$x = 1$`.
const INLINE_MATH_REGEX = /\$(?!\s)([^$\n]+?)(?<!\s)\$/g;

function parseInlineMath(text: string): MessageSegment[] {
  const segments: MessageSegment[] = [];
  let lastIndex = 0;
  INLINE_MATH_REGEX.lastIndex = 0;
  let match: RegExpExecArray | null;
  while ((match = INLINE_MATH_REGEX.exec(text)) !== null) {
    if (match.index > lastIndex) {
      segments.push({ type: "text", text: text.slice(lastIndex, match.index) });
    }
    segments.push({ type: "inline-math", tex: match[1].trim() });
    lastIndex = match.index + match[0].length;
  }
  if (lastIndex < text.length) {
    segments.push({ type: "text", text: text.slice(lastIndex) });
  }
  return segments;
}

function parseBlockMath(text: string): MessageSegment[] {
  const segments: MessageSegment[] = [];
  let lastIndex = 0;
  BLOCK_MATH_REGEX.lastIndex = 0;
  let match: RegExpExecArray | null;
  while ((match = BLOCK_MATH_REGEX.exec(text)) !== null) {
    if (match.index > lastIndex) {
      segments.push(...parseInlineMath(text.slice(lastIndex, match.index)));
    }
    segments.push({ type: "block-math", tex: match[1].trim() });
    lastIndex = match.index + match[0].length;
  }
  if (lastIndex < text.length) {
    segments.push(...parseInlineMath(text.slice(lastIndex)));
  }
  return segments;
}

/**
 * Splits raw assistant/user message text into an ordered list of segments:
 * plain text, inline math, block math, and fenced code. Code fences are
 * split out first (a top-level pass over the whole string) so `$`/`$$`
 * characters that happen to appear inside code samples are never mistaken
 * for math delimiters.
 */
export function parseMessageContent(content: string): MessageSegment[] {
  const segments: MessageSegment[] = [];
  let lastIndex = 0;
  CODE_BLOCK_REGEX.lastIndex = 0;
  let match: RegExpExecArray | null;
  while ((match = CODE_BLOCK_REGEX.exec(content)) !== null) {
    if (match.index > lastIndex) {
      segments.push(...parseBlockMath(content.slice(lastIndex, match.index)));
    }
    segments.push({
      type: "code",
      language: match[1] || "text",
      code: match[2].replace(/\n$/, ""),
    });
    lastIndex = match.index + match[0].length;
  }
  if (lastIndex < content.length) {
    segments.push(...parseBlockMath(content.slice(lastIndex)));
  }
  // Drop segments that are pure whitespace text producing no visible content
  // (an artifact of stripping delimiters right next to newlines), but keep
  // deliberate single spaces between adjacent inline segments.
  return segments.filter((segment) => segment.type !== "text" || segment.text.length > 0);
}

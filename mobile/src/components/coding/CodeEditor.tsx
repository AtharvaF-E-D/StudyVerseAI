import React, { useMemo } from "react";
import { Platform, Text, TextInput, View } from "react-native";

import { useTheme } from "../../theme/ThemeProvider";

export interface CodeEditorProps {
  value: string;
  onChangeText: (text: string) => void;
  editable?: boolean;
  /** Shown when `value` is empty, same convention as `TextField`'s placeholder. */
  placeholder?: string;
  /** Renders a left-hand line-number gutter. Defaults on; set `false` for a plainer, denser layout. */
  showLineNumbers?: boolean;
  minHeight?: number;
  className?: string;
}

// ---------------------------------------------------------------------------
// EDITOR APPROACH (documented per the phase brief): this is the plain
// `TextInput` fallback, not a real syntax-highlighting-while-editing library.
//
// Two actively-maintained-looking candidates were checked before falling
// back — `@rivascva/react-native-code-editor` and the unscoped
// `react-native-code-editor` — but both are effectively abandoned: their
// last published version dates to May 2022 (`npm view <pkg> time.modified`),
// four-plus years before this app's RN 0.86 / React 19 / Expo 57 / New
// Architecture stack existed, with no compatibility signal for any of that.
// Sinking time into integrating (and then debugging) an unmaintained
// package against a stack it predates by years — inside a time-boxed phase
// whose priority order explicitly puts a working editor above AI hints or
// the daily-challenge entry point — was judged a worse trade than this
// fallback, which the brief itself calls out as acceptable. This still gives
// a fully functional editable multi-line code area (needed for the real
// write -> submit -> Judge0-graded-result loop); it only gives up live
// coloring while typing (existing read-only `CodeBlock.tsx` still highlights
// the *starter*/reference code shown elsewhere).
//
// Known limitation of the optional line-number gutter below: numbers are
// derived from logical lines (`value.split("\n")`), not visual/wrapped
// lines. Once a single logical line grows long enough to soft-wrap inside
// the input, the gutter will drift out of vertical sync with that line
// (and every line after it) until the next logical newline resets it. This
// is judged an acceptable best-effort for typical short lines of code
// rather than a hard requirement — a real fix would mean disabling soft-wrap
// entirely (not reliably possible via RN `TextInput` props alone) or
// pulling in a real editor component, which is exactly the tradeoff this
// fallback was chosen to avoid.
// ---------------------------------------------------------------------------

/** Exported so other coding-practice components (e.g. `TestResultsPanel`'s input/output blocks) render code-ish text in the same monospace stack instead of duplicating this platform switch. */
export const MONOSPACE_FONT_FAMILY = Platform.select({
  ios: "Menlo",
  android: "monospace",
  default: "Menlo, Consolas, 'Courier New', monospace",
});

const FONT_SIZE = 14;
const LINE_HEIGHT = 20;
const VERTICAL_PADDING = 10;
const HORIZONTAL_PADDING = 12;

export function CodeEditor({
  value,
  onChangeText,
  editable = true,
  placeholder,
  showLineNumbers = true,
  minHeight = 280,
  className = "",
}: CodeEditorProps) {
  const { scheme, colors } = useTheme();
  const isDark = scheme === "dark";

  const lineCount = useMemo(() => Math.max(1, value.length === 0 ? 1 : value.split("\n").length), [value]);
  const lineNumbers = useMemo(() => Array.from({ length: lineCount }, (_, i) => i + 1), [lineCount]);

  const editorBackground = isDark ? "#1E1E1E" : "#F8F8F8";
  const gutterBackground = isDark ? "#181818" : "#EFEFF2";
  const codeColor = isDark ? "#F3F4F6" : "#12141A";
  const gutterColor = isDark ? "#5C6370" : "#9AA1B1";

  return (
    <View
      accessibilityLabel="Code editor"
      className={["overflow-hidden rounded-md border border-border dark:border-border-dark", className].join(" ")}
      style={{ backgroundColor: editorBackground }}
    >
      <View className="flex-row" style={{ minHeight }}>
        {showLineNumbers ? (
          <View
            style={{
              backgroundColor: gutterBackground,
              paddingTop: VERTICAL_PADDING,
              paddingBottom: VERTICAL_PADDING,
              paddingHorizontal: 10,
              alignItems: "flex-end",
            }}
            className="border-r border-border dark:border-border-dark"
          >
            {lineNumbers.map((n) => (
              <Text
                key={n}
                style={{
                  fontFamily: MONOSPACE_FONT_FAMILY,
                  fontSize: FONT_SIZE,
                  lineHeight: LINE_HEIGHT,
                  color: gutterColor,
                }}
              >
                {n}
              </Text>
            ))}
          </View>
        ) : null}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          editable={editable}
          placeholder={placeholder}
          placeholderTextColor={colors.textSecondary}
          multiline
          autoCapitalize="none"
          autoCorrect={false}
          autoComplete="off"
          spellCheck={false}
          textAlignVertical="top"
          accessibilityLabel="Code editor input"
          className="flex-1"
          style={{
            fontFamily: MONOSPACE_FONT_FAMILY,
            fontSize: FONT_SIZE,
            lineHeight: LINE_HEIGHT,
            color: codeColor,
            paddingVertical: VERTICAL_PADDING,
            paddingHorizontal: HORIZONTAL_PADDING,
          }}
        />
      </View>
    </View>
  );
}

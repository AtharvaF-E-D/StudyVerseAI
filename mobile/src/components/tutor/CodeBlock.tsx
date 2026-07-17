import React from "react";
import { ScrollView, Text, View } from "react-native";
import SyntaxHighlighter from "react-native-syntax-highlighter";
import { atomOneDark, docco } from "react-syntax-highlighter/styles/hljs";

import { useTheme } from "../../theme/ThemeProvider";

export interface CodeBlockProps {
  language: string;
  code: string;
}

/**
 * Fenced-code-block renderer for tutor chat messages. `react-native-syntax-highlighter`
 * defaults its internal `PreTag`/`CodeTag` to `ScrollView` with `horizontal`
 * passed straight through — on react-native-web that unrecognized prop
 * leaks onto the underlying `<div>` as a raw DOM attribute and trips a React
 * warning (cosmetic only; harmless on native). Passing plain `View`s instead
 * and wrapping the whole highlighter in our own single `ScrollView`
 * sidesteps that, while still giving horizontal scrolling for long lines.
 */
export function CodeBlock({ language, code }: CodeBlockProps) {
  const { scheme } = useTheme();
  const isDark = scheme === "dark";

  return (
    <View
      className="my-1.5 overflow-hidden rounded-md border border-border dark:border-border-dark"
      style={{ backgroundColor: isDark ? "#1E1E1E" : "#F8F8F8" }}
    >
      {language && language !== "text" ? (
        <View className="border-b border-border px-3 py-1 dark:border-border-dark">
          <Text className="text-caption font-medium text-ink-secondary dark:text-ink-secondary-dark">
            {language}
          </Text>
        </View>
      ) : null}
      <ScrollView horizontal showsHorizontalScrollIndicator={false} className="px-3 py-2">
        <SyntaxHighlighter
          language={language}
          style={isDark ? atomOneDark : docco}
          highlighter="hljs"
          fontSize={13}
          PreTag={View}
          CodeTag={View}
          customStyle={{ backgroundColor: "transparent", padding: 0, margin: 0 }}
        >
          {code}
        </SyntaxHighlighter>
      </ScrollView>
    </View>
  );
}

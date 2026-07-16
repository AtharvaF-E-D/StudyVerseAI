import React from "react";
import { KeyboardAvoidingView, Platform, ScrollView, View } from "react-native";
import { SafeAreaView, type Edge } from "react-native-safe-area-context";

import { MAX_CONTENT_WIDTH } from "../lib/responsive";

// Constrains content to a centered column on wide viewports (tablet/web)
// so forms/text don't stretch edge-to-edge; a no-op on phone widths, where
// the viewport is already narrower than `MAX_CONTENT_WIDTH`.
const centeredContentStyle = { width: "100%" as const, maxWidth: MAX_CONTENT_WIDTH, alignSelf: "center" as const };

export interface ScreenContainerProps {
  children: React.ReactNode;
  /** Wraps content in a ScrollView with keyboard-avoidance, for forms. Defaults to true. */
  scrollable?: boolean;
  className?: string;
  contentClassName?: string;
  edges?: Edge[];
}

/**
 * Theme-aware safe-area screen wrapper shared by every auth/app screen.
 * Handles keyboard avoidance on iOS (padding) and Android (height) so form
 * fields near the bottom of the screen aren't hidden by the keyboard.
 */
export function ScreenContainer({
  children,
  scrollable = true,
  className = "",
  contentClassName = "",
  edges = ["top", "bottom", "left", "right"],
}: ScreenContainerProps) {
  const content = scrollable ? (
    <ScrollView
      className={contentClassName}
      contentContainerClassName="flex-grow px-6 py-6"
      keyboardShouldPersistTaps="handled"
      showsVerticalScrollIndicator={false}
    >
      <View style={centeredContentStyle} className="flex-1">
        {children}
      </View>
    </ScrollView>
  ) : (
    <View className={["flex-1 px-6 py-6", contentClassName].join(" ")}>
      <View style={centeredContentStyle} className="flex-1">
        {children}
      </View>
    </View>
  );

  return (
    <SafeAreaView
      edges={edges}
      className={["flex-1 bg-background dark:bg-background-dark", className].join(" ")}
    >
      <KeyboardAvoidingView
        className="flex-1"
        behavior={Platform.OS === "ios" ? "padding" : "height"}
        keyboardVerticalOffset={Platform.OS === "ios" ? 0 : 0}
      >
        {content}
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

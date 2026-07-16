import React from "react";
import { View, type ViewProps } from "react-native";

export interface DividerProps extends ViewProps {
  className?: string;
}

/**
 * Simple themed horizontal rule. Defaults to full width; pass e.g.
 * `className="flex-1"` on a wrapping `View` (not on the Divider itself) if
 * you need it to share a row with other content — see the "or continue
 * with" divider row in `app/(auth)/login.tsx` for the pattern.
 */
export function Divider({ className = "", ...viewProps }: DividerProps) {
  return (
    <View
      accessibilityElementsHidden
      importantForAccessibility="no-hide-descendants"
      className={["h-px w-full bg-border dark:bg-border-dark", className].join(" ")}
      {...viewProps}
    />
  );
}

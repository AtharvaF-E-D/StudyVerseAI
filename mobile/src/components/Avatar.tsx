import React from "react";
import { Image, Text, View, type ImageSourcePropType } from "react-native";

export type AvatarSize = "sm" | "md" | "lg";

export interface AvatarProps {
  /** Used both to render the image's accessibility label and to derive initials when no `source` is given. */
  name?: string;
  source?: ImageSourcePropType;
  size?: AvatarSize;
  className?: string;
}

const sizeClasses: Record<AvatarSize, string> = {
  sm: "h-8 w-8",
  md: "h-12 w-12",
  lg: "h-16 w-16",
};

// Mirrors `sizeClasses` in raw px. RN's `Image` (specifically react-native-web)
// defaults its rendered size to the source asset's own intrinsic dimensions
// via an inline `style` attribute whenever no explicit numeric width/height
// style is given — and an inline style always wins over the `h-*`/`w-*`
// class above, regardless of specificity. Passing this as `style` alongside
// the className is what actually constrains the image on web; on native the
// className-derived style already wins, so this is a harmless no-op there.
const sizePx: Record<AvatarSize, number> = {
  sm: 32,
  md: 48,
  lg: 64,
};

const initialsFontClasses: Record<AvatarSize, string> = {
  sm: "text-caption",
  md: "text-body",
  lg: "text-heading",
};

function getInitials(name: string): string {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length === 0) return "?";
  if (parts.length === 1) return parts[0]!.slice(0, 2).toUpperCase();
  return (parts[0]![0] + parts[parts.length - 1]![0]).toUpperCase();
}

/**
 * Circular avatar. Renders `source` when given; otherwise derives a
 * one- or two-letter initials fallback from `name` on a brand-tinted
 * background (e.g. a user with no profile photo yet).
 */
export function Avatar({ name, source, size = "md", className = "" }: AvatarProps) {
  if (source) {
    return (
      <Image
        source={source}
        accessibilityIgnoresInvertColors
        accessibilityLabel={name ? `${name}'s avatar` : "Avatar"}
        className={["rounded-full bg-surface dark:bg-surface-dark", sizeClasses[size], className].join(" ")}
        style={{ width: sizePx[size], height: sizePx[size] }}
      />
    );
  }

  return (
    <View
      accessibilityLabel={name ? `${name}'s avatar` : "Avatar"}
      className={[
        "items-center justify-center rounded-full bg-brand/15 dark:bg-brand-light/20",
        sizeClasses[size],
        className,
      ].join(" ")}
    >
      <Text className={["font-semibold text-brand dark:text-brand-light", initialsFontClasses[size]].join(" ")}>
        {name?.trim() ? getInitials(name) : "?"}
      </Text>
    </View>
  );
}

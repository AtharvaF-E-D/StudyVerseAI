import React, { useState } from "react";
import { Image, View } from "react-native";

import { Icon } from "../Icon";
import { useTheme } from "../../theme/ThemeProvider";

export interface ArticleThumbnailProps {
  imageUrl: string | null;
  size?: number;
  className?: string;
}

/**
 * Small rounded thumbnail for an article row. Falls back to a neutral
 * placeholder icon both when `imageUrl` is `null` (the contract marks it
 * nullable) and when the URL 404s or otherwise fails to load at runtime —
 * `Image`'s `onError` is the only way to know that after the fact, so the
 * fallback is tracked as local state rather than derived purely from props.
 */
export function ArticleThumbnail({ imageUrl, size = 56, className = "" }: ArticleThumbnailProps) {
  const { colors } = useTheme();
  const [failed, setFailed] = useState(false);
  const showPlaceholder = !imageUrl || failed;

  if (showPlaceholder) {
    return (
      <View
        style={{ width: size, height: size }}
        className={["items-center justify-center rounded-lg bg-surface dark:bg-surface-dark", className].join(" ")}
      >
        <Icon name="newspaper-outline" size={Math.round(size * 0.45)} color={colors.textSecondary} />
      </View>
    );
  }

  return (
    <Image
      source={{ uri: imageUrl }}
      accessibilityIgnoresInvertColors
      accessibilityLabel="Article thumbnail"
      onError={() => setFailed(true)}
      style={{ width: size, height: size }}
      className={["rounded-lg bg-surface dark:bg-surface-dark", className].join(" ")}
    />
  );
}

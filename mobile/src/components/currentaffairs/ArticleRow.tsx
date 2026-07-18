import React from "react";
import { Pressable, View } from "react-native";

import { ArticleThumbnail } from "./ArticleThumbnail";
import { Icon } from "../Icon";
import { ListItem } from "../ListItem";
import { useTheme } from "../../theme/ThemeProvider";
import { formatRelativeTime } from "../../lib/relativeTime";
import type { ArticleSummaryDto } from "../../api/currentaffairs";

export interface ArticleRowProps {
  article: ArticleSummaryDto;
  /**
   * Whether `article` is currently bookmarked. Passed explicitly rather than
   * read off `article` itself — the real `NewsArticleDto` has no
   * `isBookmarked` field on any endpoint (confirmed against the backend
   * source), so callers derive this from `useBookmarkedArticleIds()` (feed/
   * search/detail) or know it's always `true` (the bookmarks screen itself).
   */
  isBookmarked: boolean;
  onPress: (article: ArticleSummaryDto) => void;
  onToggleBookmark: (article: ArticleSummaryDto) => void;
  bookmarkPending?: boolean;
  className?: string;
}

/**
 * One article row shared by the news feed, search results, and bookmarks
 * screens: thumbnail + title + source/relative-time, with a bookmark star.
 *
 * The star is a sibling `Pressable` next to `ListItem`, not passed into its
 * `trailing` slot — `ListItem` renders as a `<button>` on web whenever
 * `onPress` is set, and nesting another pressable inside it produces invalid
 * nested `<button>` HTML (a bug already fixed multiple times in this
 * codebase; see `app/(app)/notes/index.tsx`'s delete button for the same
 * pattern this mirrors).
 */
export function ArticleRow({
  article,
  isBookmarked,
  onPress,
  onToggleBookmark,
  bookmarkPending = false,
  className = "",
}: ArticleRowProps) {
  const { colors } = useTheme();

  return (
    <View className={["flex-row items-center", className].join(" ")}>
      <ListItem
        leading={<ArticleThumbnail imageUrl={article.imageUrl} />}
        title={article.title}
        subtitle={`${article.sourceName} · ${formatRelativeTime(article.publishedAtUtc)}`}
        onPress={() => onPress(article)}
        className="flex-1"
      />
      <Pressable
        onPress={() => onToggleBookmark(article)}
        disabled={bookmarkPending}
        hitSlop={8}
        accessibilityRole="button"
        accessibilityLabel={isBookmarked ? `Remove ${article.title} from bookmarks` : `Bookmark ${article.title}`}
        accessibilityState={{ selected: isBookmarked, disabled: bookmarkPending }}
        className={["ml-1 mr-3", bookmarkPending ? "opacity-40" : ""].join(" ")}
      >
        <Icon
          name={isBookmarked ? "star" : "star-outline"}
          size={22}
          color={isBookmarked ? colors.warning : colors.textSecondary}
        />
      </Pressable>
    </View>
  );
}

import React, { useState } from "react";
import { Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { ArticleRow } from "../../../src/components/currentaffairs/ArticleRow";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { ArticleSummaryDto } from "../../../src/api/currentaffairs";
import { useBookmarksQuery, useToggleBookmarkMutation } from "../../../src/hooks/useCurrentAffairs";

function BookmarksSkeleton() {
  return (
    <Card>
      {[0, 1, 2].map((i) => (
        <View key={i} className="flex-row items-center px-3 py-3">
          <Skeleton variant="rect" width={56} height={56} className="mr-3 rounded-lg" />
          <View className="flex-1">
            <Skeleton variant="text" width="85%" className="mb-2" />
            <Skeleton variant="text" width="45%" />
          </View>
        </View>
      ))}
    </Card>
  );
}

/** Bookmarked-articles screen, reached from the star icon on the news feed. Same article-row rendering as the feed/search, filtered to what the signed-in user has bookmarked. */
export default function CurrentAffairsBookmarksScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [bookmarkingId, setBookmarkingId] = useState<string | null>(null);

  const bookmarksQuery = useBookmarksQuery();
  const toggleBookmarkMutation = useToggleBookmarkMutation();

  function openArticle(article: ArticleSummaryDto) {
    router.push(`/(app)/currentaffairs/${article.id}`);
  }

  function handleToggleBookmark(article: ArticleSummaryDto) {
    if (toggleBookmarkMutation.isPending) return;
    setBookmarkingId(article.id);
    toggleBookmarkMutation.mutate(article.id, {
      onSuccess: () => setBookmarkingId(null),
      onError: () => {
        setBookmarkingId(null);
        show("Couldn't update that bookmark. Please try again.", "danger");
      },
    });
  }

  const bookmarks = bookmarksQuery.data ?? [];

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center">
        <Pressable
          onPress={() => router.back()}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back"
          className="mr-3 h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Bookmarks</Text>
      </View>

      {bookmarksQuery.isLoading ? (
        <BookmarksSkeleton />
      ) : bookmarksQuery.isError ? (
        <ErrorState
          title="Couldn't load your bookmarks"
          description="Check your connection and try again."
          onRetry={() => void bookmarksQuery.refetch()}
        />
      ) : bookmarks.length === 0 ? (
        <EmptyState
          icon="star-outline"
          title="No bookmarks yet"
          description="Tap the star on any article to save it here for later."
        />
      ) : (
        <Card>
          {bookmarks.map((article, index) => (
            <React.Fragment key={article.id}>
              {index > 0 ? <Divider /> : null}
              <ArticleRow
                article={article}
                // Every row here comes straight from `GET /bookmarks`, so it's
                // bookmarked by definition — no separate id cross-reference needed.
                isBookmarked
                onPress={openArticle}
                onToggleBookmark={handleToggleBookmark}
                bookmarkPending={bookmarkingId === article.id}
              />
            </React.Fragment>
          ))}
        </Card>
      )}
    </ScreenContainer>
  );
}

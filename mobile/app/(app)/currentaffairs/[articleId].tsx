import React, { useState } from "react";
import { Image, Linking, Pressable, Text, View } from "react-native";
import { router, useLocalSearchParams } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Button } from "../../../src/components/Button";
import { Divider } from "../../../src/components/Divider";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import {
  useArticleQuery,
  useBookmarkedArticleIds,
  useToggleBookmarkMutation,
} from "../../../src/hooks/useCurrentAffairs";

const HERO_HEIGHT = 200;

function ArticleHeroImage({ imageUrl }: { imageUrl: string | null }) {
  const { colors } = useTheme();
  const [failed, setFailed] = useState(false);
  const showPlaceholder = !imageUrl || failed;

  if (showPlaceholder) {
    return (
      <View
        style={{ height: HERO_HEIGHT }}
        className="mb-4 items-center justify-center rounded-xl bg-surface dark:bg-surface-dark"
      >
        <Icon name="newspaper-outline" size={40} color={colors.textSecondary} />
      </View>
    );
  }

  return (
    <Image
      source={{ uri: imageUrl }}
      accessibilityIgnoresInvertColors
      accessibilityLabel="Article image"
      onError={() => setFailed(true)}
      style={{ width: "100%", height: HERO_HEIGHT }}
      className="mb-4 rounded-xl bg-surface dark:bg-surface-dark"
      resizeMode="cover"
    />
  );
}

function DetailSkeleton() {
  return (
    <View>
      <Skeleton variant="rect" width="100%" height={HERO_HEIGHT} className="mb-4 rounded-xl" />
      <Skeleton variant="text" width="90%" className="mb-2" />
      <Skeleton variant="text" width="70%" className="mb-4" />
      <Skeleton variant="text" width="100%" className="mb-2" />
      <Skeleton variant="text" width="100%" className="mb-2" />
      <Skeleton variant="text" width="60%" />
    </View>
  );
}

function formatFullDate(isoUtc: string): string {
  return new Intl.DateTimeFormat("en-US", { month: "long", day: "numeric", year: "numeric" }).format(
    new Date(isoUtc),
  );
}

/**
 * Full article view: hero image, title, source/date, full body content, a
 * bookmark toggle, and a "Test your understanding" entry into the
 * per-article AI quiz (`[articleId]/quiz.tsx`).
 */
export default function ArticleDetailScreen() {
  const params = useLocalSearchParams<{ articleId: string }>();
  const articleId = params.articleId ?? "";

  const { colors } = useTheme();
  const { show } = useToast();

  const articleQuery = useArticleQuery(articleId);
  const toggleBookmarkMutation = useToggleBookmarkMutation();
  // The real `NewsArticleDto` has no `isBookmarked` field (confirmed against
  // the backend source) — cross-referenced against the bookmarks list instead.
  const bookmarkedIds = useBookmarkedArticleIds();
  const isBookmarked = bookmarkedIds.has(articleId);

  function handleToggleBookmark() {
    if (toggleBookmarkMutation.isPending) return;
    toggleBookmarkMutation.mutate(articleId, {
      onError: () => show("Couldn't update that bookmark. Please try again.", "danger"),
    });
  }

  function handleOpenSource(url: string) {
    void Linking.openURL(url).catch(() => show("Couldn't open that link.", "danger"));
  }

  const article = articleQuery.data;

  return (
    <ScreenContainer>
      <View className="mb-4 flex-row items-center justify-between">
        <Pressable
          onPress={() => router.back()}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="Back"
          className="h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
        >
          <Icon name="chevron-back" size={22} color={colors.textPrimary} />
        </Pressable>
        {article ? (
          <Pressable
            onPress={handleToggleBookmark}
            disabled={toggleBookmarkMutation.isPending}
            hitSlop={8}
            accessibilityRole="button"
            accessibilityLabel={isBookmarked ? "Remove bookmark" : "Bookmark this article"}
            accessibilityState={{ selected: isBookmarked, disabled: toggleBookmarkMutation.isPending }}
            className="h-9 w-9 items-center justify-center rounded-full active:bg-surface dark:active:bg-surface-dark"
          >
            <Icon
              name={isBookmarked ? "star" : "star-outline"}
              size={22}
              color={isBookmarked ? colors.warning : colors.textPrimary}
            />
          </Pressable>
        ) : (
          <View className="h-9 w-9" />
        )}
      </View>

      {articleQuery.isLoading ? (
        <DetailSkeleton />
      ) : articleQuery.isError || !article ? (
        <ErrorState
          title="Couldn't load this article"
          description="Check your connection and try again."
          onRetry={() => void articleQuery.refetch()}
        />
      ) : (
        <View>
          <ArticleHeroImage imageUrl={article.imageUrl} />

          <Text className="mb-2 text-heading text-ink-primary dark:text-ink-primary-dark">{article.title}</Text>
          <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            {article.sourceName} · {formatFullDate(article.publishedAtUtc)}
          </Text>

          <Divider className="mb-4" />

          <Text className="mb-6 text-body text-ink-primary dark:text-ink-primary-dark">{article.content}</Text>

          <View className="mb-3">
            <Button
              title="Test your understanding"
              onPress={() => router.push(`/(app)/currentaffairs/${articleId}/quiz`)}
            />
          </View>
          <Button title="View original source" variant="secondary" onPress={() => handleOpenSource(article.url)} />
        </View>
      )}
    </ScreenContainer>
  );
}

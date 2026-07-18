import React, { useEffect, useState } from "react";
import { Pressable, Text, View } from "react-native";
import { router } from "expo-router";

import { ScreenContainer } from "../../../src/components/ScreenContainer";
import { Card } from "../../../src/components/Card";
import { Divider } from "../../../src/components/Divider";
import { EmptyState } from "../../../src/components/EmptyState";
import { ErrorState } from "../../../src/components/ErrorState";
import { Icon } from "../../../src/components/Icon";
import { Skeleton } from "../../../src/components/Skeleton";
import { TextField } from "../../../src/components/TextField";
import { ArticleRow } from "../../../src/components/currentaffairs/ArticleRow";
import { CategoryChipRow } from "../../../src/components/currentaffairs/CategoryChipRow";
import { useTheme } from "../../../src/theme/ThemeProvider";
import { useToast } from "../../../src/lib/toast";
import type { ArticleSummaryDto } from "../../../src/api/currentaffairs";
import {
  useArticlesByCategoryQuery,
  useBookmarkedArticleIds,
  useNewsCategoriesQuery,
  useSearchArticlesQuery,
  useToggleBookmarkMutation,
  useWeeklyDigestQuery,
} from "../../../src/hooks/useCurrentAffairs";

/** How long to wait after the last keystroke before firing a search request. */
const SEARCH_DEBOUNCE_MS = 400;

function ArticleListSkeleton() {
  return (
    <Card>
      {[0, 1, 2, 3].map((i) => (
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

function DigestTeaserSkeleton() {
  return (
    <Card className="mb-6">
      <Skeleton variant="text" width="50%" className="mb-2" />
      <Skeleton variant="text" width="90%" className="mb-1" />
      <Skeleton variant="text" width="70%" />
    </Card>
  );
}

function formatWeekStart(isoDate: string): string {
  return new Intl.DateTimeFormat("en-US", { month: "short", day: "numeric" }).format(new Date(isoDate));
}

/**
 * Weekly digest teaser shown above the feed. Renders an honest "not enough
 * data yet" state rather than hiding the section outright when the backend
 * has nothing to summarize — a failed fetch is degraded silently instead
 * (returns `null`), since a broken digest teaser shouldn't block the
 * category/feed content underneath, which matters more.
 */
function WeeklyDigestTeaser() {
  const { colors } = useTheme();
  const digestQuery = useWeeklyDigestQuery();

  if (digestQuery.isLoading) return <DigestTeaserSkeleton />;
  if (digestQuery.isError) return null;

  const digest = digestQuery.data;

  if (!digest) {
    return (
      <Card className="mb-6">
        <View className="flex-row items-center">
          <Icon name="calendar-outline" size={20} color={colors.textSecondary} />
          <Text className="ml-2 flex-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
            Check back once there&apos;s more news this week for your AI weekly digest.
          </Text>
        </View>
      </Card>
    );
  }

  return (
    <Pressable
      onPress={() => router.push("/(app)/currentaffairs/digest")}
      accessibilityRole="button"
      accessibilityLabel="View weekly digest"
      className="mb-6 active:opacity-80"
    >
      <Card className="border-brand dark:border-brand-light">
        <View className="mb-1 flex-row items-center justify-between">
          <View className="flex-row items-center">
            <Icon name="sparkles" size={18} color={colors.brand} />
            <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Weekly Digest</Text>
          </View>
          <Icon name="chevron-forward" size={18} color={colors.textSecondary} />
        </View>
        <Text className="mb-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          Week of {formatWeekStart(digest.weekStartDateUtc)}
        </Text>
        <Text numberOfLines={2} className="text-body text-ink-primary dark:text-ink-primary-dark">
          {digest.summaryText}
        </Text>
      </Card>
    </Pressable>
  );
}

/**
 * News feed — the "Current Affairs" entry point reached from the dashboard.
 * Shows a weekly-digest teaser, a horizontally-scrolling category selector,
 * and the article feed for the selected category; typing in the search
 * field switches the whole feed area to (debounced) search results instead.
 */
export default function CurrentAffairsFeedScreen() {
  const { colors } = useTheme();
  const { show } = useToast();

  const [searchInput, setSearchInput] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  // Raw user selection only — never written to from a derived value, so it
  // stays a plain "last thing the user tapped" rather than something an
  // effect has to keep resynchronized with the loaded category list.
  const [selectedCategoryOverride, setSelectedCategoryOverride] = useState("");
  const [bookmarkingId, setBookmarkingId] = useState<string | null>(null);

  useEffect(() => {
    const handle = setTimeout(() => setDebouncedQuery(searchInput.trim()), SEARCH_DEBOUNCE_MS);
    return () => clearTimeout(handle);
  }, [searchInput]);

  const isSearching = debouncedQuery.length > 0;

  const categoriesQuery = useNewsCategoriesQuery();
  const categories = categoriesQuery.data ?? [];
  // Falls back to the first loaded category until the user picks one of
  // their own — computed directly during render instead of synced via a
  // `useEffect`+`setState` pair, which would just cause an extra render.
  const selectedCategory =
    selectedCategoryOverride && categories.includes(selectedCategoryOverride)
      ? selectedCategoryOverride
      : (categories[0] ?? "");

  const feedQuery = useArticlesByCategoryQuery(selectedCategory);
  const searchQuery = useSearchArticlesQuery(debouncedQuery);
  const toggleBookmarkMutation = useToggleBookmarkMutation();
  // The real `NewsArticleDto` has no `isBookmarked` field on any endpoint
  // (confirmed against the backend source) — every row's star state is
  // cross-referenced against the bookmarks list instead.
  const bookmarkedIds = useBookmarkedArticleIds();

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

  function renderArticleList(articles: ArticleSummaryDto[]) {
    return (
      <Card>
        {articles.map((article, index) => (
          <React.Fragment key={article.id}>
            {index > 0 ? <Divider /> : null}
            <ArticleRow
              article={article}
              isBookmarked={bookmarkedIds.has(article.id)}
              onPress={openArticle}
              onToggleBookmark={handleToggleBookmark}
              bookmarkPending={bookmarkingId === article.id}
            />
          </React.Fragment>
        ))}
      </Card>
    );
  }

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Current Affairs</Text>
        <Pressable
          onPress={() => router.push("/(app)/currentaffairs/bookmarks")}
          hitSlop={8}
          accessibilityRole="button"
          accessibilityLabel="View bookmarks"
          className="h-10 w-10 items-center justify-center rounded-full bg-surface active:opacity-70 dark:bg-surface-dark"
        >
          <Icon name="star-outline" size={20} color={colors.textPrimary} />
        </Pressable>
      </View>

      <TextField
        label="Search"
        placeholder="Search current affairs"
        value={searchInput}
        onChangeText={setSearchInput}
        returnKeyType="search"
        accessibilityLabel="Search current affairs"
      />

      {isSearching ? (
        <View>
          <Text className="mb-3 text-subheading text-ink-primary dark:text-ink-primary-dark">
            Search results for &quot;{debouncedQuery}&quot;
          </Text>
          {searchQuery.isLoading ? (
            <ArticleListSkeleton />
          ) : searchQuery.isError ? (
            <ErrorState
              title="Couldn't search right now"
              description="Check your connection and try again."
              onRetry={() => void searchQuery.refetch()}
            />
          ) : (searchQuery.data ?? []).length === 0 ? (
            <EmptyState
              icon="search-outline"
              title="No results"
              description="Try a different search term."
            />
          ) : (
            renderArticleList(searchQuery.data!)
          )}
        </View>
      ) : (
        <View>
          <WeeklyDigestTeaser />

          {categoriesQuery.isLoading ? (
            <Skeleton variant="rect" height={36} className="mb-4 w-2/3 rounded-full" />
          ) : categoriesQuery.isError ? (
            <ErrorState
              title="Couldn't load categories"
              description="Check your connection and try again."
              onRetry={() => void categoriesQuery.refetch()}
              className="py-4"
            />
          ) : (
            <CategoryChipRow
              categories={categories}
              selectedCategory={selectedCategory}
              onSelect={setSelectedCategoryOverride}
              className="mb-4"
            />
          )}

          {feedQuery.isLoading ? (
            <ArticleListSkeleton />
          ) : feedQuery.isError ? (
            <ErrorState
              title="Couldn't load news"
              description="Check your connection and try again."
              onRetry={() => void feedQuery.refetch()}
            />
          ) : (feedQuery.data ?? []).length === 0 ? (
            <EmptyState icon="newspaper-outline" title="No articles yet" description="Check back soon for the latest news in this category." />
          ) : (
            renderArticleList(feedQuery.data!)
          )}
        </View>
      )}
    </ScreenContainer>
  );
}

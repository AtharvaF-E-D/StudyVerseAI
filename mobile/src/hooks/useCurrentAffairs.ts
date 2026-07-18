import { useMemo } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  getArticle,
  getArticleQuiz,
  getArticlesByCategory,
  getBookmarkedArticles,
  getNewsCategories,
  getWeeklyDigest,
  searchArticles,
  toggleArticleBookmark,
  type ArticleDetailDto,
  type ArticleQuizDto,
  type ArticleSummaryDto,
  type ToggleBookmarkResponse,
  type WeeklyDigestDto,
} from "../api/currentaffairs";

export const newsCategoriesQueryKey = ["currentaffairs", "categories"] as const;

export function articlesByCategoryQueryKey(category: string) {
  return ["currentaffairs", "articles", "byCategory", category] as const;
}

export function searchArticlesQueryKey(query: string) {
  return ["currentaffairs", "search", query] as const;
}

export function articleDetailQueryKey(articleId: string) {
  return ["currentaffairs", "articles", "detail", articleId] as const;
}

export const bookmarkedArticlesQueryKey = ["currentaffairs", "bookmarks"] as const;

export function articleQuizQueryKey(articleId: string) {
  return ["currentaffairs", "articles", "detail", articleId, "quiz"] as const;
}

export const weeklyDigestQueryKey = ["currentaffairs", "digest", "weekly"] as const;

/** Fetches the list of news categories for the category selector. */
export function useNewsCategoriesQuery() {
  return useQuery<string[]>({
    queryKey: newsCategoriesQueryKey,
    queryFn: getNewsCategories,
  });
}

/** Fetches the article feed for one category. */
export function useArticlesByCategoryQuery(category: string) {
  return useQuery<ArticleSummaryDto[]>({
    queryKey: articlesByCategoryQueryKey(category),
    queryFn: () => getArticlesByCategory(category),
    enabled: category.length > 0,
  });
}

/**
 * Searches articles by free-text query. A `useQuery` (not a mutation) so the
 * feed screen can key results by the (debounced) search text and let React
 * Query cache/dedupe repeated searches, same as every other list fetch in
 * this app — `enabled` is what actually makes this "user-triggered" in
 * effect, since it only fires once `query` is non-empty.
 */
export function useSearchArticlesQuery(query: string) {
  return useQuery<ArticleSummaryDto[]>({
    queryKey: searchArticlesQueryKey(query),
    queryFn: () => searchArticles(query),
    enabled: query.trim().length > 0,
  });
}

/** Fetches one article's full detail (including body content) for the article detail screen. */
export function useArticleQuery(articleId: string) {
  return useQuery<ArticleDetailDto>({
    queryKey: articleDetailQueryKey(articleId),
    queryFn: () => getArticle(articleId),
    enabled: articleId.length > 0,
  });
}

/** Fetches the signed-in user's bookmarked articles for the bookmarks screen. */
export function useBookmarksQuery() {
  return useQuery<ArticleSummaryDto[]>({
    queryKey: bookmarkedArticlesQueryKey,
    queryFn: getBookmarkedArticles,
  });
}

/**
 * Derives the signed-in user's bookmarked article ids as a `Set`, for O(1)
 * "is this article bookmarked" lookups. This exists because the real
 * `NewsArticleDto` (confirmed by reading `Common/CurrentAffairsDtos.cs`) has
 * NO `isBookmarked` field on any endpoint — the feed, search, and detail
 * responses genuinely don't carry per-user bookmark state, so every screen
 * that renders a bookmark star has to cross-reference `GET /bookmarks`
 * itself rather than trust a field that doesn't exist on the wire.
 */
export function useBookmarkedArticleIds(): ReadonlySet<string> {
  const bookmarksQuery = useBookmarksQuery();
  return useMemo(() => new Set(bookmarksQuery.data?.map((article) => article.id) ?? []), [bookmarksQuery.data]);
}

/**
 * Toggles one article's bookmark flag. Invalidates every list this article
 * could appear in (its category feed, any cached search results, the
 * bookmarks list, and its own detail) so `isBookmarked`/list membership are
 * refetched from the server's authoritative state everywhere rather than
 * trying to hand-patch each cached list's entry in place.
 */
export function useToggleBookmarkMutation() {
  const queryClient = useQueryClient();

  return useMutation<ToggleBookmarkResponse, unknown, string>({
    mutationFn: (articleId) => toggleArticleBookmark(articleId),
    onSuccess: (_result, articleId) => {
      void queryClient.invalidateQueries({ queryKey: ["currentaffairs", "articles"] });
      void queryClient.invalidateQueries({ queryKey: ["currentaffairs", "search"] });
      void queryClient.invalidateQueries({ queryKey: bookmarkedArticlesQueryKey });
      void queryClient.invalidateQueries({ queryKey: articleDetailQueryKey(articleId) });
    },
  });
}

/**
 * Fetches (and, on the first call for a given article, triggers real AI
 * generation of) that article's quiz. `enabled` defaults to `false` — the
 * quiz screen only calls this once the user actually taps "Test your
 * understanding", not as soon as the article detail screen mounts.
 */
export function useArticleQuizQuery(articleId: string, options?: { enabled?: boolean }) {
  return useQuery<ArticleQuizDto>({
    queryKey: articleQuizQueryKey(articleId),
    queryFn: () => getArticleQuiz(articleId),
    enabled: (options?.enabled ?? true) && articleId.length > 0,
    retry: false,
  });
}

/** Fetches the current AI-generated weekly digest, or `null` when there isn't enough data yet. */
export function useWeeklyDigestQuery() {
  return useQuery<WeeklyDigestDto | null>({
    queryKey: weeklyDigestQueryKey,
    queryFn: getWeeklyDigest,
  });
}

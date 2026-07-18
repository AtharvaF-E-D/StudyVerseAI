import { isAxiosError } from "axios";

import { coreApiClient } from "./client";

// ---------------------------------------------------------------------------
// This client targets the "Current Affairs" backend contract (base path
// `api/v1/currentaffairs`), built in parallel with this mobile work. Unlike
// `flashcards.ts`/`mocktests.ts` at the time they were written, the real
// backend source landed here mid-session — `CurrentAffairsController.cs` and
// `StudyVerse.Application/Features/CurrentAffairs/**` (including the actual
// `NewsArticleDto`/`ToggleBookmarkResultDto`/`NewsArticleQuizDto`/
// `WeeklyDigestDto` records in `Common/CurrentAffairsDtos.cs`) — so every
// type below was cross-checked directly against that source, not just the
// contract shorthand handed off for this phase. The server itself still
// wasn't reachable from this environment (Postgres is up, but the
// connection string in user-secrets/env doesn't match its actual
// credentials here — a pre-existing environment gap, not something this
// pass introduced), so this couldn't be verified live end-to-end; see the
// mobile phase report for details.
//
// Real deviations the shorthand contract got wrong, found by reading the
// actual DTOs:
//
// 1. `NewsArticleDto` has NO `isBookmarked` field, on ANY endpoint — not the
//    feed, not search, not even the detail endpoint. It's one single DTO
//    shape reused identically everywhere (see `NewsArticleMappings.cs`),
//    and none of the query handlers attach per-user bookmark state to it.
//    The only way to know whether a given article is bookmarked is to cross-
//    reference its id against `GET /bookmarks`'s result — see
//    `useBookmarkedArticleIds` in `useCurrentAffairs.ts`, which every screen
//    showing a bookmark star relies on instead of trusting a field that
//    doesn't exist on the wire.
// 2. `NewsArticleDto` always includes `content` (and a real `category`
//    field) — there's no separate lighter-weight "summary" shape for the
//    feed/search/bookmarks lists vs. the detail endpoint; `ArticleDetailDto`
//    below is kept only as a readability alias at detail-screen call sites.
// 3. `POST .../bookmark` returns `{ articleId, isBookmarked }`, not just
//    `{ isBookmarked }`.
// 4. The quiz and digest DTOs both carry an extra field the shorthand
//    didn't mention (`articleId`/`generatedAtUtc` on the quiz;
//    `generatedAtUtc` on the digest) — harmless to have typed even though
//    the UI doesn't currently display them.
//
// One nuance carried over from every other controller in this app: enums
// serialize as camelCase strings (`JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`)
// — not that anything here is an enum on the wire, but `Guid`/`DateTime`/
// `DateOnly` all serialize as plain strings, which is what every `id`/
// `...AtUtc`/`weekStartDateUtc` field below is typed as.
// ---------------------------------------------------------------------------

// ---------------------------------------------------------------------------
// GET /currentaffairs/categories  →  string[]
// ---------------------------------------------------------------------------

export async function getNewsCategories(): Promise<string[]> {
  const { data } = await coreApiClient.get<string[]>("/currentaffairs/categories");
  return data;
}

// ---------------------------------------------------------------------------
// GET /currentaffairs/articles?category=&take=   →  NewsArticleDto[]
// GET /currentaffairs/search?q=                  →  NewsArticleDto[]
// GET /currentaffairs/bookmarks                  →  NewsArticleDto[]
// GET /currentaffairs/articles/{id}               →  NewsArticleDto
// ---------------------------------------------------------------------------

/**
 * Mirrors the real `NewsArticleDto` record exactly (`Common/CurrentAffairsDtos.cs`)
 * — the same shape is returned by the feed, search, bookmarks, and detail
 * endpoints alike, `content` included every time. Deliberately has NO
 * `isBookmarked` field — see this file's header comment.
 */
export interface ArticleSummaryDto {
  id: string;
  title: string;
  description: string | null;
  content: string;
  url: string;
  imageUrl: string | null;
  category: string;
  sourceName: string;
  publishedAtUtc: string;
}

/**
 * Structurally identical to `ArticleSummaryDto` — kept as a distinct name
 * purely for readability at the article detail screen's call site, since
 * the real backend has no separate lighter "summary" shape.
 */
export type ArticleDetailDto = ArticleSummaryDto;

/** `take` is left to the backend's own default (10) when omitted — `GetArticlesByCategory` treats `take <= 0` the same as not passing it. */
export async function getArticlesByCategory(category: string, take?: number): Promise<ArticleSummaryDto[]> {
  const { data } = await coreApiClient.get<ArticleSummaryDto[]>("/currentaffairs/articles", {
    params: { category, take },
  });
  return data;
}

export async function searchArticles(query: string): Promise<ArticleSummaryDto[]> {
  const { data } = await coreApiClient.get<ArticleSummaryDto[]>("/currentaffairs/search", {
    params: { q: query },
  });
  return data;
}

export async function getBookmarkedArticles(): Promise<ArticleSummaryDto[]> {
  const { data } = await coreApiClient.get<ArticleSummaryDto[]>("/currentaffairs/bookmarks");
  return data;
}

export async function getArticle(articleId: string): Promise<ArticleDetailDto> {
  const { data } = await coreApiClient.get<ArticleDetailDto>(`/currentaffairs/articles/${articleId}`);
  return data;
}

// ---------------------------------------------------------------------------
// POST /currentaffairs/articles/{id}/bookmark  →  { articleId, isBookmarked }
// ---------------------------------------------------------------------------

/** Mirrors the real `ToggleBookmarkResultDto(Guid ArticleId, bool IsBookmarked)`. */
export interface ToggleBookmarkResponse {
  articleId: string;
  isBookmarked: boolean;
}

export async function toggleArticleBookmark(articleId: string): Promise<ToggleBookmarkResponse> {
  const { data } = await coreApiClient.post<ToggleBookmarkResponse>(
    `/currentaffairs/articles/${articleId}/bookmark`,
  );
  return data;
}

// ---------------------------------------------------------------------------
// GET /currentaffairs/articles/{id}/quiz  →  NewsArticleQuizDto
// ---------------------------------------------------------------------------

/** Mirrors the real `NewsArticleQuizQuestionDto`. Unlike Rapid Fire Quiz, `correctOptionIndex` is included up front — there's no session/score/opponent for a one-reader comprehension quiz to protect against. */
export interface ArticleQuizQuestionDto {
  questionText: string;
  options: string[];
  correctOptionIndex: number;
  explanation: string;
}

/** Mirrors the real `NewsArticleQuizDto(Guid ArticleId, IReadOnlyList<NewsArticleQuizQuestionDto> Questions, DateTime GeneratedAtUtc)`. */
export interface ArticleQuizDto {
  articleId: string;
  questions: ArticleQuizQuestionDto[];
  generatedAtUtc: string;
}

/**
 * The FIRST request for a given article's quiz generates it for real via
 * OpenAI server-side (subsequent requests hit a cache) — mirroring
 * `createStudyPlan`'s finding that the shared 15s `coreApiClient` timeout is
 * nowhere near enough for a real generation call and silently kills it
 * client-side (which also aborts the in-flight OpenAI call server-side via
 * `HttpContext.RequestAborted`). Overriding just this call's timeout to 120s
 * is the same narrow fix applied there, rather than raising the shared
 * default for every other feature's fast reads.
 */
export async function getArticleQuiz(articleId: string): Promise<ArticleQuizDto> {
  const { data } = await coreApiClient.get<ArticleQuizDto>(`/currentaffairs/articles/${articleId}/quiz`, {
    timeout: 120_000,
  });
  return data;
}

// ---------------------------------------------------------------------------
// GET /currentaffairs/digest/weekly  →  WeeklyDigestDto, or a real 404 when
// there isn't enough cached news yet (confirmed via `GetWeeklyDigestQueryHandler`,
// which returns `Result.Failure(..., ResultErrorType.NotFound)` in that case).
// ---------------------------------------------------------------------------

/** Mirrors the real `WeeklyDigestDto(DateOnly WeekStartDateUtc, string SummaryText, DateTime GeneratedAtUtc)`. `weekStartDateUtc` serializes as a plain `"yyyy-MM-dd"` string (`DateOnly`'s default `System.Text.Json` format). */
export interface WeeklyDigestDto {
  summaryText: string;
  weekStartDateUtc: string;
  generatedAtUtc: string;
}

/**
 * Fetches the AI-generated weekly digest, or `null` when there isn't enough
 * data yet to produce one — confirmed to be a real 404 server-side, same
 * convention as `getActiveStudyPlan`. Still tolerates an empty/null body or
 * a shape missing the fields that actually matter, rather than assuming
 * every environment's proxy/gateway preserves the exact 404 verbatim.
 */
export async function getWeeklyDigest(): Promise<WeeklyDigestDto | null> {
  try {
    const { data } = await coreApiClient.get<Partial<WeeklyDigestDto> | null>("/currentaffairs/digest/weekly");
    if (!data || !data.summaryText || !data.weekStartDateUtc) {
      return null;
    }
    return data as WeeklyDigestDto;
  } catch (error) {
    if (isAxiosError(error) && error.response?.status === 404) {
      return null;
    }
    throw error;
  }
}

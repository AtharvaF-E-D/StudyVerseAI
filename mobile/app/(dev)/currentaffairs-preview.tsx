import React, { useMemo, useState } from "react";
import { ActivityIndicator, Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { EmptyState } from "../../src/components/EmptyState";
import { Icon } from "../../src/components/Icon";
import { QuizOptionButton, type QuizOptionState } from "../../src/components/quiz/QuizOptionButton";
import { ArticleRow } from "../../src/components/currentaffairs/ArticleRow";
import { ArticleThumbnail } from "../../src/components/currentaffairs/ArticleThumbnail";
import { CategoryChipRow } from "../../src/components/currentaffairs/CategoryChipRow";
import { useTheme } from "../../src/theme/ThemeProvider";
import type { ArticleDetailDto, ArticleQuizQuestionDto, ArticleSummaryDto, WeeklyDigestDto } from "../../src/api/currentaffairs";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/currentaffairs-preview`. Mirrors `app/(dev)/quiz-preview.tsx`'s
// approach: the real backend source (`CurrentAffairsController.cs`,
// `StudyVerse.Application/Features/CurrentAffairs/**`) landed mid-session and
// was read directly to shape every type in `src/api/currentaffairs.ts`, but
// the server itself still wasn't reachable from this environment (Postgres
// is up, but the connection string this app resolves to has a credential
// mismatch — a pre-existing environment gap). So this feeds hand-written
// fixtures, shaped exactly like the real `NewsArticleDto` (notably: NO
// `isBookmarked` field — see `useBookmarkedArticleIds` in
// `useCurrentAffairs.ts`), straight into the same shared components the real
// feed/detail/quiz/bookmarks screens use (`ArticleRow`, `ArticleThumbnail`,
// `CategoryChipRow`, `QuizOptionButton`) for manual/automated visual QA in
// both light and dark mode. None of the data below comes from — or is wired
// to — the real current affairs API. Delete this file once the real backend
// is reachable and the current affairs screens have been re-verified
// against it.
// ---------------------------------------------------------------------------

const FIXTURE_CATEGORIES = [
  "general",
  "world",
  "nation",
  "business",
  "technology",
  "entertainment",
  "sports",
  "science",
  "health",
];

// Deliberately exercises all three thumbnail states `ArticleThumbnail` has to
// handle: a real (reachable) image, a URL that will 404/fail to resolve, and
// a `null` imageUrl — see that component's fallback-to-placeholder logic.
// `content` and `category` are always present, same as the real `NewsArticleDto`
// (there's no separate lighter "summary" shape server-side); there's
// deliberately no `isBookmarked` field — see this file's header comment.
const FIXTURE_ARTICLES: ArticleSummaryDto[] = [
  {
    id: "a1",
    title: "Global Leaders Reach New Climate Accord at Summit",
    description: "Delegates from over 190 countries agreed on updated emissions targets after two weeks of talks.",
    content:
      "Delegates from more than 190 countries concluded a two-week summit today with a new accord setting " +
      "updated emissions-reduction targets for the next decade. The agreement, reached after intense overnight " +
      "negotiations, commits signatory nations to cut greenhouse gas emissions by 45% from current levels by " +
      "2035, alongside a new fund to help developing economies transition to renewable energy sources.\n\n" +
      "Several major economies had pushed back on the timeline earlier in the talks, citing concerns about " +
      "energy security and the pace of industrial transition. A compromise was reached late last night that " +
      "phases in the strictest targets over a longer window while still preserving the original 2035 deadline " +
      "for the overall goal.\n\n" +
      "Environmental groups offered a cautiously positive response, calling the accord \"a meaningful step\" " +
      "while warning that enforcement mechanisms remain the real test of whether the targets are met.",
    imageUrl: "https://picsum.photos/seed/studyverse-news-1/400/300",
    category: "world",
    sourceName: "World News Wire",
    url: "https://example.com/articles/climate-accord",
    publishedAtUtc: "2026-07-18T06:30:00Z",
  },
  {
    id: "a2",
    title: "Central Bank Holds Interest Rates Steady Amid Inflation Concerns",
    description: "Policymakers cited mixed signals from labor markets in their decision to pause further hikes.",
    content:
      "The central bank's policy committee voted to hold its benchmark interest rate steady today, citing " +
      "mixed signals from labor markets and persistent but moderating inflation.",
    imageUrl: "https://this-domain-should-not-exist-studyverse-qa.invalid/broken.jpg",
    category: "business",
    sourceName: "Finance Daily",
    url: "https://example.com/articles/interest-rates",
    publishedAtUtc: "2026-07-18T02:15:00Z",
  },
  {
    id: "a3",
    title: "New Space Telescope Captures Clearest Images Yet of Distant Galaxy",
    description: "Astronomers say the images could reshape understanding of early galaxy formation.",
    content:
      "A newly deployed space telescope has captured its clearest images yet of a distant galaxy, giving " +
      "astronomers an unprecedented look at conditions in the early universe.",
    imageUrl: null,
    category: "science",
    sourceName: "Science Today",
    url: "https://example.com/articles/space-telescope",
    publishedAtUtc: "2026-07-17T18:00:00Z",
  },
  {
    id: "a4",
    title: "National Team Advances to Final After Dramatic Extra-Time Win",
    description: "A late goal in extra time sent fans into celebration across the country.",
    content:
      "A dramatic extra-time goal sent the national team through to the final today, capping off a tense " +
      "match that had fans on the edge of their seats until the very last minute.",
    imageUrl: "https://picsum.photos/seed/studyverse-news-4/400/300",
    category: "sports",
    sourceName: "Sports Network",
    url: "https://example.com/articles/final-advance",
    publishedAtUtc: "2026-07-17T12:45:00Z",
  },
];

/** Ids bookmarked in the fixture data — a standalone set, not a field on the articles themselves, mirroring how the real client has to track it (see `useBookmarkedArticleIds`). */
const FIXTURE_BOOKMARKED_IDS = new Set(["a1", "a4"]);

const FIXTURE_ARTICLE_DETAIL: ArticleDetailDto = FIXTURE_ARTICLES[0]!;

const FIXTURE_QUIZ_QUESTIONS: ArticleQuizQuestionDto[] = [
  {
    questionText: "By what percentage did signatory nations agree to cut emissions from current levels?",
    options: ["25% by 2030", "45% by 2035", "60% by 2040", "10% by 2028"],
    correctOptionIndex: 1,
    explanation: "The accord commits signatories to a 45% cut from current levels by 2035.",
  },
  {
    questionText: "What was created to help developing economies transition to renewable energy?",
    options: ["A new trade tariff", "A carbon tax", "A new fund", "A military alliance"],
    correctOptionIndex: 2,
    explanation: "The agreement includes a new fund specifically to help developing economies transition.",
  },
  {
    questionText: "How did environmental groups respond to the accord?",
    options: [
      "They rejected it outright",
      "Cautiously positive, but skeptical of enforcement",
      "They had no comment",
      "They called for stricter targets immediately",
    ],
    correctOptionIndex: 1,
    explanation: "Groups called it \"a meaningful step\" while flagging enforcement as the real test.",
  },
];

const FIXTURE_DIGEST: WeeklyDigestDto = {
  summaryText:
    "This week was dominated by the climate summit accord, a central bank rate decision holding steady amid " +
    "inflation concerns, and a major breakthrough image from a new space telescope. Sports fans also saw a " +
    "dramatic extra-time finish send the national team through to the final.",
  weekStartDateUtc: "2026-07-13",
  generatedAtUtc: "2026-07-18T09:00:00Z",
};

function SectionHeading({ children }: { children: string }) {
  return (
    <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">{children}</Text>
  );
}

function FeedFixtureSection() {
  const { colors } = useTheme();
  const [selectedCategory, setSelectedCategory] = useState("general");
  const [bookmarkedIds, setBookmarkedIds] = useState(FIXTURE_BOOKMARKED_IDS);

  function toggleBookmark(article: ArticleSummaryDto) {
    setBookmarkedIds((prev) => {
      const next = new Set(prev);
      if (next.has(article.id)) next.delete(article.id);
      else next.add(article.id);
      return next;
    });
  }

  return (
    <View className="mb-10">
      <SectionHeading>Fixture: news feed (categories, digest teaser, thumbnails, bookmark toggle)</SectionHeading>

      <Card className="mb-4 border-brand dark:border-brand-light">
        <View className="mb-1 flex-row items-center justify-between">
          <View className="flex-row items-center">
            <Icon name="sparkles" size={18} color={colors.brand} />
            <Text className="ml-2 text-subheading text-ink-primary dark:text-ink-primary-dark">Weekly Digest</Text>
          </View>
          <Icon name="chevron-forward" size={18} color={colors.textSecondary} />
        </View>
        <Text numberOfLines={2} className="text-body text-ink-primary dark:text-ink-primary-dark">
          {FIXTURE_DIGEST.summaryText}
        </Text>
      </Card>

      <CategoryChipRow
        categories={FIXTURE_CATEGORIES}
        selectedCategory={selectedCategory}
        onSelect={setSelectedCategory}
        className="mb-4"
      />

      <Card>
        {FIXTURE_ARTICLES.map((article, index) => (
          <React.Fragment key={article.id}>
            {index > 0 ? <Divider /> : null}
            <ArticleRow
              article={article}
              isBookmarked={bookmarkedIds.has(article.id)}
              onPress={() => {}}
              onToggleBookmark={toggleBookmark}
            />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

function ThumbnailStatesFixtureSection() {
  return (
    <View className="mb-10">
      <SectionHeading>Fixture: thumbnail fallback states (real image / broken URL / null)</SectionHeading>
      <Card>
        <View className="flex-row items-center justify-around py-2">
          <View className="items-center">
            <ArticleThumbnail imageUrl="https://picsum.photos/seed/studyverse-news-1/400/300" size={64} />
            <Text className="mt-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">Loads fine</Text>
          </View>
          <View className="items-center">
            <ArticleThumbnail imageUrl="https://this-domain-should-not-exist-studyverse-qa.invalid/broken.jpg" size={64} />
            <Text className="mt-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">Broken URL</Text>
          </View>
          <View className="items-center">
            <ArticleThumbnail imageUrl={null} size={64} />
            <Text className="mt-2 text-caption text-ink-secondary dark:text-ink-secondary-dark">No image</Text>
          </View>
        </View>
      </Card>
    </View>
  );
}

function DetailFixtureSection() {
  return (
    <View className="mb-10">
      <SectionHeading>Fixture: article detail</SectionHeading>
      <Card>
        <ArticleThumbnail imageUrl={FIXTURE_ARTICLE_DETAIL.imageUrl} size={120} className="mb-4 self-center" />
        <Text className="mb-2 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {FIXTURE_ARTICLE_DETAIL.title}
        </Text>
        <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
          {FIXTURE_ARTICLE_DETAIL.sourceName} · July 18, 2026
        </Text>
        <Divider className="mb-4" />
        <Text className="text-body text-ink-primary dark:text-ink-primary-dark">
          {FIXTURE_ARTICLE_DETAIL.content}
        </Text>
      </Card>
    </View>
  );
}

function QuizGeneratingFixtureSection() {
  const { colors } = useTheme();
  return (
    <View className="mb-10">
      <SectionHeading>Fixture: quiz — generating (first-time AI generation wait)</SectionHeading>
      <Card className="items-center py-8">
        <ActivityIndicator size="large" color={colors.brand} />
        <Text className="mb-2 mt-4 text-center text-subheading text-ink-primary dark:text-ink-primary-dark">
          Generating your quiz…
        </Text>
        <Text className="text-center text-caption text-ink-secondary dark:text-ink-secondary-dark">
          The first quiz for an article is written fresh by AI from its actual content — this can take up to a
          minute. It&apos;s instant after that.
        </Text>
      </Card>
    </View>
  );
}

function QuizInProgressFixtureSection() {
  const question = FIXTURE_QUIZ_QUESTIONS[0]!;
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const [revealed, setRevealed] = useState(false);

  function optionState(optionIndex: number): QuizOptionState {
    if (!revealed) return optionIndex === selectedIndex ? "selected" : "default";
    if (optionIndex === question.correctOptionIndex) return "correct";
    if (optionIndex === selectedIndex) return "incorrect";
    return "dimmed";
  }

  function handleToggle() {
    if (revealed) {
      setRevealed(false);
      setSelectedIndex(null);
    } else {
      setSelectedIndex(0); // simulate an incorrect pick, for the reveal-coloring screenshot
      setRevealed(true);
    }
  }

  return (
    <View className="mb-10">
      <View className="mb-4 flex-row items-center justify-between">
        <Text className="flex-1 pr-3 text-heading text-ink-primary dark:text-ink-primary-dark">
          Fixture: quiz — in progress (1 / 3)
        </Text>
        <Button title={revealed ? "Reset" : "Reveal answer"} variant="secondary" fullWidth={false} onPress={handleToggle} />
      </View>
      <Card>
        <Text className="mb-1 text-caption text-ink-secondary dark:text-ink-secondary-dark">Question 1 / 3</Text>
        <Text className="mb-5 text-subheading text-ink-primary dark:text-ink-primary-dark">
          {question.questionText}
        </Text>
        {question.options.map((option, index) => (
          <QuizOptionButton
            key={option}
            label={option}
            state={optionState(index)}
            onPress={revealed ? undefined : () => setSelectedIndex(index)}
          />
        ))}
        {revealed ? (
          <Card className="mt-1">
            <Text className="text-caption text-ink-secondary dark:text-ink-secondary-dark">{question.explanation}</Text>
          </Card>
        ) : null}
      </Card>
    </View>
  );
}

function QuizCompleteFixtureSection() {
  const { colors } = useTheme();
  return (
    <View className="mb-10">
      <SectionHeading>Fixture: quiz — complete</SectionHeading>
      <Card className="mb-4 items-center">
        <Icon name="trophy" size={40} color={colors.brand} />
        <Text className="mb-1 mt-3 text-heading text-ink-primary dark:text-ink-primary-dark">Quiz complete!</Text>
        <Text className="text-center text-body text-ink-secondary dark:text-ink-secondary-dark">3 of 3 correct</Text>
      </Card>
      <Button title="Done" onPress={() => {}} />
    </View>
  );
}

function BookmarksFixtureSection() {
  const bookmarked = useMemo(
    () => FIXTURE_ARTICLES.filter((a) => FIXTURE_BOOKMARKED_IDS.has(a.id)),
    [],
  );

  return (
    <View className="mb-10">
      <SectionHeading>Fixture: bookmarks</SectionHeading>
      <Card>
        {bookmarked.map((article, index) => (
          <React.Fragment key={article.id}>
            {index > 0 ? <Divider /> : null}
            <ArticleRow article={article} isBookmarked onPress={() => {}} onToggleBookmark={() => {}} />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

function BookmarksEmptyFixtureSection() {
  return (
    <View className="mb-10">
      <SectionHeading>Fixture: bookmarks — empty state</SectionHeading>
      <Card>
        <EmptyState
          icon="star-outline"
          title="No bookmarks yet"
          description="Tap the star on any article to save it here for later."
        />
      </Card>
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Current Affairs UI, mirroring
 * the pattern established by `app/(dev)/quiz-preview.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/currentaffairs-preview`.
 */
export default function CurrentAffairsPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">
          Current Affairs Preview (dev)
        </Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <FeedFixtureSection />
      <ThumbnailStatesFixtureSection />
      <DetailFixtureSection />
      <QuizGeneratingFixtureSection />
      <QuizInProgressFixtureSection />
      <QuizCompleteFixtureSection />
      <BookmarksFixtureSection />
      <BookmarksEmptyFixtureSection />
    </ScreenContainer>
  );
}

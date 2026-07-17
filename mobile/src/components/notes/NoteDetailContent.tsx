import React, { useState } from "react";
import { ScrollView, Text, View } from "react-native";

import { Card } from "../Card";
import { Chip } from "../Chip";
import { Divider } from "../Divider";
import { EmptyState } from "../EmptyState";
import { MessageContent } from "../tutor/MessageContent";
import { FlashcardItem } from "./FlashcardItem";
import { McqItem } from "./McqItem";
import { MindMapOutline } from "./MindMapOutline";
import { useTheme } from "../../theme/ThemeProvider";
import type { NoteContentDto } from "../../api/notes";

export interface NoteDetailContentProps {
  content: NoteContentDto;
}

type SectionKey =
  | "summary"
  | "keyPoints"
  | "flashcards"
  | "mcqs"
  | "mindMap"
  | "revisionSheet"
  | "vocabulary"
  | "formulas";

const SECTIONS: { key: SectionKey; label: string }[] = [
  { key: "summary", label: "Summary" },
  { key: "keyPoints", label: "Key Points" },
  { key: "flashcards", label: "Flashcards" },
  { key: "mcqs", label: "MCQs" },
  { key: "mindMap", label: "Mind Map" },
  { key: "revisionSheet", label: "Revision Sheet" },
  { key: "vocabulary", label: "Vocabulary" },
  { key: "formulas", label: "Formulas" },
];

/**
 * Splits a revision sheet's raw text into paragraph/bullet lines. This is
 * deliberately NOT a markdown renderer (no headings/bold/links) — just
 * enough structure so `- `/`* `/`• `-prefixed lines read as a bulleted list
 * instead of a wall of text, per the spec's "trivial" bar for this section.
 */
function RevisionSheetText({ text }: { text: string }) {
  const lines = text.split("\n").filter((line) => line.trim().length > 0);

  if (lines.length === 0) {
    return (
      <Text className="text-body text-ink-secondary dark:text-ink-secondary-dark">
        No revision sheet generated.
      </Text>
    );
  }

  return (
    <View>
      {lines.map((line, index) => {
        const trimmed = line.trim();
        const isBullet = /^[-*•]\s+/.test(trimmed);
        const displayText = isBullet ? trimmed.replace(/^[-*•]\s+/, "") : trimmed;
        return (
          <View key={index} className={["flex-row items-start", index > 0 ? "mt-2" : ""].join(" ")}>
            {isBullet ? (
              <Text className="mr-2 text-body text-ink-secondary dark:text-ink-secondary-dark">{"•"}</Text>
            ) : null}
            <Text className="flex-1 text-body text-ink-primary dark:text-ink-primary-dark">{displayText}</Text>
          </View>
        );
      })}
    </View>
  );
}

/**
 * Tabbed (segmented-control) body for one note's generated content —
 * Summary, Key Points, Flashcards, MCQs, Mind Map, Revision Sheet,
 * Vocabulary, and Formulas. Pure presentational component fed a note's
 * `NoteContentDto` (the backend nests generated content under
 * `NoteDetailDto.content`, non-null only once `status` is `"ready"`), used
 * by both the real detail screen (`app/(app)/notes/[id].tsx`) and the
 * `(dev)/notes-preview` fixture screen — same split as `DashboardContent`.
 */
export function NoteDetailContent({ content }: NoteDetailContentProps) {
  const { colors } = useTheme();
  const [activeSection, setActiveSection] = useState<SectionKey>("summary");

  return (
    <View>
      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        className="mb-4"
        contentContainerClassName="gap-2 pr-2"
      >
        {SECTIONS.map((section) => (
          <Chip
            key={section.key}
            label={section.label}
            selected={activeSection === section.key}
            onPress={() => setActiveSection(section.key)}
          />
        ))}
      </ScrollView>

      {activeSection === "summary" ? (
        <Card>
          {content.summary ? (
            <Text className="text-body text-ink-primary dark:text-ink-primary-dark">{content.summary}</Text>
          ) : (
            <EmptyState icon="document-text-outline" title="No summary yet" />
          )}
        </Card>
      ) : null}

      {activeSection === "keyPoints" ? (
        content.keyPoints.length === 0 ? (
          <EmptyState icon="list-outline" title="No key points yet" />
        ) : (
          <Card>
            {content.keyPoints.map((point, index) => (
              <View key={index} className={["flex-row items-start", index > 0 ? "mt-3" : ""].join(" ")}>
                <View className="mr-3 mt-2 h-1.5 w-1.5 rounded-full bg-brand dark:bg-brand-light" />
                <Text className="flex-1 text-body text-ink-primary dark:text-ink-primary-dark">{point}</Text>
              </View>
            ))}
          </Card>
        )
      ) : null}

      {activeSection === "flashcards" ? (
        content.flashcards.length === 0 ? (
          <EmptyState icon="albums-outline" title="No flashcards yet" />
        ) : (
          <View>
            {content.flashcards.map((flashcard, index) => (
              <FlashcardItem key={index} index={index} flashcard={flashcard} className="mb-3" />
            ))}
          </View>
        )
      ) : null}

      {activeSection === "mcqs" ? (
        content.mcqs.length === 0 ? (
          <EmptyState icon="help-circle-outline" title="No practice questions yet" />
        ) : (
          <View>
            {content.mcqs.map((mcq, index) => (
              <McqItem key={index} index={index} mcq={mcq} className="mb-3" />
            ))}
          </View>
        )
      ) : null}

      {activeSection === "mindMap" ? (
        <Card>
          <MindMapOutline node={content.mindMap} />
        </Card>
      ) : null}

      {activeSection === "revisionSheet" ? (
        <Card>
          <RevisionSheetText text={content.revisionSheet} />
        </Card>
      ) : null}

      {activeSection === "vocabulary" ? (
        content.vocabulary.length === 0 ? (
          <EmptyState icon="book-outline" title="No vocabulary yet" />
        ) : (
          <Card>
            {content.vocabulary.map((entry, index) => (
              <React.Fragment key={index}>
                {index > 0 ? <Divider className="my-3" /> : null}
                <Text className="text-bodyMedium text-ink-primary dark:text-ink-primary-dark">{entry.term}</Text>
                <Text className="mt-1 text-body text-ink-secondary dark:text-ink-secondary-dark">
                  {entry.definition}
                </Text>
              </React.Fragment>
            ))}
          </Card>
        )
      ) : null}

      {activeSection === "formulas" ? (
        content.formulas.length === 0 ? (
          <EmptyState icon="calculator-outline" title="No formulas yet" />
        ) : (
          <Card>
            {content.formulas.map((formula, index) => (
              <React.Fragment key={index}>
                {index > 0 ? <Divider className="my-3" /> : null}
                <Text className="mb-1 text-bodyMedium text-ink-primary dark:text-ink-primary-dark">
                  {formula.name}
                </Text>
                <MessageContent
                  content={formula.formula}
                  textColor={colors.textPrimary}
                  textClassName="text-body font-semibold text-ink-primary dark:text-ink-primary-dark"
                />
                {formula.explanation ? (
                  <View className="mt-2">
                    <MessageContent
                      content={formula.explanation}
                      textColor={colors.textSecondary}
                      textClassName="text-caption text-ink-secondary dark:text-ink-secondary-dark"
                    />
                  </View>
                ) : null}
              </React.Fragment>
            ))}
          </Card>
        )
      ) : null}
    </View>
  );
}

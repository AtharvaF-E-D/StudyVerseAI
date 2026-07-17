import React from "react";
import { Text, View } from "react-native";

import { ScreenContainer } from "../../src/components/ScreenContainer";
import { Badge } from "../../src/components/Badge";
import { Button } from "../../src/components/Button";
import { Card } from "../../src/components/Card";
import { Divider } from "../../src/components/Divider";
import { Icon } from "../../src/components/Icon";
import { ListItem } from "../../src/components/ListItem";
import { NoteDetailContent } from "../../src/components/notes/NoteDetailContent";
import { useTheme } from "../../src/theme/ThemeProvider";
import { formatRelativeTime } from "../../src/lib/relativeTime";
import { NOTE_STATUS_BADGE_VARIANTS, NOTE_STATUS_LABELS } from "../../src/lib/noteStatus";
import type { NoteContentDto, NoteDetailDto, NoteSummaryDto } from "../../src/api/notes";

// ---------------------------------------------------------------------------
// DEV-ONLY SCAFFOLDING — not linked from any navigation, reached directly at
// `/(dev)/notes-preview`. The real `/api/v1/notes/*` backend (source now in
// this repo — `NotesController.cs`, `StudyVerse.Application/Features/Notes/**`)
// requires a locally-running Postgres with matching credentials, which
// wasn't reachable from this environment (see the mobile Phase 6 report for
// details) — so this feeds hand-written fixtures, shaped exactly like the
// real `NoteDetailDto`/`NoteContentDto` records, straight into the same
// rendering components the real list/detail screens use (`ListItem`/`Badge`
// for the list row, `NoteDetailContent` → `FlashcardItem` / `McqItem` /
// `MindMapOutline` / `MessageContent` for the detail sections) for
// manual/automated visual QA in both light and dark mode. None of the data
// below comes from — or is wired to — the real notes API. Delete this file
// once the real backend is reachable and the notes screens have been
// re-verified against it.
// ---------------------------------------------------------------------------

const FIXTURE_NOTES: NoteSummaryDto[] = [
  {
    id: "n-ready",
    title: "Cell Biology — Chapter 4",
    status: "ready",
    createdAtUtc: "2026-07-17T08:15:00Z",
  },
  {
    id: "n-processing",
    title: "Photosynthesis diagram.jpg",
    status: "processing",
    createdAtUtc: "2026-07-17T09:50:00Z",
  },
  {
    id: "n-failed",
    title: "Handwritten calculus notes.png",
    status: "failed",
    createdAtUtc: "2026-07-16T14:20:00Z",
  },
];

const FIXTURE_NOTE_CONTENT: NoteContentDto = {
  summary:
    "This chapter covers the structure and function of eukaryotic cells, focusing on the roles of the nucleus, mitochondria, and endoplasmic reticulum in maintaining cellular homeostasis and energy production.",
  keyPoints: [
    "The nucleus houses the cell's genetic material and controls gene expression.",
    "Mitochondria generate ATP through cellular respiration — the \"powerhouse of the cell\".",
    "The rough endoplasmic reticulum synthesizes proteins; the smooth ER synthesizes lipids.",
    "The Golgi apparatus modifies, sorts, and packages proteins for secretion or use within the cell.",
  ],
  flashcards: [
    { question: "What is the main function of mitochondria?", answer: "Producing ATP through cellular respiration." },
    { question: "What distinguishes rough ER from smooth ER?", answer: "Rough ER is studded with ribosomes and synthesizes proteins; smooth ER lacks ribosomes and synthesizes lipids." },
    { question: "Where is a cell's DNA stored?", answer: "In the nucleus, organized into chromatin/chromosomes." },
  ],
  mcqs: [
    {
      question: "Which organelle is primarily responsible for ATP production?",
      options: ["Nucleus", "Mitochondria", "Golgi apparatus", "Lysosome"],
      correctOptionIndex: 1,
      explanation: "Mitochondria carry out cellular respiration, converting glucose and oxygen into ATP.",
    },
    {
      question: "The Golgi apparatus is best described as the cell's:",
      options: ["Genetic library", "Shipping and packaging center", "Power plant", "Waste disposal unit"],
      correctOptionIndex: 1,
      explanation: "The Golgi apparatus modifies, sorts, and packages proteins and lipids for transport.",
    },
  ],
  mindMap: {
    topic: "Eukaryotic Cell",
    children: [
      {
        topic: "Nucleus",
        children: [
          { topic: "Chromatin / DNA", children: [] },
          { topic: "Nucleolus (ribosome assembly)", children: [] },
        ],
      },
      {
        topic: "Mitochondria",
        children: [{ topic: "Cellular respiration", children: [] }, { topic: "ATP production", children: [] }],
      },
      {
        topic: "Endoplasmic Reticulum",
        children: [
          { topic: "Rough ER — protein synthesis", children: [] },
          { topic: "Smooth ER — lipid synthesis", children: [] },
        ],
      },
      { topic: "Golgi Apparatus", children: [{ topic: "Sorting & packaging", children: [] }] },
    ],
  },
  revisionSheet:
    "Cell Biology — Chapter 4 revision\n\n" +
    "- Know the four major organelles covered: nucleus, mitochondria, ER, Golgi apparatus.\n" +
    "- Be able to explain ATP production in one sentence.\n" +
    "- Compare rough vs. smooth ER — this is a common exam question.\n\n" +
    "Remember: structure follows function. Each organelle's shape directly supports its job.",
  vocabulary: [
    { term: "Organelle", definition: "A specialized structure within a cell that performs a specific function." },
    { term: "ATP", definition: "Adenosine triphosphate — the primary energy currency of the cell." },
    { term: "Chromatin", definition: "The combination of DNA and proteins that makes up a cell's chromosomes." },
  ],
  formulas: [
    {
      name: "Cellular respiration (overall)",
      formula: "$C_6H_{12}O_6 + 6O_2 \\rightarrow 6CO_2 + 6H_2O + \\text{ATP}$",
      explanation: "Glucose and oxygen react to release carbon dioxide, water, and usable energy as ATP.",
    },
    {
      name: "Surface area to volume ratio",
      formula: "$$\\dfrac{SA}{V} = \\dfrac{6s^2}{s^3} = \\dfrac{6}{s}$$",
      explanation: "For a cube of side length $s$, this ratio shrinks as the cell grows — a key limit on cell size.",
    },
  ],
};

/** Full `NoteDetailDto` envelope around `FIXTURE_NOTE_CONTENT` — realistic, but `NoteDetailContent` only ever consumes the nested `content`. */
const FIXTURE_NOTE_DETAIL: NoteDetailDto = {
  id: "n-ready",
  title: "Cell Biology — Chapter 4",
  sourceFileName: "cell-biology-ch4.pdf",
  sourceFileType: "pdf",
  status: "ready",
  extractedText: "Chapter 4: The Eukaryotic Cell...",
  errorMessage: null,
  createdAtUtc: "2026-07-17T08:15:00Z",
  content: FIXTURE_NOTE_CONTENT,
};

function ListFixtureSection() {
  const { colors } = useTheme();

  return (
    <View className="mb-10">
      <Text className="mb-4 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: notes list (ready / processing / failed)
      </Text>
      <Card>
        {FIXTURE_NOTES.map((note, index) => (
          <React.Fragment key={note.id}>
            {index > 0 ? <Divider /> : null}
            <ListItem
              leading={<Icon name="document-text-outline" size={22} color={colors.brand} />}
              title={note.title}
              subtitle={formatRelativeTime(note.createdAtUtc)}
              trailing={
                <Badge label={NOTE_STATUS_LABELS[note.status]} variant={NOTE_STATUS_BADGE_VARIANTS[note.status]} />
              }
            />
          </React.Fragment>
        ))}
      </Card>
    </View>
  );
}

function DetailFixtureSection() {
  return (
    <View>
      <Text className="mb-1 text-heading text-ink-primary dark:text-ink-primary-dark">
        Fixture: note detail (all 8 sections, incl. math formulas + nested mind map)
      </Text>
      <Text className="mb-4 text-caption text-ink-secondary dark:text-ink-secondary-dark">
        Source: {FIXTURE_NOTE_DETAIL.sourceFileName} ({FIXTURE_NOTE_DETAIL.sourceFileType})
      </Text>
      {FIXTURE_NOTE_DETAIL.content ? <NoteDetailContent content={FIXTURE_NOTE_DETAIL.content} /> : null}
    </View>
  );
}

/**
 * Manual/automated visual QA screen for the Phase 6 AI Notes UI, mirroring
 * the pattern established by `app/(dev)/quiz-preview.tsx`. Not linked from
 * any navigation — reached directly at `/(dev)/notes-preview`.
 */
export default function NotesPreviewScreen() {
  const { scheme, setScheme } = useTheme();

  return (
    <ScreenContainer>
      <View className="mb-6 flex-row items-center justify-between">
        <Text className="text-heading text-ink-primary dark:text-ink-primary-dark">Notes Preview (dev)</Text>
        <Button
          title={scheme === "dark" ? "Switch to light" : "Switch to dark"}
          variant="secondary"
          fullWidth={false}
          onPress={() => setScheme(scheme === "dark" ? "light" : "dark")}
        />
      </View>

      <ListFixtureSection />
      <DetailFixtureSection />
    </ScreenContainer>
  );
}

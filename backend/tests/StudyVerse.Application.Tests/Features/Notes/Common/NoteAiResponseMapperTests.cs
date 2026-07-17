using FluentAssertions;
using StudyVerse.Application.Features.Notes.Common;

namespace StudyVerse.Application.Tests.Features.Notes.Common;

public sealed class NoteAiResponseMapperTests
{
    // A realistic sample of what OpenAI's JSON-mode response looks like for a short note about
    // photosynthesis — deliberately includes an MCQ with only 3 options and a formulas array with
    // one entry, to exercise the mapper's tolerance/normalization, not just the happy path.
    private const string RealisticSampleJson = """
        {
          "summary": "Photosynthesis is the process by which green plants, algae, and some bacteria convert light energy into chemical energy stored in glucose, using carbon dioxide and water and releasing oxygen as a byproduct.",
          "keyPoints": [
            "Photosynthesis occurs mainly in the chloroplasts of plant cells.",
            "Chlorophyll absorbs light energy, primarily in the red and blue wavelengths.",
            "The overall reaction converts CO2 and water into glucose and oxygen.",
            "It has two main stages: the light-dependent reactions and the Calvin cycle."
          ],
          "flashcards": [
            { "question": "What organelle does photosynthesis occur in?", "answer": "The chloroplast." },
            { "question": "What pigment absorbs light for photosynthesis?", "answer": "Chlorophyll." }
          ],
          "mcqs": [
            {
              "question": "What gas is released as a byproduct of photosynthesis?",
              "options": ["Oxygen", "Nitrogen", "Carbon dioxide"],
              "correctOptionIndex": 0,
              "explanation": "Water is split during the light-dependent reactions, releasing oxygen."
            }
          ],
          "mindMap": {
            "topic": "Photosynthesis",
            "children": [
              { "topic": "Light-dependent reactions", "children": [] },
              {
                "topic": "Calvin cycle",
                "children": [
                  { "topic": "Carbon fixation", "children": [] }
                ]
              }
            ]
          },
          "revisionSheet": "# Photosynthesis\n- Inputs: CO2 + H2O + light\n- Outputs: glucose + O2",
          "vocabulary": [
            { "term": "Chlorophyll", "definition": "The green pigment that absorbs light energy." }
          ],
          "formulas": [
            { "name": "Photosynthesis equation", "formula": "6CO2 + 6H2O + light -> C6H12O6 + 6O2", "explanation": "Overall balanced equation for photosynthesis." }
          ]
        }
        """;

    [Fact]
    public void Parse_RealisticAiResponse_ProducesExpectedDto()
    {
        var result = NoteAiResponseMapper.Parse(RealisticSampleJson);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value;

        dto.Summary.Should().StartWith("Photosynthesis is the process");
        dto.KeyPoints.Should().HaveCount(4);
        dto.Flashcards.Should().HaveCount(2);
        dto.Flashcards[0].Question.Should().Be("What organelle does photosynthesis occur in?");
        dto.Flashcards[0].Answer.Should().Be("The chloroplast.");

        dto.Mcqs.Should().HaveCount(1);
        // Only 3 options were supplied — the mapper must pad to exactly 4 so the client can
        // always render options A-D without special-casing a short array.
        dto.Mcqs[0].Options.Should().HaveCount(4);
        dto.Mcqs[0].Options[0].Should().Be("Oxygen");
        dto.Mcqs[0].Options[3].Should().BeEmpty();
        dto.Mcqs[0].CorrectOptionIndex.Should().Be(0);

        dto.MindMap.Topic.Should().Be("Photosynthesis");
        dto.MindMap.Children.Should().HaveCount(2);
        dto.MindMap.Children[1].Topic.Should().Be("Calvin cycle");
        dto.MindMap.Children[1].Children.Should().ContainSingle(c => c.Topic == "Carbon fixation");

        dto.RevisionSheet.Should().Contain("# Photosynthesis");
        dto.Vocabulary.Should().ContainSingle(v => v.Term == "Chlorophyll");
        dto.Formulas.Should().ContainSingle(f => f.Formula.Contains("6CO2"));
    }

    [Fact]
    public void ToEntity_ThenFromEntity_RoundTripsAllSevenFields()
    {
        var dto = NoteAiResponseMapper.Parse(RealisticSampleJson).Value;
        var noteId = Guid.NewGuid();

        var entity = NoteAiResponseMapper.ToEntity(noteId, dto);

        entity.NoteId.Should().Be(noteId);
        entity.Id.Should().NotBeEmpty();
        entity.Summary.Should().Be(dto.Summary);
        entity.RevisionSheet.Should().Be(dto.RevisionSheet);
        // The JSON sub-documents must actually be serialized JSON, not e.g. ToString() output.
        entity.KeyPointsJson.Should().Contain("\"Photosynthesis occurs mainly in the chloroplasts of plant cells.\"");
        entity.FlashcardsJson.Should().Contain("\"The chloroplast.\"");
        entity.McqsJson.Should().Contain("\"correctOptionIndex\":0");
        entity.MindMapJson.Should().Contain("\"Calvin cycle\"");
        entity.VocabularyJson.Should().Contain("\"Chlorophyll\"");
        entity.FormulasJson.Should().Contain("6CO2");

        var roundTripped = NoteAiResponseMapper.FromEntity(entity);

        roundTripped.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void Parse_WhenResponseOmitsFormulasAndVocabulary_DefaultsToEmptyListsRatherThanFailing()
    {
        const string jsonWithoutOptionalSections = """
            {
              "summary": "A brief history note.",
              "keyPoints": ["Point one."],
              "flashcards": [],
              "mcqs": [],
              "mindMap": { "topic": "History", "children": [] },
              "revisionSheet": "Some history."
            }
            """;

        var result = NoteAiResponseMapper.Parse(jsonWithoutOptionalSections);

        result.IsSuccess.Should().BeTrue();
        result.Value.Vocabulary.Should().BeEmpty();
        result.Value.Formulas.Should().BeEmpty();
        result.Value.Flashcards.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WhenResponseIsNotValidJson_ReturnsFailure()
    {
        var result = NoteAiResponseMapper.Parse("this is not json at all");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Parse_WhenCorrectOptionIndexIsOutOfRange_ClampsIntoValidRange()
    {
        const string jsonWithBadIndex = """
            {
              "summary": "s",
              "keyPoints": [],
              "flashcards": [],
              "mcqs": [
                { "question": "q", "options": ["a", "b", "c", "d"], "correctOptionIndex": 9, "explanation": "e" }
              ],
              "mindMap": { "topic": "t", "children": [] },
              "revisionSheet": "r"
            }
            """;

        var result = NoteAiResponseMapper.Parse(jsonWithBadIndex);

        result.IsSuccess.Should().BeTrue();
        result.Value.Mcqs[0].CorrectOptionIndex.Should().BeInRange(0, 3);
    }
}

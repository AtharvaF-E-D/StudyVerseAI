using FluentAssertions;
using StudyVerse.Application.Features.InterviewPrep.Common;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.Common;

public sealed class ResumeAnalysisResponseParserTests
{
    private const string RealisticSampleJson = """
        {
          "overallScore": 72,
          "strengths": [
            "Clear reverse-chronological structure that's easy to scan.",
            "Quantified impact in the most recent role (e.g. \"reduced latency by 30%\")."
          ],
          "weaknesses": [
            "The summary at the top is generic and doesn't mention a specific specialization.",
            "Several bullet points describe responsibilities rather than measurable outcomes."
          ],
          "suggestions": [
            "Rewrite the summary to name the specific domain/stack this candidate specializes in.",
            "Add a metric or outcome to each bullet point that currently just describes a duty.",
            "Move the skills section above older, less relevant roles."
          ]
        }
        """;

    [Fact]
    public void Parse_WellFormedResponse_ReturnsAllFields()
    {
        var result = ResumeAnalysisResponseParser.Parse(RealisticSampleJson);

        result.Should().NotBeNull();
        result!.OverallScore.Should().Be(72);
        result.Strengths.Should().HaveCount(2);
        result.Weaknesses.Should().HaveCount(2);
        result.Suggestions.Should().HaveCount(3);
    }

    [Fact]
    public void Parse_MoreThanFiveItemsInAList_CapsAtFive()
    {
        var items = string.Join(", ", Enumerable.Range(1, 8).Select(i => $"\"Item {i}\""));
        var json = $$"""{ "overallScore": 50, "strengths": [{{items}}], "weaknesses": [], "suggestions": [] }""";

        var result = ResumeAnalysisResponseParser.Parse(json);

        result!.Strengths.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(150)]
    [InlineData(-10)]
    public void Parse_OutOfRangeScore_ClampsToZeroToHundred(int rawScore)
    {
        var json = $$"""{ "overallScore": {{rawScore}}, "strengths": ["a"], "weaknesses": ["b"], "suggestions": ["c"] }""";

        var result = ResumeAnalysisResponseParser.Parse(json);

        result!.OverallScore.Should().BeInRange(0, 100);
    }

    [Fact]
    public void Parse_MalformedJson_ReturnsNullInsteadOfThrowing()
    {
        var result = ResumeAnalysisResponseParser.Parse("not valid json { [");

        result.Should().BeNull();
    }

    [Fact]
    public void Parse_EmptyListsEverywhere_ReturnsNull()
    {
        var result = ResumeAnalysisResponseParser.Parse("""{ "overallScore": 50, "strengths": [], "weaknesses": [], "suggestions": [] }""");

        result.Should().BeNull();
    }
}

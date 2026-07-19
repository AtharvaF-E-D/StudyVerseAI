using FluentAssertions;
using StudyVerse.Application.Features.InterviewPrep.Common;

namespace StudyVerse.Application.Tests.Features.InterviewPrep.Common;

public sealed class InterviewAnswerGradingResponseParserTests
{
    [Fact]
    public void Parse_WellFormedResponse_ReturnsScoreAndFeedback()
    {
        var result = InterviewAnswerGradingResponseParser.Parse(
            """{ "score": 7, "feedback": "Good structure but could use a more concrete outcome." }""");

        result.Score.Should().Be(7);
        result.Feedback.Should().Be("Good structure but could use a more concrete outcome.");
    }

    [Theory]
    [InlineData(15)]
    [InlineData(-3)]
    public void Parse_OutOfRangeScore_ClampsToZeroToTen(int rawScore)
    {
        var result = InterviewAnswerGradingResponseParser.Parse($$"""{ "score": {{rawScore}}, "feedback": "Feedback." }""");

        result.Score.Should().BeInRange(0, 10);
    }

    [Fact]
    public void Parse_MalformedJson_FallsBackSafelyInsteadOfThrowing()
    {
        var result = InterviewAnswerGradingResponseParser.Parse("not valid json at all { [");

        result.Score.Should().Be(0);
        result.Feedback.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Parse_MissingFeedback_FallsBackToADefaultMessage()
    {
        var result = InterviewAnswerGradingResponseParser.Parse("""{ "score": 5 }""");

        result.Score.Should().Be(5);
        result.Feedback.Should().NotBeNullOrWhiteSpace();
    }
}

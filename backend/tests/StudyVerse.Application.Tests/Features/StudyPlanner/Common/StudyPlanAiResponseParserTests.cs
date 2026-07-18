using FluentAssertions;
using StudyVerse.Application.Features.StudyPlanner.Common;

namespace StudyVerse.Application.Tests.Features.StudyPlanner.Common;

public sealed class StudyPlanAiResponseParserTests
{
    private static readonly DateOnly MinDate = new(2026, 7, 18);
    private static readonly DateOnly MaxDate = new(2026, 7, 25);

    [Fact]
    public void Parse_EntriesOutsideTheRequestedDateRange_AreFilteredOutNotClamped()
    {
        const string json = """
            {"tasks":[
                {"date":"2026-07-19","subject":"Math","topic":"InRange","durationMinutes":45,"isWeakTopic":false},
                {"date":"2026-07-10","subject":"Math","topic":"TooEarly","durationMinutes":30,"isWeakTopic":false},
                {"date":"2026-08-01","subject":"Math","topic":"TooLate","durationMinutes":30,"isWeakTopic":false}
            ]}
            """;

        var result = StudyPlanAiResponseParser.Parse(json, MinDate, MaxDate);

        result.Should().ContainSingle();
        result[0].Topic.Should().Be("InRange");
    }

    [Fact]
    public void Parse_DateEqualToEitherBoundary_IsKeptInclusive()
    {
        const string json = """
            {"tasks":[
                {"date":"2026-07-18","subject":"Math","topic":"MinBoundary","durationMinutes":30,"isWeakTopic":false},
                {"date":"2026-07-25","subject":"Math","topic":"MaxBoundary","durationMinutes":30,"isWeakTopic":false}
            ]}
            """;

        var result = StudyPlanAiResponseParser.Parse(json, MinDate, MaxDate);

        result.Should().HaveCount(2);
        result.Select(t => t.Topic).Should().BeEquivalentTo(["MinBoundary", "MaxBoundary"]);
    }

    [Fact]
    public void Parse_DurationBeyondSaneBounds_IsClampedRatherThanTrustedOutright()
    {
        const string json = """
            {"tasks":[
                {"date":"2026-07-19","subject":"Math","topic":"TooLong","durationMinutes":5000,"isWeakTopic":false},
                {"date":"2026-07-20","subject":"Math","topic":"NonPositive","durationMinutes":0,"isWeakTopic":false}
            ]}
            """;

        var result = StudyPlanAiResponseParser.Parse(json, MinDate, MaxDate);

        // The non-positive duration is dropped entirely (not clamped up to a fake positive value),
        // the too-long one is clamped down to the sane maximum.
        result.Should().ContainSingle();
        result[0].Topic.Should().Be("TooLong");
        result[0].DurationMinutes.Should().Be(480);
    }

    [Fact]
    public void Parse_EntryMissingSubjectOrTopic_IsDropped()
    {
        const string json = """
            {"tasks":[
                {"date":"2026-07-19","subject":"","topic":"Blank Subject","durationMinutes":30,"isWeakTopic":false},
                {"date":"2026-07-19","subject":"Math","topic":"Good","durationMinutes":30,"isWeakTopic":false}
            ]}
            """;

        var result = StudyPlanAiResponseParser.Parse(json, MinDate, MaxDate);

        result.Should().ContainSingle();
        result[0].Topic.Should().Be("Good");
    }

    [Fact]
    public void Parse_MalformedJson_ReturnsEmptyRatherThanThrowing()
    {
        var result = StudyPlanAiResponseParser.Parse("this is not json", MinDate, MaxDate);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_ResponseWithNoTasksKey_ReturnsEmpty()
    {
        var result = StudyPlanAiResponseParser.Parse("{}", MinDate, MaxDate);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Parse_UnparseableDate_IsDropped()
    {
        const string json = """
            {"tasks":[
                {"date":"not-a-date","subject":"Math","topic":"Bad","durationMinutes":30,"isWeakTopic":false},
                {"date":"2026-07-19","subject":"Math","topic":"Good","durationMinutes":30,"isWeakTopic":false}
            ]}
            """;

        var result = StudyPlanAiResponseParser.Parse(json, MinDate, MaxDate);

        result.Should().ContainSingle();
        result[0].Topic.Should().Be("Good");
    }
}

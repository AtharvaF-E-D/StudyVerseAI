using FluentAssertions;
using StudyVerse.Application.Features.MockTests.GetMockTestTemplates;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Tests.Features.MockTests.GetMockTestTemplates;

public sealed class GetMockTestTemplatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheFullFixedCatalog()
    {
        var result = await new GetMockTestTemplatesQueryHandler().Handle(new GetMockTestTemplatesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(MockTestCatalog.All.Count);
        result.Value.Select(t => t.Id).Should().BeEquivalentTo(MockTestCatalog.All.Select(t => t.Id));
    }
}

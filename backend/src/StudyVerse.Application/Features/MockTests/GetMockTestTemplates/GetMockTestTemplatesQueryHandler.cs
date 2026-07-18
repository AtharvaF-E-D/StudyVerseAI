using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.MockTests;

namespace StudyVerse.Application.Features.MockTests.GetMockTestTemplates;

public sealed class GetMockTestTemplatesQueryHandler
    : IRequestHandler<GetMockTestTemplatesQuery, Result<IReadOnlyList<MockTestTemplateDto>>>
{
    public Task<Result<IReadOnlyList<MockTestTemplateDto>>> Handle(
        GetMockTestTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var templates = MockTestCatalog.All
            .Select(t => new MockTestTemplateDto(t.Id, t.Title, t.Description, t.Category, t.QuestionCount, t.DurationMinutes))
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<MockTestTemplateDto>>(templates));
    }
}

using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.GetMockTestTemplates;

/// <summary>The complete, fixed catalog of mock test templates (see <c>MockTestCatalog</c>) - no per-user filtering.</summary>
public sealed record GetMockTestTemplatesQuery : IRequest<Result<IReadOnlyList<MockTestTemplateDto>>>;

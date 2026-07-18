using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempts;

/// <summary>The user's full mock test attempt history (both statuses), newest first - the data source for progress reports.</summary>
public sealed record GetMockTestAttemptsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<MockTestAttemptListItemDto>>>;

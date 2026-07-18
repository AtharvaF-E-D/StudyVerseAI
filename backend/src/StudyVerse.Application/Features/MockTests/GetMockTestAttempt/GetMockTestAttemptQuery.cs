using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.GetMockTestAttempt;

/// <summary>Ownership-checked lookup of a single mock test attempt - InProgress or Submitted - for viewing a past (or in-flight) result.</summary>
public sealed record GetMockTestAttemptQuery(Guid UserId, Guid AttemptId) : IRequest<Result<MockTestAttemptDetailDto>>;

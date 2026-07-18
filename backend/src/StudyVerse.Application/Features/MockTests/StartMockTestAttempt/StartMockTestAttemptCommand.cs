using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.StartMockTestAttempt;

public sealed record StartMockTestAttemptCommand(Guid UserId, Guid TemplateId)
    : IRequest<Result<StartMockTestAttemptResultDto>>;

public sealed record StartMockTestAttemptResultDto(
    Guid AttemptId,
    int DurationMinutes,
    DateTime StartedAtUtc,
    IReadOnlyList<MockTestQuestionOptionsDto> Questions);

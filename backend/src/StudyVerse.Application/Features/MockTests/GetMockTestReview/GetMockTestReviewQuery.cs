using MediatR;
using StudyVerse.Application.Features.MockTests.Common;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.MockTests.GetMockTestReview;

/// <summary>Only available for Submitted attempts: every question with the user's answer, the correct answer, and the explanation.</summary>
public sealed record GetMockTestReviewQuery(Guid UserId, Guid AttemptId) : IRequest<Result<MockTestReviewDto>>;

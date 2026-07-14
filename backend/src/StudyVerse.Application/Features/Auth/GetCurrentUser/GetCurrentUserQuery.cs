using MediatR;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.GetCurrentUser;

public sealed record GetCurrentUserQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

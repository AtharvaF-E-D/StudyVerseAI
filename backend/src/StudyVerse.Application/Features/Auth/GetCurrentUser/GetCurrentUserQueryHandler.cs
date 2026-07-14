using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Common.Models;
using StudyVerse.Domain.Common;

namespace StudyVerse.Application.Features.Auth.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserProfileDto>>
{
    private readonly IAppDbContext _db;
    private readonly IConfigurationProvider _mapperConfiguration;

    public GetCurrentUserQueryHandler(IAppDbContext db, IConfigurationProvider mapperConfiguration)
    {
        _db = db;
        _mapperConfiguration = mapperConfiguration;
    }

    public async Task<Result<UserProfileDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var profile = await _db.Users
            .Where(u => u.Id == request.UserId)
            .ProjectTo<UserProfileDto>(_mapperConfiguration)
            .FirstOrDefaultAsync(cancellationToken);

        return profile is null
            ? Result.Failure<UserProfileDto>("User not found.", ResultErrorType.NotFound)
            : Result.Success(profile);
    }
}

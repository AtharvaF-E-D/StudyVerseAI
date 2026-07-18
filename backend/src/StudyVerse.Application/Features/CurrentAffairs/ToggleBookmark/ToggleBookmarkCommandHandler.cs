using MediatR;
using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Application.Features.CurrentAffairs.Common;
using StudyVerse.Domain.Common;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Features.CurrentAffairs.ToggleBookmark;

public sealed class ToggleBookmarkCommandHandler : IRequestHandler<ToggleBookmarkCommand, Result<ToggleBookmarkResultDto>>
{
    private readonly IAppDbContext _db;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ToggleBookmarkCommandHandler(IAppDbContext db, IDateTimeProvider dateTimeProvider)
    {
        _db = db;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<ToggleBookmarkResultDto>> Handle(ToggleBookmarkCommand request, CancellationToken cancellationToken)
    {
        var articleExists = await _db.NewsArticles.AnyAsync(a => a.Id == request.ArticleId, cancellationToken);
        if (!articleExists)
        {
            return Result.Failure<ToggleBookmarkResultDto>("Article not found.", ResultErrorType.NotFound);
        }

        var existingBookmark = await _db.NewsBookmarks.FirstOrDefaultAsync(
            b => b.UserId == request.UserId && b.ArticleId == request.ArticleId, cancellationToken);

        bool isBookmarked;
        if (existingBookmark is not null)
        {
            _db.NewsBookmarks.Remove(existingBookmark);
            isBookmarked = false;
        }
        else
        {
            _db.NewsBookmarks.Add(new NewsBookmark
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ArticleId = request.ArticleId,
                CreatedAtUtc = _dateTimeProvider.UtcNow,
            });
            isBookmarked = true;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleBookmarkResultDto(request.ArticleId, isBookmarked));
    }
}

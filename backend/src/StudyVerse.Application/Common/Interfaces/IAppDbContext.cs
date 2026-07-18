using Microsoft.EntityFrameworkCore;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the EF Core <c>DbContext</c> so the Application layer can depend on
/// persistence without referencing Npgsql or any other provider-specific package.
/// </summary>
public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<OtpCode> OtpCodes { get; }

    DbSet<UserToken> UserTokens { get; }

    DbSet<UserProgress> UserProgresses { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<ChallengeCompletion> ChallengeCompletions { get; }

    DbSet<Conversation> Conversations { get; }

    DbSet<Message> Messages { get; }

    DbSet<QuizQuestion> QuizQuestions { get; }

    DbSet<QuizSession> QuizSessions { get; }

    DbSet<QuizSessionQuestion> QuizSessionQuestions { get; }

    DbSet<Note> Notes { get; }

    DbSet<NoteContent> NoteContents { get; }

    DbSet<FlashcardDeck> FlashcardDecks { get; }

    DbSet<Flashcard> Flashcards { get; }

    DbSet<MockTestAttempt> MockTestAttempts { get; }

    DbSet<MockTestAttemptAnswer> MockTestAttemptAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

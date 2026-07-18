using Microsoft.EntityFrameworkCore;
using StudyVerse.Application.Common.Interfaces;
using StudyVerse.Domain.Entities;

namespace StudyVerse.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();

    public DbSet<UserToken> UserTokens => Set<UserToken>();

    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<ChallengeCompletion> ChallengeCompletions => Set<ChallengeCompletion>();

    public DbSet<Conversation> Conversations => Set<Conversation>();

    public DbSet<Message> Messages => Set<Message>();

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    public DbSet<QuizSession> QuizSessions => Set<QuizSession>();

    public DbSet<QuizSessionQuestion> QuizSessionQuestions => Set<QuizSessionQuestion>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<NoteContent> NoteContents => Set<NoteContent>();

    public DbSet<FlashcardDeck> FlashcardDecks => Set<FlashcardDeck>();

    public DbSet<Flashcard> Flashcards => Set<Flashcard>();

    public DbSet<MockTestAttempt> MockTestAttempts => Set<MockTestAttempt>();

    public DbSet<MockTestAttemptAnswer> MockTestAttemptAnswers => Set<MockTestAttemptAnswer>();

    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();

    public DbSet<StudyPlanTask> StudyPlanTasks => Set<StudyPlanTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

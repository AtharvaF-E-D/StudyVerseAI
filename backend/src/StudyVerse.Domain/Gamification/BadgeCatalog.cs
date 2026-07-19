namespace StudyVerse.Domain.Gamification;

/// <summary>
/// A fixed, in-code badge definition. Purely descriptive metadata — the actual "has this user
/// earned it" criteria (checked against real activity across many tables) lives in the Application
/// layer's <c>BadgeEvaluationService</c>, since Domain cannot depend on <c>IAppDbContext</c>.
/// <see cref="Category"/> is a short grouping label for display (e.g. "Quiz", "Coding") and doubles
/// as one of the areas counted by the "Well Rounded" badge.
/// </summary>
public sealed record BadgeDefinition(Guid Id, string Title, string Description, string Category);

/// <summary>
/// The complete, fixed set of badge definitions. Ids are stable hardcoded GUIDs, the same pattern
/// <see cref="ChallengeCatalog"/> uses, so they stay consistent across app restarts and database
/// migrations — <see cref="Entities.UserBadge.BadgeId"/> references them by value.
///
/// Every criterion here is checkable against real, already-existing activity tables (Phases 3-12) —
/// no badge here is awarded for anything fabricated. See <c>BadgeEvaluationService</c> for the exact
/// query behind each one.
/// </summary>
public static class BadgeCatalog
{
    public static readonly Guid FirstStepsId = Guid.Parse("44444444-4444-4444-4444-444444440001");
    public static readonly Guid BookwormId = Guid.Parse("44444444-4444-4444-4444-444444440002");
    public static readonly Guid CodeWarriorId = Guid.Parse("44444444-4444-4444-4444-444444440003");
    public static readonly Guid ScholarId = Guid.Parse("44444444-4444-4444-4444-444444440004");
    public static readonly Guid ChatterboxId = Guid.Parse("44444444-4444-4444-4444-444444440005");
    public static readonly Guid PlannerId = Guid.Parse("44444444-4444-4444-4444-444444440006");
    public static readonly Guid NewsHoundId = Guid.Parse("44444444-4444-4444-4444-444444440007");
    public static readonly Guid InterviewReadyId = Guid.Parse("44444444-4444-4444-4444-444444440008");
    public static readonly Guid WeekWarriorId = Guid.Parse("44444444-4444-4444-4444-444444440009");
    public static readonly Guid QuizMasterId = Guid.Parse("44444444-4444-4444-4444-444444440010");
    public static readonly Guid CodeMasterId = Guid.Parse("44444444-4444-4444-4444-444444440011");
    public static readonly Guid WellRoundedId = Guid.Parse("44444444-4444-4444-4444-444444440012");

    public static readonly IReadOnlyList<BadgeDefinition> All =
    [
        new(FirstStepsId, "First Steps", "Complete your first Rapid Fire Quiz session.", "Quiz"),
        new(BookwormId, "Bookworm", "Create your first flashcard deck.", "Flashcards"),
        new(CodeWarriorId, "Code Warrior", "Get your first coding submission Accepted.", "Coding"),
        new(ScholarId, "Scholar", "Submit your first Mock Test attempt.", "Mock Tests"),
        new(ChatterboxId, "Chatterbox", "Have a real conversation with the AI Tutor.", "AI Tutor"),
        new(PlannerId, "Planner", "Create your first Study Plan.", "Study Planner"),
        new(NewsHoundId, "News Hound", "Bookmark your first Current Affairs article.", "Current Affairs"),
        new(InterviewReadyId, "Interview Ready", "Complete your first practice interview.", "Interview Prep"),
        new(WeekWarriorId, "Week Warrior", "Reach a 7-day activity streak.", "Streak"),
        new(QuizMasterId, "Quiz Master", "Complete 10 Rapid Fire Quiz sessions.", "Quiz"),
        new(CodeMasterId, "Code Master", "Get 10 distinct coding problems Accepted.", "Coding"),
        new(WellRoundedId, "Well Rounded", "Have real activity in 6 or more different features.", "General"),
    ];
}

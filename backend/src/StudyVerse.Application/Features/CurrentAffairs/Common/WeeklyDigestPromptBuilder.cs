using System.Text;

namespace StudyVerse.Application.Features.CurrentAffairs.Common;

/// <summary>One cached article's essentials, enough for the weekly digest prompt to reference
/// specifics without needing the full article body.</summary>
public sealed record WeeklyDigestArticleSummary(string Category, string Title, string? Description);

/// <summary>
/// Builds the single user-role prompt sent to <c>IAiChatProvider.GetCompletionAsync</c> for the
/// weekly digest - same reasoning as <c>NewsQuizPromptBuilder</c>/<c>MockTestWeaknessPromptBuilder</c>
/// for why the instructions live in one user-role message rather than a purpose-built system prompt.
/// </summary>
internal static class WeeklyDigestPromptBuilder
{
    public static string Build(DateOnly weekStartDate, IReadOnlyList<WeeklyDigestArticleSummary> articles)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            $"Below are news stories cached this week (starting Monday {weekStartDate:yyyy-MM-dd}), grouped by category.");
        sb.AppendLine();

        foreach (var group in articles.GroupBy(a => a.Category))
        {
            sb.AppendLine($"Category: {group.Key}");
            foreach (var article in group)
            {
                var suffix = string.IsNullOrWhiteSpace(article.Description) ? string.Empty : $": {article.Description}";
                sb.AppendLine($"- {article.Title}{suffix}");
            }

            sb.AppendLine();
        }

        sb.AppendLine(
            "Write a real 3-4 paragraph weekly digest summarizing the most significant stories above " +
            "across categories, for a student who wants to stay informed. Address the reader directly, " +
            "do not repeat or quote these instructions back, and do not fabricate any story not listed above.");

        return sb.ToString();
    }
}

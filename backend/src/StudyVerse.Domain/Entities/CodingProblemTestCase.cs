namespace StudyVerse.Domain.Entities;

/// <summary>
/// One stdin/stdout test case for a <see cref="CodingProblem"/>, graded by handing
/// <see cref="Input"/> and <see cref="ExpectedOutput"/> straight to Judge0 (which does the output
/// comparison itself — see <c>IJudge0Provider</c>). <see cref="IsSample"/> cases are shown to the
/// user as worked examples on the problem detail screen; the rest are graded against but never
/// leaked to the client (same anti-cheating reasoning quiz answers use) — see
/// <c>GetProblemQueryHandler</c> and <c>SubmitCodeCommandHandler</c>.
/// </summary>
public class CodingProblemTestCase
{
    public Guid Id { get; set; }

    public Guid ProblemId { get; set; }

    public string Input { get; set; } = string.Empty;

    public string ExpectedOutput { get; set; } = string.Empty;

    public bool IsSample { get; set; }

    public int OrderIndex { get; set; }

    public CodingProblem? Problem { get; set; }
}

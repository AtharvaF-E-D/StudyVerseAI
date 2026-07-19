namespace StudyVerse.Domain.Enums;

/// <summary>
/// The graded outcome of a <see cref="StudyVerse.Domain.Entities.CodeSubmission"/>, derived from
/// Judge0's per-test-case <c>status.description</c> values. <see cref="CompileError"/> and
/// <see cref="RuntimeError"/> short-circuit grading (see <c>SubmitCodeCommandHandler</c> — the
/// first test case that comes back with either of these stops the run rather than running the
/// remaining test cases). <see cref="Error"/> is StudyVerse's own status, never Judge0's: it means
/// Judge0 itself was unreachable/rate-limited/returned something unparseable, i.e. we never got a
/// real verdict at all — see <see cref="StudyVerse.Application.Common.Interfaces.IJudge0Provider"/>.
/// </summary>
public enum CodeSubmissionStatus
{
    Accepted = 0,
    WrongAnswer = 1,
    CompileError = 2,
    RuntimeError = 3,
    Error = 4,
}

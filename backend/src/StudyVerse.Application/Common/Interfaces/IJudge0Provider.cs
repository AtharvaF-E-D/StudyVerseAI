namespace StudyVerse.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the real Judge0 CE REST API (via RapidAPI), so the Application layer never
/// references <c>HttpClient</c>/Judge0's response shape directly (mirrors the
/// <see cref="IGNewsProvider"/>/<c>GNewsProvider</c> split: interface here, provider-specific
/// implementation in Infrastructure).
///
/// One call = one graded test case: Judge0 itself does the stdout-vs-<paramref name="expectedOutput"/>
/// comparison (via <c>expected_output</c> + <c>wait=true</c>), so the result already carries a
/// final <c>status.description</c> ("Accepted", "Wrong Answer", "Compilation Error", "Runtime Error
/// (...)", etc.) rather than raw output the caller would have to diff itself.
///
/// Best-effort like <see cref="IGNewsProvider"/>: if Judge0 itself is unreachable, rate-limited, or
/// returns something unparseable, the implementation logs and returns a result with
/// <see cref="Judge0ResultDto.Status"/> set to the sentinel <c>"Error"</c> rather than throwing -
/// a Judge0 outage must never 500 <c>SubmitCodeCommandHandler</c>'s whole request.
/// </summary>
public interface IJudge0Provider
{
    Task<Judge0ResultDto> RunAsync(
        int languageId,
        string sourceCode,
        string stdin,
        string expectedOutput,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// <see cref="Status"/> is Judge0's own <c>status.description</c> value verbatim (e.g. "Accepted",
/// "Wrong Answer", "Compilation Error") - or the StudyVerse-only sentinel <c>"Error"</c> when Judge0
/// itself couldn't be reached at all. <see cref="CompileOutput"/> is only populated on a compile
/// error; <see cref="Stderr"/> only on a runtime error.
/// </summary>
public sealed record Judge0ResultDto(string Status, string? Stdout, string? Stderr, string? CompileOutput);

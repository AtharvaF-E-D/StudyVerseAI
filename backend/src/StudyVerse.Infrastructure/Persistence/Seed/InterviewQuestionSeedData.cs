using StudyVerse.Domain.Entities;
using StudyVerse.Domain.Enums;

namespace StudyVerse.Infrastructure.Persistence.Seed;

/// <summary>
/// The static Interview Prep question bank, applied via <c>InterviewQuestionConfiguration.HasData</c>.
/// Ids are stable hardcoded GUIDs (pattern: <c>99999999-9999-9999-9999-{typeCode}{sequence:D11}</c>,
/// where <c>typeCode</c> is a 1-digit type code and <c>sequence</c> is an 11-digit per-type sequence
/// number) so the seed stays idempotent across migrations — the same reasoning as
/// <c>QuizQuestionSeedData</c>/<c>CodingProblemSeedData</c>'s static ids. 36 real, hand-written
/// questions: 12 per type (Hr/Technical/Behavioral), comfortably clearing the phase's ≥30 minimum.
/// Technical questions are deliberately generic/non-language-specific.
/// </summary>
public static class InterviewQuestionSeedData
{
    // Fixed (not DateTime.UtcNow) because EF Core HasData values must be static/deterministic —
    // a changing CreatedAtUtc would produce a spurious migration diff on every `migrations add`.
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private const string HrCode = "1";
    private const string TechnicalCode = "2";
    private const string BehavioralCode = "3";

    public static IReadOnlyList<InterviewQuestion> All { get; } = BuildAll();

    private static InterviewQuestion Q(string typeCode, int sequence, InterviewQuestionType type, string questionText, string whatGoodAnswersCover) => new()
    {
        Id = Guid.Parse($"99999999-9999-9999-9999-{typeCode}{sequence:D11}"),
        Type = type,
        QuestionText = questionText,
        WhatGoodAnswersCover = whatGoodAnswersCover,
        CreatedAtUtc = SeedTimestamp,
    };

    private static List<InterviewQuestion> BuildAll()
    {
        var list = new List<InterviewQuestion>();

        // ---------------- HR ----------------
        list.Add(Q(HrCode, 1, InterviewQuestionType.Hr,
            "Why do you want to work here?",
            "Specific, researched reasons tied to the company's mission/products/team (not generic " +
            "'good opportunity' filler), and a genuine link between the candidate's own goals and " +
            "what the role offers."));
        list.Add(Q(HrCode, 2, InterviewQuestionType.Hr,
            "What are your salary expectations?",
            "A realistic, researched range (not a refusal to answer or an arbitrary number), framed " +
            "with some flexibility and openness to discussing the full compensation package."));
        list.Add(Q(HrCode, 3, InterviewQuestionType.Hr,
            "Tell me about yourself.",
            "A concise professional narrative (not a full life story): relevant background, a couple " +
            "of concrete achievements, and why that history leads naturally to this role."));
        list.Add(Q(HrCode, 4, InterviewQuestionType.Hr,
            "Why are you leaving your current job?",
            "A positive, forward-looking reason (growth, new challenge, alignment with goals) rather " +
            "than bad-mouthing a current/former employer or manager."));
        list.Add(Q(HrCode, 5, InterviewQuestionType.Hr,
            "Where do you see yourself in five years?",
            "A plausible growth trajectory that's realistic for the industry/role and shows the " +
            "candidate has actually thought about their career, ideally connected to growing within " +
            "this company or field."));
        list.Add(Q(HrCode, 6, InterviewQuestionType.Hr,
            "What are your greatest strengths?",
            "One or two specific strengths relevant to the role, backed by a concrete example of them " +
            "being applied — not just a list of adjectives."));
        list.Add(Q(HrCode, 7, InterviewQuestionType.Hr,
            "What is your biggest weakness?",
            "A genuine, real weakness (not a humble-brag disguised as a weakness like 'I work too " +
            "hard') paired with a concrete step being taken to actively improve on it."));
        list.Add(Q(HrCode, 8, InterviewQuestionType.Hr,
            "Why should we hire you?",
            "A clear, specific match between the candidate's skills/experience and the role's actual " +
            "requirements, ideally with a concrete example demonstrating that fit."));
        list.Add(Q(HrCode, 9, InterviewQuestionType.Hr,
            "What motivates you at work?",
            "An authentic, specific motivator (learning, impact, solving hard problems, collaboration, " +
            "etc.) illustrated with a real example, not a vague platitude."));
        list.Add(Q(HrCode, 10, InterviewQuestionType.Hr,
            "How do you handle stress and pressure?",
            "A concrete coping strategy (prioritization, breaking work down, communication) illustrated " +
            "with a real high-pressure situation the candidate actually navigated successfully."));
        list.Add(Q(HrCode, 11, InterviewQuestionType.Hr,
            "Are you willing to relocate or travel for this role?",
            "A direct, honest answer with any real constraints stated clearly, plus enough context " +
            "(family situation, notice period, etc. only if volunteered) to show it was actually " +
            "thought through rather than dodged."));
        list.Add(Q(HrCode, 12, InterviewQuestionType.Hr,
            "Do you have any questions for us?",
            "At least one or two thoughtful, specific questions about the team, role, or company " +
            "(not 'no, I think you covered everything') that show genuine engagement and curiosity."));

        // ---------------- Technical (generic, non-language-specific) ----------------
        list.Add(Q(TechnicalCode, 1, InterviewQuestionType.Technical,
            "Explain the difference between a process and a thread.",
            "A process has its own isolated memory space and resources; threads are units of " +
            "execution within a process that share that process's memory. Should mention the " +
            "overhead/isolation trade-off and that threads within one process can communicate more " +
            "cheaply but risk shared-state bugs (race conditions)."));
        list.Add(Q(TechnicalCode, 2, InterviewQuestionType.Technical,
            "What is the time complexity of binary search and why?",
            "O(log n), because each comparison halves the remaining search space; requires the input " +
            "to be sorted. A strong answer explains the halving intuition, not just states the answer."));
        list.Add(Q(TechnicalCode, 3, InterviewQuestionType.Technical,
            "What is the difference between an array and a linked list?",
            "Arrays: contiguous memory, O(1) index access, costly insert/delete in the middle. Linked " +
            "lists: nodes scattered in memory with pointers, O(n) access but O(1) insert/delete given " +
            "a reference to the node. Should mention cache locality/memory overhead trade-offs."));
        list.Add(Q(TechnicalCode, 4, InterviewQuestionType.Technical,
            "Explain what a hash table is and how it handles collisions.",
            "Maps keys to array slots via a hash function for average O(1) lookup/insert; collisions " +
            "(two keys hashing to the same slot) are handled via chaining (linked lists/buckets per " +
            "slot) or open addressing (probing for the next free slot). Should mention amortized vs. " +
            "worst-case complexity."));
        list.Add(Q(TechnicalCode, 5, InterviewQuestionType.Technical,
            "What is the difference between stack and queue data structures?",
            "Stack: LIFO (last in, first out) — push/pop from the same end. Queue: FIFO (first in, " +
            "first out) — enqueue at one end, dequeue at the other. Bonus for real-world examples " +
            "(call stack/undo for stacks; task scheduling/print queues for queues)."));
        list.Add(Q(TechnicalCode, 6, InterviewQuestionType.Technical,
            "Explain the concept of Big O notation and why it matters.",
            "Describes how an algorithm's runtime/memory scales with input size in the worst case, " +
            "abstracting away constants/hardware specifics. Matters because it lets you compare " +
            "algorithms' scalability and predict performance on large inputs before running them."));
        list.Add(Q(TechnicalCode, 7, InterviewQuestionType.Technical,
            "What is the difference between SQL and NoSQL databases?",
            "SQL: relational, fixed schema, tables with rows/columns, strong consistency (ACID), joins. " +
            "NoSQL: flexible/schema-less (document, key-value, column, graph stores), built for " +
            "horizontal scale and often eventual consistency. Should mention when each is a better fit."));
        list.Add(Q(TechnicalCode, 8, InterviewQuestionType.Technical,
            "Explain what an API is and how REST APIs work.",
            "An API is a contract letting software components communicate. REST APIs use HTTP verbs " +
            "(GET/POST/PUT/DELETE) against resource-oriented URLs, are stateless per-request, and " +
            "typically exchange JSON. Bonus for mentioning status codes or statelessness explicitly."));
        list.Add(Q(TechnicalCode, 9, InterviewQuestionType.Technical,
            "What is the difference between synchronous and asynchronous programming?",
            "Synchronous: operations block and run one after another in order. Asynchronous: an " +
            "operation can start and let other work proceed while it completes in the background, " +
            "notifying via callback/promise/await when done. Should mention why this matters for I/O-bound work."));
        list.Add(Q(TechnicalCode, 10, InterviewQuestionType.Technical,
            "Explain what recursion is and give an example of when to use it.",
            "A function that calls itself with a smaller version of the problem until reaching a base " +
            "case. Good examples: factorial, tree/graph traversal, divide-and-conquer algorithms. " +
            "Bonus for mentioning stack depth/overflow risk and when iteration is preferable."));
        list.Add(Q(TechnicalCode, 11, InterviewQuestionType.Technical,
            "What is the difference between compiled and interpreted languages?",
            "Compiled languages are translated to machine code ahead of time by a compiler (faster " +
            "execution, separate build step); interpreted languages are executed line-by-line at " +
            "runtime by an interpreter (slower but more portable/flexible). Bonus for mentioning " +
            "JIT compilation as a middle ground."));
        list.Add(Q(TechnicalCode, 12, InterviewQuestionType.Technical,
            "Explain the concept of normalization in relational databases.",
            "The process of organizing tables/columns to reduce data redundancy and improve integrity, " +
            "typically by splitting data into related tables linked by keys (1NF/2NF/3NF). Should " +
            "mention the trade-off against query performance/denormalization for read-heavy workloads."));

        // ---------------- Behavioral ----------------
        list.Add(Q(BehavioralCode, 1, InterviewQuestionType.Behavioral,
            "Tell me about a time you disagreed with a teammate — how did you handle it?",
            "A specific real situation, the disagreement's substance, how it was resolved " +
            "constructively (listening, compromise, data/evidence), and a positive outcome or lesson " +
            "learned — using the STAR (Situation/Task/Action/Result) structure."));
        list.Add(Q(BehavioralCode, 2, InterviewQuestionType.Behavioral,
            "Describe a project you're proud of.",
            "A concrete project with the candidate's specific role/contribution, the challenge " +
            "involved, and a measurable or clearly-described positive result — not just 'it went well'."));
        list.Add(Q(BehavioralCode, 3, InterviewQuestionType.Behavioral,
            "Tell me about a time you failed and what you learned from it.",
            "Honest ownership of a real failure (not deflecting blame), a clear explanation of what " +
            "went wrong, and a specific, applied lesson or changed behavior since then."));
        list.Add(Q(BehavioralCode, 4, InterviewQuestionType.Behavioral,
            "Describe a situation where you had to meet a tight deadline.",
            "A specific deadline scenario, the plan/prioritization used to meet it (not just working " +
            "longer hours), and the outcome — bonus for mentioning trade-offs made under pressure."));
        list.Add(Q(BehavioralCode, 5, InterviewQuestionType.Behavioral,
            "Tell me about a time you had to learn something new quickly.",
            "A real example of unfamiliar territory, the concrete approach taken to get up to speed " +
            "(resources used, practice, asking for help), and how it was successfully applied."));
        list.Add(Q(BehavioralCode, 6, InterviewQuestionType.Behavioral,
            "Describe a time you received difficult feedback — how did you respond?",
            "Genuine acknowledgment of the feedback without defensiveness, a specific example of the " +
            "feedback itself, and a concrete change in behavior/output that followed."));
        list.Add(Q(BehavioralCode, 7, InterviewQuestionType.Behavioral,
            "Tell me about a time you had to work with a difficult team member.",
            "A specific, professional description of the difficulty (not a rant), the steps taken to " +
            "improve the working relationship or outcome, and a resolution or lesson learned."));
        list.Add(Q(BehavioralCode, 8, InterviewQuestionType.Behavioral,
            "Describe a situation where you took initiative without being asked.",
            "A concrete example of spotting a gap/opportunity and acting on it proactively, the " +
            "specific action taken, and the resulting impact or outcome."));
        list.Add(Q(BehavioralCode, 9, InterviewQuestionType.Behavioral,
            "Tell me about a time you had to prioritize multiple competing tasks.",
            "A real scenario with genuinely competing priorities, the method used to decide what came " +
            "first (urgency/impact/stakeholder input), and how it played out."));
        list.Add(Q(BehavioralCode, 10, InterviewQuestionType.Behavioral,
            "Describe a time you made a mistake at work — what did you do?",
            "Honest ownership of a specific real mistake, the immediate steps taken to fix/mitigate " +
            "it, and what changed afterward to prevent it recurring."));
        list.Add(Q(BehavioralCode, 11, InterviewQuestionType.Behavioral,
            "Tell me about a time you had to persuade someone to see things your way.",
            "A specific disagreement or differing viewpoint, the concrete persuasion approach used " +
            "(evidence, empathy, framing), and the outcome — ideally a mutually acceptable result."));
        list.Add(Q(BehavioralCode, 12, InterviewQuestionType.Behavioral,
            "Describe a time you went above and beyond for a project or customer.",
            "A specific instance of exceeding what was strictly required, the reasoning behind doing " +
            "so, and the concrete positive impact it had."));

        return list;
    }
}

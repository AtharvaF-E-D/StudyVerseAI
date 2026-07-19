using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewPrepFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "interview_questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    QuestionText = table.Column<string>(type: "text", nullable: false),
                    WhatGoodAnswersCover = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "interview_sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SelectedQuestionIdsJson = table.Column<string>(type: "jsonb", nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: true),
                    ImprovementPlan = table.Column<string>(type: "text", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interview_sessions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resume_analyses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    StoredFilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OverallScore = table.Column<int>(type: "integer", nullable: false),
                    StrengthsJson = table.Column<string>(type: "jsonb", nullable: false),
                    WeaknessesJson = table.Column<string>(type: "jsonb", nullable: false),
                    SuggestionsJson = table.Column<string>(type: "jsonb", nullable: false),
                    AnalyzedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resume_analyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_resume_analyses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "interview_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: false),
                    AiScore = table.Column<int>(type: "integer", nullable: true),
                    AiFeedback = table.Column<string>(type: "text", nullable: true),
                    AnsweredAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interview_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interview_answers_interview_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "interview_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_interview_answers_interview_sessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "interview_sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "interview_questions",
                columns: new[] { "Id", "CreatedAtUtc", "QuestionText", "Type", "WhatGoodAnswersCover" },
                values: new object[,]
                {
                    { new Guid("99999999-9999-9999-9999-100000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Why do you want to work here?", "Hr", "Specific, researched reasons tied to the company's mission/products/team (not generic 'good opportunity' filler), and a genuine link between the candidate's own goals and what the role offers." },
                    { new Guid("99999999-9999-9999-9999-100000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What are your salary expectations?", "Hr", "A realistic, researched range (not a refusal to answer or an arbitrary number), framed with some flexibility and openness to discussing the full compensation package." },
                    { new Guid("99999999-9999-9999-9999-100000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about yourself.", "Hr", "A concise professional narrative (not a full life story): relevant background, a couple of concrete achievements, and why that history leads naturally to this role." },
                    { new Guid("99999999-9999-9999-9999-100000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Why are you leaving your current job?", "Hr", "A positive, forward-looking reason (growth, new challenge, alignment with goals) rather than bad-mouthing a current/former employer or manager." },
                    { new Guid("99999999-9999-9999-9999-100000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Where do you see yourself in five years?", "Hr", "A plausible growth trajectory that's realistic for the industry/role and shows the candidate has actually thought about their career, ideally connected to growing within this company or field." },
                    { new Guid("99999999-9999-9999-9999-100000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What are your greatest strengths?", "Hr", "One or two specific strengths relevant to the role, backed by a concrete example of them being applied — not just a list of adjectives." },
                    { new Guid("99999999-9999-9999-9999-100000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is your biggest weakness?", "Hr", "A genuine, real weakness (not a humble-brag disguised as a weakness like 'I work too hard') paired with a concrete step being taken to actively improve on it." },
                    { new Guid("99999999-9999-9999-9999-100000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Why should we hire you?", "Hr", "A clear, specific match between the candidate's skills/experience and the role's actual requirements, ideally with a concrete example demonstrating that fit." },
                    { new Guid("99999999-9999-9999-9999-100000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What motivates you at work?", "Hr", "An authentic, specific motivator (learning, impact, solving hard problems, collaboration, etc.) illustrated with a real example, not a vague platitude." },
                    { new Guid("99999999-9999-9999-9999-100000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "How do you handle stress and pressure?", "Hr", "A concrete coping strategy (prioritization, breaking work down, communication) illustrated with a real high-pressure situation the candidate actually navigated successfully." },
                    { new Guid("99999999-9999-9999-9999-100000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Are you willing to relocate or travel for this role?", "Hr", "A direct, honest answer with any real constraints stated clearly, plus enough context (family situation, notice period, etc. only if volunteered) to show it was actually thought through rather than dodged." },
                    { new Guid("99999999-9999-9999-9999-100000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Do you have any questions for us?", "Hr", "At least one or two thoughtful, specific questions about the team, role, or company (not 'no, I think you covered everything') that show genuine engagement and curiosity." },
                    { new Guid("99999999-9999-9999-9999-200000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain the difference between a process and a thread.", "Technical", "A process has its own isolated memory space and resources; threads are units of execution within a process that share that process's memory. Should mention the overhead/isolation trade-off and that threads within one process can communicate more cheaply but risk shared-state bugs (race conditions)." },
                    { new Guid("99999999-9999-9999-9999-200000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the time complexity of binary search and why?", "Technical", "O(log n), because each comparison halves the remaining search space; requires the input to be sorted. A strong answer explains the halving intuition, not just states the answer." },
                    { new Guid("99999999-9999-9999-9999-200000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the difference between an array and a linked list?", "Technical", "Arrays: contiguous memory, O(1) index access, costly insert/delete in the middle. Linked lists: nodes scattered in memory with pointers, O(n) access but O(1) insert/delete given a reference to the node. Should mention cache locality/memory overhead trade-offs." },
                    { new Guid("99999999-9999-9999-9999-200000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain what a hash table is and how it handles collisions.", "Technical", "Maps keys to array slots via a hash function for average O(1) lookup/insert; collisions (two keys hashing to the same slot) are handled via chaining (linked lists/buckets per slot) or open addressing (probing for the next free slot). Should mention amortized vs. worst-case complexity." },
                    { new Guid("99999999-9999-9999-9999-200000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the difference between stack and queue data structures?", "Technical", "Stack: LIFO (last in, first out) — push/pop from the same end. Queue: FIFO (first in, first out) — enqueue at one end, dequeue at the other. Bonus for real-world examples (call stack/undo for stacks; task scheduling/print queues for queues)." },
                    { new Guid("99999999-9999-9999-9999-200000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain the concept of Big O notation and why it matters.", "Technical", "Describes how an algorithm's runtime/memory scales with input size in the worst case, abstracting away constants/hardware specifics. Matters because it lets you compare algorithms' scalability and predict performance on large inputs before running them." },
                    { new Guid("99999999-9999-9999-9999-200000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the difference between SQL and NoSQL databases?", "Technical", "SQL: relational, fixed schema, tables with rows/columns, strong consistency (ACID), joins. NoSQL: flexible/schema-less (document, key-value, column, graph stores), built for horizontal scale and often eventual consistency. Should mention when each is a better fit." },
                    { new Guid("99999999-9999-9999-9999-200000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain what an API is and how REST APIs work.", "Technical", "An API is a contract letting software components communicate. REST APIs use HTTP verbs (GET/POST/PUT/DELETE) against resource-oriented URLs, are stateless per-request, and typically exchange JSON. Bonus for mentioning status codes or statelessness explicitly." },
                    { new Guid("99999999-9999-9999-9999-200000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the difference between synchronous and asynchronous programming?", "Technical", "Synchronous: operations block and run one after another in order. Asynchronous: an operation can start and let other work proceed while it completes in the background, notifying via callback/promise/await when done. Should mention why this matters for I/O-bound work." },
                    { new Guid("99999999-9999-9999-9999-200000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain what recursion is and give an example of when to use it.", "Technical", "A function that calls itself with a smaller version of the problem until reaching a base case. Good examples: factorial, tree/graph traversal, divide-and-conquer algorithms. Bonus for mentioning stack depth/overflow risk and when iteration is preferable." },
                    { new Guid("99999999-9999-9999-9999-200000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "What is the difference between compiled and interpreted languages?", "Technical", "Compiled languages are translated to machine code ahead of time by a compiler (faster execution, separate build step); interpreted languages are executed line-by-line at runtime by an interpreter (slower but more portable/flexible). Bonus for mentioning JIT compilation as a middle ground." },
                    { new Guid("99999999-9999-9999-9999-200000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Explain the concept of normalization in relational databases.", "Technical", "The process of organizing tables/columns to reduce data redundancy and improve integrity, typically by splitting data into related tables linked by keys (1NF/2NF/3NF). Should mention the trade-off against query performance/denormalization for read-heavy workloads." },
                    { new Guid("99999999-9999-9999-9999-300000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you disagreed with a teammate — how did you handle it?", "Behavioral", "A specific real situation, the disagreement's substance, how it was resolved constructively (listening, compromise, data/evidence), and a positive outcome or lesson learned — using the STAR (Situation/Task/Action/Result) structure." },
                    { new Guid("99999999-9999-9999-9999-300000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a project you're proud of.", "Behavioral", "A concrete project with the candidate's specific role/contribution, the challenge involved, and a measurable or clearly-described positive result — not just 'it went well'." },
                    { new Guid("99999999-9999-9999-9999-300000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you failed and what you learned from it.", "Behavioral", "Honest ownership of a real failure (not deflecting blame), a clear explanation of what went wrong, and a specific, applied lesson or changed behavior since then." },
                    { new Guid("99999999-9999-9999-9999-300000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a situation where you had to meet a tight deadline.", "Behavioral", "A specific deadline scenario, the plan/prioritization used to meet it (not just working longer hours), and the outcome — bonus for mentioning trade-offs made under pressure." },
                    { new Guid("99999999-9999-9999-9999-300000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you had to learn something new quickly.", "Behavioral", "A real example of unfamiliar territory, the concrete approach taken to get up to speed (resources used, practice, asking for help), and how it was successfully applied." },
                    { new Guid("99999999-9999-9999-9999-300000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a time you received difficult feedback — how did you respond?", "Behavioral", "Genuine acknowledgment of the feedback without defensiveness, a specific example of the feedback itself, and a concrete change in behavior/output that followed." },
                    { new Guid("99999999-9999-9999-9999-300000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you had to work with a difficult team member.", "Behavioral", "A specific, professional description of the difficulty (not a rant), the steps taken to improve the working relationship or outcome, and a resolution or lesson learned." },
                    { new Guid("99999999-9999-9999-9999-300000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a situation where you took initiative without being asked.", "Behavioral", "A concrete example of spotting a gap/opportunity and acting on it proactively, the specific action taken, and the resulting impact or outcome." },
                    { new Guid("99999999-9999-9999-9999-300000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you had to prioritize multiple competing tasks.", "Behavioral", "A real scenario with genuinely competing priorities, the method used to decide what came first (urgency/impact/stakeholder input), and how it played out." },
                    { new Guid("99999999-9999-9999-9999-300000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a time you made a mistake at work — what did you do?", "Behavioral", "Honest ownership of a specific real mistake, the immediate steps taken to fix/mitigate it, and what changed afterward to prevent it recurring." },
                    { new Guid("99999999-9999-9999-9999-300000000011"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tell me about a time you had to persuade someone to see things your way.", "Behavioral", "A specific disagreement or differing viewpoint, the concrete persuasion approach used (evidence, empathy, framing), and the outcome — ideally a mutually acceptable result." },
                    { new Guid("99999999-9999-9999-9999-300000000012"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Describe a time you went above and beyond for a project or customer.", "Behavioral", "A specific instance of exceeding what was strictly required, the reasoning behind doing so, and the concrete positive impact it had." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_interview_answers_QuestionId",
                table: "interview_answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_interview_answers_SessionId_QuestionId",
                table: "interview_answers",
                columns: new[] { "SessionId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_interview_questions_Type",
                table: "interview_questions",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_interview_sessions_UserId_StartedAtUtc",
                table: "interview_sessions",
                columns: new[] { "UserId", "StartedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_resume_analyses_UserId_AnalyzedAtUtc",
                table: "resume_analyses",
                columns: new[] { "UserId", "AnalyzedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "interview_answers");

            migrationBuilder.DropTable(
                name: "resume_analyses");

            migrationBuilder.DropTable(
                name: "interview_questions");

            migrationBuilder.DropTable(
                name: "interview_sessions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMockTestsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mock_test_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    SubmittedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: true),
                    CorrectCount = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestions = table.Column<int>(type: "integer", nullable: false),
                    PercentileRank = table.Column<double>(type: "double precision", nullable: true),
                    AiWeaknessAnalysis = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mock_test_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mock_test_attempts_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "mock_test_attempt_answers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    SelectedOptionIndex = table.Column<int>(type: "integer", nullable: true),
                    IsCorrect = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mock_test_attempt_answers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mock_test_attempt_answers_mock_test_attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "mock_test_attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_mock_test_attempt_answers_quiz_questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "quiz_questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_mock_test_attempt_answers_AttemptId_OrderIndex",
                table: "mock_test_attempt_answers",
                columns: new[] { "AttemptId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mock_test_attempt_answers_QuestionId",
                table: "mock_test_attempt_answers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_mock_test_attempts_TemplateId_Status",
                table: "mock_test_attempts",
                columns: new[] { "TemplateId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_mock_test_attempts_UserId_StartedAtUtc",
                table: "mock_test_attempts",
                columns: new[] { "UserId", "StartedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mock_test_attempt_answers");

            migrationBuilder.DropTable(
                name: "mock_test_attempts");
        }
    }
}

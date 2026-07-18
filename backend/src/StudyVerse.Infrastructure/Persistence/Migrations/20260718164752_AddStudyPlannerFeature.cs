using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyPlannerFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "study_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamDate = table.Column<DateOnly>(type: "date", nullable: false),
                    SubjectsJson = table.Column<string>(type: "text", nullable: false),
                    WeakTopicsJson = table.Column<string>(type: "text", nullable: false),
                    HoursPerDayMinutes = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plans_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_plan_tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Topic = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsWeakTopic = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    OriginalScheduledDateUtc = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plan_tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plan_tasks_study_plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "study_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_plan_tasks_PlanId_ScheduledDateUtc",
                table: "study_plan_tasks",
                columns: new[] { "PlanId", "ScheduledDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_UserId_Status",
                table: "study_plans",
                columns: new[] { "UserId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_plan_tasks");

            migrationBuilder.DropTable(
                name: "study_plans");
        }
    }
}

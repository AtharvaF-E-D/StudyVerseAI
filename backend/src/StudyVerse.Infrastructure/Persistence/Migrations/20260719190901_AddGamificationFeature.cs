using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_reward_claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    ConsecutiveDayNumber = table.Column<int>(type: "integer", nullable: false),
                    CoinsAwarded = table.Column<int>(type: "integer", nullable: false),
                    XpAwarded = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reward_claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_daily_reward_claims_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "spin_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpinDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    SpunAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    PrizeLabel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CoinsAwarded = table.Column<int>(type: "integer", nullable: false),
                    XpAwarded = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_spin_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_spin_results_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_badges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BadgeId = table.Column<Guid>(type: "uuid", nullable: false),
                    EarnedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_badges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_badges_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_mission_progresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MissionTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    CurrentCount = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_mission_progresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_mission_progresses_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_daily_reward_claims_UserId_ClaimDateUtc",
                table: "daily_reward_claims",
                columns: new[] { "UserId", "ClaimDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_spin_results_UserId_SpinDateUtc",
                table: "spin_results",
                columns: new[] { "UserId", "SpinDateUtc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_badges_UserId_BadgeId",
                table: "user_badges",
                columns: new[] { "UserId", "BadgeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_mission_progresses_UserId_MissionTemplateId_WeekStartD~",
                table: "user_mission_progresses",
                columns: new[] { "UserId", "MissionTemplateId", "WeekStartDateUtc" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reward_claims");

            migrationBuilder.DropTable(
                name: "spin_results");

            migrationBuilder.DropTable(
                name: "user_badges");

            migrationBuilder.DropTable(
                name: "user_mission_progresses");
        }
    }
}

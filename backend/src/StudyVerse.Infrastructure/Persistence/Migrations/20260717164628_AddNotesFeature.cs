using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceFileName = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    SourceFileType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StorageKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notes_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "note_contents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    KeyPointsJson = table.Column<string>(type: "text", nullable: false),
                    FlashcardsJson = table.Column<string>(type: "text", nullable: false),
                    McqsJson = table.Column<string>(type: "text", nullable: false),
                    MindMapJson = table.Column<string>(type: "text", nullable: false),
                    RevisionSheet = table.Column<string>(type: "text", nullable: false),
                    VocabularyJson = table.Column<string>(type: "text", nullable: false),
                    FormulasJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_note_contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_note_contents_notes_NoteId",
                        column: x => x.NoteId,
                        principalTable: "notes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_note_contents_NoteId",
                table: "note_contents",
                column: "NoteId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notes_UserId_CreatedAtUtc",
                table: "notes",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "note_contents");

            migrationBuilder.DropTable(
                name: "notes");
        }
    }
}

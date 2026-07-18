using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudyVerse.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentAffairsFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_articles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PublishedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    FetchedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_articles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_digests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekStartDateUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    SummaryText = table.Column<string>(type: "text", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_digests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "news_article_quizzes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionsJson = table.Column<string>(type: "text", nullable: false),
                    GeneratedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_article_quizzes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_article_quizzes_news_articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "news_articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "news_bookmarks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_bookmarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_news_bookmarks_news_articles_ArticleId",
                        column: x => x.ArticleId,
                        principalTable: "news_articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_news_bookmarks_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_news_article_quizzes_ArticleId",
                table: "news_article_quizzes",
                column: "ArticleId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_articles_Category_FetchedAtUtc",
                table: "news_articles",
                columns: new[] { "Category", "FetchedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_news_articles_Category_PublishedAtUtc",
                table: "news_articles",
                columns: new[] { "Category", "PublishedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_news_articles_ExternalId",
                table: "news_articles",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_bookmarks_ArticleId",
                table: "news_bookmarks",
                column: "ArticleId");

            migrationBuilder.CreateIndex(
                name: "IX_news_bookmarks_UserId_ArticleId",
                table: "news_bookmarks",
                columns: new[] { "UserId", "ArticleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_digests_WeekStartDateUtc",
                table: "weekly_digests",
                column: "WeekStartDateUtc",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "news_article_quizzes");

            migrationBuilder.DropTable(
                name: "news_bookmarks");

            migrationBuilder.DropTable(
                name: "weekly_digests");

            migrationBuilder.DropTable(
                name: "news_articles");
        }
    }
}

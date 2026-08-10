using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDustLibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPostSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Posts",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Backfill pre-existing rows with a unique placeholder before the
            // unique index below can be created — every row currently has the
            // same "" default, which would violate uniqueness immediately.
            // See docs/superpowers/specs/2026-08-10-blog-public-api-design.md's
            // "Migration backfill" section: this only ever touches rows that
            // existed before this migration ran; anything created afterward
            // (including a freshly seeded environment) gets a real
            // slugify(title)-based slug from application code instead.
            migrationBuilder.Sql(
                "UPDATE \"Posts\" SET \"Slug\" = 'post-' || substring(replace(\"Id\"::text, '-', ''), 1, 12) WHERE \"Slug\" = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Slug",
                table: "Posts",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Posts_Slug",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Posts");
        }
    }
}

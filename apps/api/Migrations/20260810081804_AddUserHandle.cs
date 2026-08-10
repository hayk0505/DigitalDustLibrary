using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDustLibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserHandle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Handle",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Same backfill reasoning as AddPostSlug — see
            // docs/superpowers/specs/2026-08-10-blog-public-api-design.md.
            migrationBuilder.Sql(
                "UPDATE \"Users\" SET \"Handle\" = 'user-' || substring(replace(\"Id\"::text, '-', ''), 1, 12) WHERE \"Handle\" = '';");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Handle",
                table: "Users",
                column: "Handle",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Handle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Handle",
                table: "Users");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDustLibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPostFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Format",
                table: "Posts",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Format",
                table: "Posts");
        }
    }
}

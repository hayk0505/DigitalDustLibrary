using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDustLibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakeActivityLogActorNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLog_Users_ActorId",
                table: "ActivityLog");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActorId",
                table: "ActivityLog",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLog_Users_ActorId",
                table: "ActivityLog",
                column: "ActorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        // NOT SAFE to run once the hard-delete-user feature has actually been used in
        // production: this backfills any NULL ActorId row to Guid.Empty before restoring
        // the NOT NULL + Restrict constraint, but no User row ever has Id = Guid.Empty, so
        // the AddForeignKey below will fail against real NULL-actor rows left behind by a
        // deleted user (and there's no way to reconstruct who the original actor was).
        // Failing loudly here is intentional/acceptable — better than inventing a fake actor.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActivityLog_Users_ActorId",
                table: "ActivityLog");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActorId",
                table: "ActivityLog",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActivityLog_Users_ActorId",
                table: "ActivityLog",
                column: "ActorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

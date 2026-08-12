using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalDustLibrary.Api.Migrations
{
    /// <inheritdoc />
    public partial class CategoryTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. New Category columns first (existing rows get "" defaults
            // — description/color get real values below via the seed
            // upsert's ON CONFLICT DO UPDATE, this default only exists so
            // the NOT NULL constraint can attach).
            migrationBuilder.AddColumn<string>(
                name: "Description", table: "Categories", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(
                name: "Color", table: "Categories", type: "text", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<int>(
                name: "Position", table: "Categories", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.DropColumn(name: "IsPillar", table: "Categories");

            // 2. Seed the 3 categories replacing the old Pillar enum values,
            // preserving today's accent colors (apps/blog/src/app.css's
            // --color-accent-red/green/blue) and giving every post
            // somewhere to land in step 3. Fixed, well-known IDs (not
            // Guid.CreateVersion7()) on a fresh install so this migration is
            // deterministic and repeatable — but a category with one of
            // these slugs may already exist (e.g. created by hand through
            // the admin Categories screen before this migration ever ran),
            // in which case ON CONFLICT ("Slug") DO UPDATE fills in real
            // Description/Color/Position on the EXISTING row instead of
            // failing on the unique index, and deliberately keeps that
            // row's existing Id rather than the hardcoded one above (an
            // INSERT can't change another row's primary key) — see step 3,
            // which resolves by slug for exactly this reason.
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Id"", ""Name"", ""Slug"", ""Description"", ""Color"", ""Position"", ""IsVisible"", ""IsDeleted"", ""CreatedAt"")
                VALUES
                    ('00000000-0000-0000-0000-000000000001', 'Tech', 'tech',
                     'Where the industry''s tools, platforms, and infrastructure get taken apart — what''s actually happening under the hype.',
                     '#C9553D', 1, true, false, now()),
                    ('00000000-0000-0000-0000-000000000002', 'Social · Psych', 'social_psych',
                     'Field notes on attention, identity, and the internet''s slow reshaping of how people think and connect.',
                     '#3F8F6A', 2, true, false, now()),
                    ('00000000-0000-0000-0000-000000000003', 'Software Dev', 'software_dev',
                     'The craft side of building software — architecture, process, and the decisions that hold up once real users show up.',
                     '#4A6FBF', 3, true, false, now())
                ON CONFLICT (""Slug"") DO UPDATE SET
                    ""Description"" = EXCLUDED.""Description"",
                    ""Color"" = EXCLUDED.""Color"",
                    ""Position"" = EXCLUDED.""Position"";
            ");

            // 3. Backfill: Pillar was stored as its C# enum ordinal
            // (Tech=0, SocialPsych=1, SoftwareDev=2 — no HasConversion in
            // AppDbContext, so EF used the default int mapping). Resolved
            // by slug, not the hardcoded seed IDs above — step 2's ON
            // CONFLICT DO UPDATE keeps whatever Id a pre-existing row
            // already had, so the hardcoded '...0001'/'...0002'/'...0003'
            // GUIDs are only guaranteed correct on a fresh install.
            migrationBuilder.Sql(@"
                UPDATE ""Posts"" SET ""CategoryId"" = (SELECT ""Id"" FROM ""Categories"" WHERE ""Slug"" = 'tech') WHERE ""Pillar"" = 0;
                UPDATE ""Posts"" SET ""CategoryId"" = (SELECT ""Id"" FROM ""Categories"" WHERE ""Slug"" = 'social_psych') WHERE ""Pillar"" = 1;
                UPDATE ""Posts"" SET ""CategoryId"" = (SELECT ""Id"" FROM ""Categories"" WHERE ""Slug"" = 'software_dev') WHERE ""Pillar"" = 2;
            ");

            // 3b. Safety net: if any post's Pillar somehow wasn't 0/1/2 (not
            // expected — the old enum only had 3 members — but nothing at
            // the schema level guaranteed it), it would still have a NULL
            // CategoryId at this point and fail the NOT NULL constraint
            // below with an opaque Postgres error. Land it in whichever
            // category sorts first rather than aborting the migration.
            migrationBuilder.Sql(@"
                UPDATE ""Posts"" SET ""CategoryId"" = (SELECT ""Id"" FROM ""Categories"" ORDER BY ""Position"" LIMIT 1)
                WHERE ""CategoryId"" IS NULL;
            ");

            // 4. Now safe to tighten CategoryId and drop Pillar — every row
            // has a real CategoryId as of steps 3/3b. Keep EF's own
            // generated DropForeignKey/AlterColumn/AddForeignKey calls for
            // this part (nullable -> NOT NULL, SetNull -> Restrict) exactly
            // as scaffolded, placed here in the sequence.
            migrationBuilder.DropForeignKey(name: "FK_Posts_Categories_CategoryId", table: "Posts");
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId", table: "Posts", type: "uuid", nullable: false,
                oldClrType: typeof(Guid), oldType: "uuid", oldNullable: true);
            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId", table: "Posts", column: "CategoryId",
                principalTable: "Categories", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.DropColumn(name: "Pillar", table: "Posts");
        }

        // Down() is not safe once this migration has been applied against a
        // database with posts referencing non-seed categories (any category
        // created after this migration has no Pillar equivalent to revert
        // to) — this restores the 3 seed categories' posts to their
        // original Pillar values and drops the new columns, nothing more.
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Posts_Categories_CategoryId", table: "Posts");
            migrationBuilder.AddColumn<int>(name: "Pillar", table: "Posts", type: "integer", nullable: false, defaultValue: 0);
            migrationBuilder.Sql(@"
                UPDATE ""Posts"" SET ""Pillar"" = 0 WHERE ""CategoryId"" = '00000000-0000-0000-0000-000000000001';
                UPDATE ""Posts"" SET ""Pillar"" = 1 WHERE ""CategoryId"" = '00000000-0000-0000-0000-000000000002';
                UPDATE ""Posts"" SET ""Pillar"" = 2 WHERE ""CategoryId"" = '00000000-0000-0000-0000-000000000003';
            ");
            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId", table: "Posts", type: "uuid", nullable: true,
                oldClrType: typeof(Guid), oldType: "uuid");
            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categories_CategoryId", table: "Posts", column: "CategoryId",
                principalTable: "Categories", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
            migrationBuilder.DeleteData(table: "Categories", keyColumn: "Id", keyValues: new object[]
            {
                new Guid("00000000-0000-0000-0000-000000000001"),
                new Guid("00000000-0000-0000-0000-000000000002"),
                new Guid("00000000-0000-0000-0000-000000000003"),
            });
            migrationBuilder.AddColumn<bool>(name: "IsPillar", table: "Categories", type: "boolean", nullable: false, defaultValue: false);
            migrationBuilder.DropColumn(name: "Description", table: "Categories");
            migrationBuilder.DropColumn(name: "Color", table: "Categories");
            migrationBuilder.DropColumn(name: "Position", table: "Categories");
        }
    }
}

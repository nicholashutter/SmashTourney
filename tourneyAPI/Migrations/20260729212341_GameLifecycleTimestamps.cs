using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace tourneyAPI.Migrations
{
    /// <inheritdoc />
    public partial class GameLifecycleTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedUtc",
                table: "Games",
                type: "TEXT",
                nullable: true);

            // Backfilled to now, not to DateTime.MinValue as EF scaffolded it.
            //
            // PruneStaleGamesAsync deletes any game whose LastActivityUtc is
            // older than the abandoned window, so a MinValue backfill would mark
            // every game that already exists as two thousand years idle and sweep
            // the lot on the first listing call after upgrading. Treating rows
            // that predate the column as active right now is the safe reading:
            // they get a full silence window to prove they are abandoned.
            migrationBuilder.AddColumn<DateTime>(
                name: "LastActivityUtc",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedUtc",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "LastActivityUtc",
                table: "Games");
        }
    }
}

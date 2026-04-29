using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <summary>
    /// Allow multiple members to point to the same SpouseId.
    /// Initial migration created a unique index on Members.SpouseId because the
    /// relation was modelled as 1-1, which made it impossible for one person to
    /// have several spouses (polygamy). This migration drops the uniqueness
    /// constraint and recreates the index as a regular many-to-one index.
    /// </summary>
    public partial class RelaxSpouseUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_SpouseId",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_SpouseId",
                table: "Members",
                column: "SpouseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_SpouseId",
                table: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_SpouseId",
                table: "Members",
                column: "SpouseId",
                unique: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <summary>
    /// Allow one user to own several families. Initial migration created a
    /// unique index on Families.OwnerId because the Owner relation was
    /// modelled as 1-1, which made it impossible to create more than one
    /// family per user. Drops the uniqueness and recreates a regular index.
    /// </summary>
    public partial class RelaxFamilyOwnerUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_OwnerId",
                table: "Families");

            migrationBuilder.CreateIndex(
                name: "IX_Families_OwnerId",
                table: "Families",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Families_OwnerId",
                table: "Families");

            migrationBuilder.CreateIndex(
                name: "IX_Families_OwnerId",
                table: "Families",
                column: "OwnerId",
                unique: true);
        }
    }
}

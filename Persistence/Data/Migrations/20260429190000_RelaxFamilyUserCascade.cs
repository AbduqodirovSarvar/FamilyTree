using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <summary>
    /// Allow deleting a family even when its owner is also a member of it.
    /// Initial migration cascaded user deletion when the family was deleted,
    /// which combined with the OwnerId FK in the opposite direction produced a
    /// circular dependency in EF's topological sort. SetNull breaks the cycle:
    /// family is removed, the owner-user stays, FamilyId becomes null.
    /// </summary>
    public partial class RelaxFamilyUserCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Families_FamilyId",
                table: "Users",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixMemberFatherSelfRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_Members_FatherId1",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_FatherId1",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "FatherId1",
                table: "Members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FatherId1",
                table: "Members",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_FatherId1",
                table: "Members",
                column: "FatherId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_Members_FatherId1",
                table: "Members",
                column: "FatherId1",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}

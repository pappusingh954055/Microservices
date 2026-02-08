using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserPermissionRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Menus_MenuId1",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_MenuId1",
                table: "Menus");

            migrationBuilder.DropColumn(
                name: "MenuId1",
                table: "Menus");

            migrationBuilder.CreateIndex(
                name: "IX_Menus_ParentId",
                table: "Menus",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Menus_ParentId",
                table: "Menus",
                column: "ParentId",
                principalTable: "Menus",
                principalColumn: "MenuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Menus_Menus_ParentId",
                table: "Menus");

            migrationBuilder.DropIndex(
                name: "IX_Menus_ParentId",
                table: "Menus");

            migrationBuilder.AddColumn<int>(
                name: "MenuId1",
                table: "Menus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menus_MenuId1",
                table: "Menus",
                column: "MenuId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Menus_Menus_MenuId1",
                table: "Menus",
                column: "MenuId1",
                principalTable: "Menus",
                principalColumn: "MenuId");
        }
    }
}

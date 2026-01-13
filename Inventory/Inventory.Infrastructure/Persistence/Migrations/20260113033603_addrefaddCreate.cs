using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class addrefaddCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId2",
                table: "Subcategories",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subcategories_CategoryId2",
                table: "Subcategories",
                column: "CategoryId2");

            migrationBuilder.AddForeignKey(
                name: "FK_Subcategories_Categories_CategoryId2",
                table: "Subcategories",
                column: "CategoryId2",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subcategories_Categories_CategoryId2",
                table: "Subcategories");

            migrationBuilder.DropIndex(
                name: "IX_Subcategories_CategoryId2",
                table: "Subcategories");

            migrationBuilder.DropColumn(
                name: "CategoryId2",
                table: "Subcategories");
        }
    }
}

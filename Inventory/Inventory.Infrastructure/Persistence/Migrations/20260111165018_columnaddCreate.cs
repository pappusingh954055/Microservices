using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class columnaddCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Products",
                newName: "TrackInventory");

            migrationBuilder.AddColumn<string>(
                name: "HSNCode",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MinStock",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HSNCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "MinStock",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "TrackInventory",
                table: "Products",
                newName: "IsActive");
        }
    }
}

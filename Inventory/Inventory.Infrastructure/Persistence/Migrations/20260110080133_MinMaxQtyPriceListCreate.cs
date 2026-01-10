using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MinMaxQtyPriceListCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PriceListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxQty",
                table: "PriceListItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinQty",
                table: "PriceListItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "MaxQty",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "MinQty",
                table: "PriceListItems");
        }
    }
}

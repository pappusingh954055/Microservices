using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ColumnAddPurchaseOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GstPercent",
                table: "PurchaseOrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountPercent",
                table: "PurchaseOrderItems");

            migrationBuilder.DropColumn(
                name: "GstPercent",
                table: "PurchaseOrderItems");
        }
    }
}

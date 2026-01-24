using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemarksPurchaseOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_PriceLists_PriceListId",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "PriceListId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpectedDeliveryDate",
                table: "PurchaseOrders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "PurchaseOrders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_PriceLists_PriceListId",
                table: "PurchaseOrders",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_PriceLists_PriceListId",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDate",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<Guid>(
                name: "PriceListId",
                table: "PurchaseOrders",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_PriceLists_PriceListId",
                table: "PurchaseOrders",
                column: "PriceListId",
                principalTable: "PriceLists",
                principalColumn: "Id");
        }
    }
}

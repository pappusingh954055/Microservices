using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrderIdPO : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "GRNHeaders");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "GRNDetails");

            migrationBuilder.RenameColumn(
                name: "POHeaderId",
                table: "GRNHeaders",
                newName: "PurchaseOrderId");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "GRNHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GRNHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "GRNDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GRNDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GRNHeaders_PurchaseOrderId",
                table: "GRNHeaders",
                column: "PurchaseOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_GRNHeaders_PurchaseOrders_PurchaseOrderId",
                table: "GRNHeaders",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GRNHeaders_PurchaseOrders_PurchaseOrderId",
                table: "GRNHeaders");

            migrationBuilder.DropIndex(
                name: "IX_GRNHeaders_PurchaseOrderId",
                table: "GRNHeaders");

            migrationBuilder.RenameColumn(
                name: "PurchaseOrderId",
                table: "GRNHeaders",
                newName: "POHeaderId");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "GRNHeaders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GRNHeaders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "GRNHeaders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                table: "GRNDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "GRNDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "GRNDetails",
                type: "datetime2",
                nullable: true);
        }
    }
}

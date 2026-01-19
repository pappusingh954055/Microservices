using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSSSPricelistData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "PriceLists");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "PriceListItems");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "PriceListItems");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "PriceLists",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                table: "PriceLists",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "PriceLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "PriceLists",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "PriceLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "PriceLists",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "PriceListItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "PriceListItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PriceListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "PriceListItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "PriceListItems",
                type: "datetime2",
                nullable: true);
        }
    }
}

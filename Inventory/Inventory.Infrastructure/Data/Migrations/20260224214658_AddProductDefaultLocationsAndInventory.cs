using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductDefaultLocationsAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefaultRackId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultWarehouseId",
                table: "Products",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RackId",
                table: "GRNDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WarehouseId",
                table: "GRNDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RackId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Warehouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Warehouses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Racks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Racks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Racks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultRackId",
                table: "Products",
                column: "DefaultRackId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_DefaultWarehouseId",
                table: "Products",
                column: "DefaultWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_RackId",
                table: "GRNDetails",
                column: "RackId");

            migrationBuilder.CreateIndex(
                name: "IX_GRNDetails_WarehouseId",
                table: "GRNDetails",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Racks_WarehouseId",
                table: "Racks",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_GRNDetails_Racks_RackId",
                table: "GRNDetails",
                column: "RackId",
                principalTable: "Racks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GRNDetails_Warehouses_WarehouseId",
                table: "GRNDetails",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Racks_DefaultRackId",
                table: "Products",
                column: "DefaultRackId",
                principalTable: "Racks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Warehouses_DefaultWarehouseId",
                table: "Products",
                column: "DefaultWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GRNDetails_Racks_RackId",
                table: "GRNDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_GRNDetails_Warehouses_WarehouseId",
                table: "GRNDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Racks_DefaultRackId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Warehouses_DefaultWarehouseId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropTable(
                name: "Racks");

            migrationBuilder.DropTable(
                name: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultRackId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_DefaultWarehouseId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_GRNDetails_RackId",
                table: "GRNDetails");

            migrationBuilder.DropIndex(
                name: "IX_GRNDetails_WarehouseId",
                table: "GRNDetails");

            migrationBuilder.DropColumn(
                name: "DefaultRackId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "DefaultWarehouseId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RackId",
                table: "GRNDetails");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "GRNDetails");
        }
    }
}

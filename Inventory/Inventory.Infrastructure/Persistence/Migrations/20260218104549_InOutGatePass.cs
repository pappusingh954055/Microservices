using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InOutGatePass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GatePasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PassType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReferenceType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<int>(type: "int", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VehicleType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DriverPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransporterName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalQty = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalWeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GateEntryTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SecurityGuard = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatePasses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GatePasses_PassNo",
                table: "GatePasses",
                column: "PassNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GatePasses");
        }
    }
}

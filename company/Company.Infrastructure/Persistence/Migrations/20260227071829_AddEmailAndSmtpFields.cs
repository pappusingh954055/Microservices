using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Company.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailAndSmtpFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpEmail",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpHost",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmtpPassword",
                table: "CompanyProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SmtpPort",
                table: "CompanyProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SmtpUseSsl",
                table: "CompanyProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "BankDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AuthorizedSignatories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Addresses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SmtpEmail",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SmtpHost",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SmtpPassword",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SmtpPort",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "SmtpUseSsl",
                table: "CompanyProfiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "BankDetails");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AuthorizedSignatories");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Addresses");
        }
    }
}

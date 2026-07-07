using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetOtpExpiresAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetOtpHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetOtpExpiresAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetOtpHash",
                table: "Users");
        }
    }
}

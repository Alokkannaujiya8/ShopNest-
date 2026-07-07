using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopNest.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndMobile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationOtpExpiresAtUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationOtpHash",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationOtpRetryCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpExpiresAtUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmailVerificationOtpRetryCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Users");
        }
    }
}

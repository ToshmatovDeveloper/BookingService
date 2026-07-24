using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Auth.Migrations
{
    /// <inheritdoc />
    public partial class added_createdOn_field_in_refreshToken_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "auth",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "auth",
                table: "RefreshTokens");
        }
    }
}

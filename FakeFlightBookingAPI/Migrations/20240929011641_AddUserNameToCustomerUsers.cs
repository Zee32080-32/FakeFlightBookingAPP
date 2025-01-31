﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FakeFlightBookingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNameToCustomerUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "CustomerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                table: "CustomerUsers");
        }
    }
}

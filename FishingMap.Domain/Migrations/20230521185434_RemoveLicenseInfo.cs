using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLicenseInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicenseInfo",
                table: "Locations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 5, 21, 21, 54, 34, 88, DateTimeKind.Local).AddTicks(2569), new DateTime(2023, 5, 21, 21, 54, 34, 88, DateTimeKind.Local).AddTicks(2569), "dBGObuORnhWEb59hFvXSMJ6bfKKGdkuRbkqmHXl9Vgg=", "cHKFZ6L15sY6uJSIiEZ65g==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicenseInfo",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 4, 4, 23, 26, 35, 495, DateTimeKind.Local).AddTicks(3088), new DateTime(2023, 4, 4, 23, 26, 35, 495, DateTimeKind.Local).AddTicks(3088), "Bz4U3E0PLjYM0Cz3lbsJCn7DoqY5RujcHX6AQnHzhyc=", "xL6AH1CSshok+OXZOZBT5g==" });
        }
    }
}

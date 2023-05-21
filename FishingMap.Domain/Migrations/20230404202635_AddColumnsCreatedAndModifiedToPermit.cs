using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsCreatedAndModifiedToPermit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Permits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "Permits",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 4, 4, 23, 26, 35, 495, DateTimeKind.Local).AddTicks(3088), new DateTime(2023, 4, 4, 23, 26, 35, 495, DateTimeKind.Local).AddTicks(3088), "Bz4U3E0PLjYM0Cz3lbsJCn7DoqY5RujcHX6AQnHzhyc=", "xL6AH1CSshok+OXZOZBT5g==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "Permits");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "Permits");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 4, 4, 22, 9, 48, 445, DateTimeKind.Local).AddTicks(9435), new DateTime(2023, 4, 4, 22, 9, 48, 445, DateTimeKind.Local).AddTicks(9435), "F9se4c9IXDvMsyeDQEAav7ebCA1WVJLmGQ+ANgkp7jo=", "y7XRq2KW+nri7DlO2aA3kw==" });
        }
    }
}

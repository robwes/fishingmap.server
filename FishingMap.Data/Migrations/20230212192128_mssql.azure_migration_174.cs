using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class mssqlazuremigration174 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "FirstName", "LastName", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 2, 12, 21, 21, 28, 217, DateTimeKind.Local).AddTicks(1052), "Lord Admin", "First of His Name", new DateTime(2023, 2, 12, 21, 21, 28, 217, DateTimeKind.Local).AddTicks(1052), "4LPA91YSVvL0h8iVb91zh3J3gnrqlxQ+cQbfkhgEvFI=", "nJU7/z/7oyz/wjo7RRopzg==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserName",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "FirstName", "LastName", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2022, 3, 10, 23, 11, 22, 421, DateTimeKind.Local).AddTicks(2164), null, null, new DateTime(2022, 3, 10, 23, 11, 22, 421, DateTimeKind.Local).AddTicks(2164), "30Ssr4vnUkD82RIxlvw5ZRo0AH+b1ytO0uYVB+czPrM=", "hTZuuLbLUvTpxpOnn61TZg==" });
        }
    }
}

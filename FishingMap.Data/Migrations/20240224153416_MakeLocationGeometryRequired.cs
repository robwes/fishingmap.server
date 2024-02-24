using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeLocationGeometryRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleUser",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "RoleUser",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geometry",
                table: "Locations",
                type: "geography",
                nullable: false,
                oldClrType: typeof(MultiPolygon),
                oldType: "geography",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<MultiPolygon>(
                name: "Geometry",
                table: "Locations",
                type: "geography",
                nullable: true,
                oldClrType: typeof(MultiPolygon),
                oldType: "geography");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Administrator" },
                    { 2, "User" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Created", "Email", "FirstName", "LastName", "Modified", "Password", "Salt", "UserName" },
                values: new object[] { 1, new DateTime(2023, 8, 30, 21, 22, 21, 282, DateTimeKind.Local).AddTicks(7865), "admin@fishingmap.se", "Lord Admin", "First of His Name", new DateTime(2023, 8, 30, 21, 22, 21, 282, DateTimeKind.Local).AddTicks(7865), "ywBh1PQoSpI8OwVI3VPujyBwKnE6C6KmPa1fh1K5xEc=", "s/IQaB1yEM4C/nXW6k+vVQ==", "admin" });

            migrationBuilder.InsertData(
                table: "RoleUser",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 }
                });
        }
    }
}

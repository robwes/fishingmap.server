using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace FishingMap.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnNavigationPositionToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Point>(
                name: "NavigationPosition",
                table: "Locations",
                type: "geography",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 8, 30, 21, 22, 21, 282, DateTimeKind.Local).AddTicks(7865), new DateTime(2023, 8, 30, 21, 22, 21, 282, DateTimeKind.Local).AddTicks(7865), "ywBh1PQoSpI8OwVI3VPujyBwKnE6C6KmPa1fh1K5xEc=", "s/IQaB1yEM4C/nXW6k+vVQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NavigationPosition",
                table: "Locations");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 5, 21, 21, 54, 34, 88, DateTimeKind.Local).AddTicks(2569), new DateTime(2023, 5, 21, 21, 54, 34, 88, DateTimeKind.Local).AddTicks(2569), "dBGObuORnhWEb59hFvXSMJ6bfKKGdkuRbkqmHXl9Vgg=", "cHKFZ6L15sY6uJSIiEZ65g==" });
        }
    }
}

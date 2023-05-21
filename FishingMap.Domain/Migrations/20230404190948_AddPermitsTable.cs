using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPermitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationPermit",
                columns: table => new
                {
                    LocationsId = table.Column<int>(type: "int", nullable: false),
                    PermitsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPermit", x => new { x.LocationsId, x.PermitsId });
                    table.ForeignKey(
                        name: "FK_LocationPermit_Locations_LocationsId",
                        column: x => x.LocationsId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationPermit_Permits_PermitsId",
                        column: x => x.PermitsId,
                        principalTable: "Permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 4, 4, 22, 9, 48, 445, DateTimeKind.Local).AddTicks(9435), new DateTime(2023, 4, 4, 22, 9, 48, 445, DateTimeKind.Local).AddTicks(9435), "F9se4c9IXDvMsyeDQEAav7ebCA1WVJLmGQ+ANgkp7jo=", "y7XRq2KW+nri7DlO2aA3kw==" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationPermit_PermitsId",
                table: "LocationPermit",
                column: "PermitsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationPermit");

            migrationBuilder.DropTable(
                name: "Permits");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Created", "Modified", "Password", "Salt" },
                values: new object[] { new DateTime(2023, 2, 12, 21, 21, 28, 217, DateTimeKind.Local).AddTicks(1052), new DateTime(2023, 2, 12, 21, 21, 28, 217, DateTimeKind.Local).AddTicks(1052), "4LPA91YSVvL0h8iVb91zh3J3gnrqlxQ+cQbfkhgEvFI=", "nJU7/z/7oyz/wjo7RRopzg==" });
        }
    }
}

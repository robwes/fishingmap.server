using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Domain.Migrations
{
    public partial class RenamePointsToGeometry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Locations",
                newName: "Geometry");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Geometry",
                table: "Locations",
                newName: "Points");
        }
    }
}

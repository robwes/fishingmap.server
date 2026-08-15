using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOptInRegionInheritance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationSpeciesFollowsRegions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    SpeciesId = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSpeciesFollowsRegions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationSpeciesFollowsRegions_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationSpeciesFollowsRegions_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationSpeciesFollowsRegions_LocationId_SpeciesId",
                table: "LocationSpeciesFollowsRegions",
                columns: new[] { "LocationId", "SpeciesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationSpeciesFollowsRegions_SpeciesId",
                table: "LocationSpeciesFollowsRegions",
                column: "SpeciesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationSpeciesFollowsRegions");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRegionsAndRegulations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ParentRegionId = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Regions_Regions_ParentRegionId",
                        column: x => x.ParentRegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpeciesRegulations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpeciesId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    MinimumSizeCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaximumSizeCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BagLimit = table.Column<int>(type: "int", nullable: true),
                    IsCatchAndReleaseOnly = table.Column<bool>(type: "bit", nullable: false),
                    MustReportCatch = table.Column<bool>(type: "bit", nullable: false),
                    AdditionalRules = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeciesRegulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpeciesRegulations_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SpeciesRegulations_Species_SpeciesId",
                        column: x => x.SpeciesId,
                        principalTable: "Species",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationSpeciesRegulation",
                columns: table => new
                {
                    LocationsId = table.Column<int>(type: "int", nullable: false),
                    SpeciesRegulationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationSpeciesRegulation", x => new { x.LocationsId, x.SpeciesRegulationsId });
                    table.ForeignKey(
                        name: "FK_LocationSpeciesRegulation_Locations_LocationsId",
                        column: x => x.LocationsId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationSpeciesRegulation_SpeciesRegulations_SpeciesRegulationsId",
                        column: x => x.SpeciesRegulationsId,
                        principalTable: "SpeciesRegulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtectedPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpeciesRegulationId = table.Column<int>(type: "int", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    EndMonth = table.Column<int>(type: "int", nullable: false),
                    EndDay = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtectedPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtectedPeriods_SpeciesRegulations_SpeciesRegulationId",
                        column: x => x.SpeciesRegulationId,
                        principalTable: "SpeciesRegulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_RegionId",
                table: "Locations",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationSpeciesRegulation_SpeciesRegulationsId",
                table: "LocationSpeciesRegulation",
                column: "SpeciesRegulationsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtectedPeriods_SpeciesRegulationId",
                table: "ProtectedPeriods",
                column: "SpeciesRegulationId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_ParentRegionId",
                table: "Regions",
                column: "ParentRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Regions_Type",
                table: "Regions",
                column: "Type",
                unique: true,
                filter: "[Type] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesRegulations_RegionId",
                table: "SpeciesRegulations",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SpeciesRegulations_SpeciesId",
                table: "SpeciesRegulations",
                column: "SpeciesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Regions_RegionId",
                table: "Locations",
                column: "RegionId",
                principalTable: "Regions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Regions_RegionId",
                table: "Locations");

            migrationBuilder.DropTable(
                name: "LocationSpeciesRegulation");

            migrationBuilder.DropTable(
                name: "ProtectedPeriods");

            migrationBuilder.DropTable(
                name: "SpeciesRegulations");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropIndex(
                name: "IX_Locations_RegionId",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Locations");
        }
    }
}

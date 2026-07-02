using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScientificNameToSpecies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ScientificName",
                table: "Species",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScientificName",
                table: "Species");
        }
    }
}

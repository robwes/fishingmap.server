using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBagLimitBasis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BagLimitBasis",
                table: "SpeciesRegulations",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BagLimitBasis",
                table: "SpeciesRegulations");
        }
    }
}

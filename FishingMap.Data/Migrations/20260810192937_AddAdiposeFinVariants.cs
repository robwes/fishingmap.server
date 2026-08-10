using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FishingMap.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdiposeFinVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdiposeFin",
                table: "SpeciesRegulations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFullyProtected",
                table: "SpeciesRegulations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdiposeFin",
                table: "SpeciesRegulations");

            migrationBuilder.DropColumn(
                name: "IsFullyProtected",
                table: "SpeciesRegulations");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityJwtWeather.Migrations
{
    /// <inheritdoc />
    public partial class AddSolarPowerPlantProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolarPowerPlantProduction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolarPlantId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Production = table.Column<decimal>(type: "decimal(15,3)", precision: 15, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolarPowerPlantProduction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolarPowerPlantProduction_SolarPowerPlants_SolarPlantId",
                        column: x => x.SolarPlantId,
                        principalTable: "SolarPowerPlants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SolarPowerPlantProduction_SolarPlantId",
                table: "SolarPowerPlantProduction",
                column: "SolarPlantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolarPowerPlantProduction");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityJwtWether.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSolarPlantModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SolarPowerPlants",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<decimal>(
                name: "InstalledPower",
                table: "SolarPowerPlants",
                type: "decimal(15,3)",
                precision: 15,
                scale: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SolarPowerPlants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<double>(
                name: "InstalledPower",
                table: "SolarPowerPlants",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(15,3)",
                oldPrecision: 15,
                oldScale: 3);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class change_model_prediction_variables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgriculturalHouseHoldIndicator",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfBedrooms",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfCars",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalNumberOfBedrooms",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalNumberOfCars",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "AgriculturalHouseHoldIndicator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}

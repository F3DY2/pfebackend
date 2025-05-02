using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class add_optional_information_to_user_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgriculturalHouseHoldIndicator",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfFamilyMembers",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalNumberOfFamilyMembersEmployed",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgriculturalHouseHoldIndicator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalNumberOfFamilyMembers",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TotalNumberOfFamilyMembersEmployed",
                table: "AspNetUsers");
        }
    }
}

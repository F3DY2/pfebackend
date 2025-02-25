using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFirstAndLastName1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "first_Name",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_Name",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "first_Name",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "last_Name",
                table: "AspNetUsers");
        }
    }
}

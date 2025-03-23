using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class updateNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "limitValue",
                table: "Budgets",
                newName: "LimitValue");

            migrationBuilder.RenameColumn(
                name: "alertValue",
                table: "Budgets",
                newName: "AlertValue");

            migrationBuilder.RenameColumn(
                name: "last_Name",
                table: "AspNetUsers",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "first_Name",
                table: "AspNetUsers",
                newName: "FirstName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LimitValue",
                table: "Budgets",
                newName: "limitValue");

            migrationBuilder.RenameColumn(
                name: "AlertValue",
                table: "Budgets",
                newName: "alertValue");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "AspNetUsers",
                newName: "last_Name");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "AspNetUsers",
                newName: "first_Name");
        }
    }
}

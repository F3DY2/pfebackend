using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class updateNotificationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryNum",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryNum",
                table: "Notifications");
        }
    }
}

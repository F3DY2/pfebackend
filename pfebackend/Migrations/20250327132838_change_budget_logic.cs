using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace pfebackend.Migrations
{
    /// <inheritdoc />
    public partial class change_budget_logic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_AspNetUsers_UserId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_UserId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Budgets");

            migrationBuilder.AddColumn<int>(
                name: "BudgetPeriodId",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BudgetPeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Income = table.Column<float>(type: "real", nullable: false),
                    Savings = table.Column<float>(type: "real", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetPeriod_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_BudgetPeriodId",
                table: "Budgets",
                column: "BudgetPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetPeriod_UserId",
                table: "BudgetPeriod",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_BudgetPeriod_BudgetPeriodId",
                table: "Budgets",
                column: "BudgetPeriodId",
                principalTable: "BudgetPeriod",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_BudgetPeriod_BudgetPeriodId",
                table: "Budgets");

            migrationBuilder.DropTable(
                name: "BudgetPeriod");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_BudgetPeriodId",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "BudgetPeriodId",
                table: "Budgets");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Budgets",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Budgets",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Budgets",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_UserId",
                table: "Budgets",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_AspNetUsers_UserId",
                table: "Budgets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

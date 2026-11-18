using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finaco.Api.Migrations;

public partial class AddCashflowSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateOnly>("NextIncomeDate", "accounts", nullable: true);
        migrationBuilder.AddColumn<decimal>("NextIncomeAmount", "accounts", type: "numeric(18,2)", precision: 18, scale: 2, nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("NextIncomeDate", "accounts");
        migrationBuilder.DropColumn("NextIncomeAmount", "accounts");
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    public partial class LockedPaymentColumnAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "PaidOvertime",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "PaidOvertime");
        }
    }
}

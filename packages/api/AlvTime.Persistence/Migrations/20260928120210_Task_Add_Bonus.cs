using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Task_Add_Bonus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bonus",
                table: "Task",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                UPDATE t
                SET t.Bonus = 1
                FROM Task t
                INNER JOIN (
                    SELECT TaskId, Rate,
                           ROW_NUMBER() OVER (PARTITION BY TaskId ORDER BY FromDate DESC) AS rn
                    FROM HourRate
                    WHERE FromDate <= GETDATE()
                ) hr ON hr.TaskId = t.Id AND hr.rn = 1
                WHERE hr.Rate > 0
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bonus",
                table: "Task");
        }
    }
}

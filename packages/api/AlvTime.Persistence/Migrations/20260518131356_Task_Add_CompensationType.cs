using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Task_Add_CompensationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompensationType",
                table: "Task",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE t
                SET t.CompensationType = CASE cr.Value
                    WHEN 0.5 THEN 0
                    WHEN 1.0 THEN 1
                    WHEN 1.5 THEN 2
                    WHEN 2.0 THEN 2
                    ELSE -1
                END
                FROM Task t
                INNER JOIN (
                    SELECT TaskId, Value,
                           ROW_NUMBER() OVER (PARTITION BY TaskId ORDER BY FromDate DESC) AS rn
                    FROM CompensationRate
                ) cr ON cr.TaskId = t.Id AND cr.rn = 1
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompensationType",
                table: "Task");
        }
    }
}

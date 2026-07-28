using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Drop_CompensationRate_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompensationRate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompensationRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompensationRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompensationRate_Task",
                        column: x => x.TaskId,
                        principalTable: "Task",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompensationRate_TaskId",
                table: "CompensationRate",
                column: "TaskId");
        }
    }
}

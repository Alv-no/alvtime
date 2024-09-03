using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    public partial class AddCommentToHour : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "hours",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommentedAt",
                table: "hours",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "hours");

            migrationBuilder.DropColumn(
                name: "CommentedAt",
                table: "hours");
        }
    }
}

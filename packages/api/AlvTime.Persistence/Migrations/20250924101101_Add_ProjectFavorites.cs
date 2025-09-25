using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlvTime.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Add_ProjectFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectFavorite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectFavorite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectFavorites_Project",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectFavorites_User",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFavorite_ProjectId",
                table: "ProjectFavorite",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectFavorite_UserId_ProjectId",
                table: "ProjectFavorite",
                columns: new[] { "UserId", "ProjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectFavorite");
        }
    }
}

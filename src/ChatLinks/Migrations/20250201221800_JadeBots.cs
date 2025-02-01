using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class JadeBots : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JadeBots",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    UnlockItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JadeBots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JadeBots_Name",
                table: "JadeBots",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JadeBots_UnlockItemId",
                table: "JadeBots",
                column: "UnlockItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JadeBots");
        }
    }
}

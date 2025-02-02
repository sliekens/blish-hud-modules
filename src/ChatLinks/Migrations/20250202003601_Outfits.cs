using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class Outfits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Outfits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    IconHref = table.Column<string>(nullable: false),
                    UnlockItemIds = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Outfits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Outfits_Name",
                table: "Outfits",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Outfits");
        }
    }
}

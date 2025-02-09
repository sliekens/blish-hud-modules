using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class Miniatures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Miniatures",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    LockedText = table.Column<string>(nullable: false),
                    IconHref = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    ItemId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Miniatures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Miniatures_ItemId",
                table: "Miniatures",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Miniatures_Name",
                table: "Miniatures",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Miniatures");
        }
    }
}

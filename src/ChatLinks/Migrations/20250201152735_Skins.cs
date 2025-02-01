using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class Skins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Skins",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Flags = table.Column<string>(nullable: false),
                    Races = table.Column<string>(nullable: false),
                    Rarity = table.Column<string>(nullable: false),
                    IconHref = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    WeightClass = table.Column<string>(nullable: true),
                    DyeSlots = table.Column<string>(nullable: true),
                    DamageType = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skins", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Skins_Name",
                table: "Skins",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Skins");
        }
    }
}

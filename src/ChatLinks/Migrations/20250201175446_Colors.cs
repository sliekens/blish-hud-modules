using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Colors : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Colors",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                BaseRgb = table.Column<int>(nullable: false),
                Cloth = table.Column<string>(nullable: false),
                Leather = table.Column<string>(nullable: false),
                Metal = table.Column<string>(nullable: false),
                Fur = table.Column<string>(nullable: true),
                ItemId = table.Column<int>(nullable: true),
                Hue = table.Column<string>(nullable: false),
                Material = table.Column<string>(nullable: false),
                Set = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Colors", x => x.Id);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Colors_Hue",
            table: "Colors",
            column: "Hue");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Colors_ItemId",
            table: "Colors",
            column: "ItemId");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Colors_Material",
            table: "Colors",
            column: "Material");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Colors_Name",
            table: "Colors",
            column: "Name");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Colors_Set",
            table: "Colors",
            column: "Set");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "Colors");
    }
}

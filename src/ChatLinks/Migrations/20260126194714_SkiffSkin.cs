using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class SkiffSkin : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SkiffSkins",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                IconUrl = table.Column<string>(nullable: false),
                DyeSlots = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkiffSkins", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SkiffSkinUnlocks",
            columns: table => new
            {
                SkiffSkinId = table.Column<int>(nullable: false),
                ItemId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SkiffSkinUnlocks", x => new { x.SkiffSkinId, x.ItemId });
                table.ForeignKey(
                    name: "FK_SkiffSkinUnlocks_Items_ItemId",
                    column: x => x.ItemId,
                    principalTable: "Items",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_SkiffSkinUnlocks_SkiffSkins_SkiffSkinId",
                    column: x => x.SkiffSkinId,
                    principalTable: "SkiffSkins",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SkiffSkins_Name",
            table: "SkiffSkins",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_SkiffSkinUnlocks_ItemId",
            table: "SkiffSkinUnlocks",
            column: "ItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SkiffSkinUnlocks");

        migrationBuilder.DropTable(
            name: "SkiffSkins");
    }
}

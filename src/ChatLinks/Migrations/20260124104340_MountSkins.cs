using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class MountSkins : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "MountSkins",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                IconUrl = table.Column<string>(nullable: false),
                DyeSlots = table.Column<string>(nullable: false),
                MountId = table.Column<Guid>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MountSkins", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "MountSkinUnlocks",
            columns: table => new
            {
                MountSkinId = table.Column<int>(nullable: false),
                ItemId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MountSkinUnlocks", x => new { x.MountSkinId, x.ItemId });
                table.ForeignKey(
                    name: "FK_MountSkinUnlocks_Items_ItemId",
                    column: x => x.ItemId,
                    principalTable: "Items",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_MountSkinUnlocks_MountSkins_MountSkinId",
                    column: x => x.MountSkinId,
                    principalTable: "MountSkins",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_MountSkins_Name",
            table: "MountSkins",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_MountSkinUnlocks_ItemId",
            table: "MountSkinUnlocks",
            column: "ItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MountSkinUnlocks");

        migrationBuilder.DropTable(
            name: "MountSkins");
    }
}

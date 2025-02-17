using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class ItemsIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateIndex(
            name: "IX_Items_ChatLink",
            table: "Items",
            column: "ChatLink");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Items_Name",
            table: "Items",
            column: "Name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropIndex(
            name: "IX_Items_ChatLink",
            table: "Items");

        _ = migrationBuilder.DropIndex(
            name: "IX_Items_Name",
            table: "Items");
    }
}

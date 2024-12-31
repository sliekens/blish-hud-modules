using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class ItemsIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Items_ChatLink",
                table: "Items",
                column: "ChatLink");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_ChatLink",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_Name",
                table: "Items");
        }
    }
}

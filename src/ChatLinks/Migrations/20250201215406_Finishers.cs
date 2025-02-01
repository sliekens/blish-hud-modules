using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class Finishers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Finishers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LockedText = table.Column<string>(nullable: false),
                    UnlockItemIds = table.Column<string>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    IconHref = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Finishers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Finishers_Name",
                table: "Finishers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Finishers_Order",
                table: "Finishers",
                column: "Order");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Finishers");
        }
    }
}

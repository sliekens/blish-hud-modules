using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class MailCarriers : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "MailCarriers",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UnlockItemIds = table.Column<string>(nullable: false),
                Order = table.Column<int>(nullable: false),
                IconHref = table.Column<string>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Flags = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_MailCarriers", x => x.Id);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_MailCarriers_Name",
            table: "MailCarriers",
            column: "Name");

        _ = migrationBuilder.CreateIndex(
            name: "IX_MailCarriers_Order",
            table: "MailCarriers",
            column: "Order");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "MailCarriers");
    }
}

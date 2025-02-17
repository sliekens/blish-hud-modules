using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Novelties : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Novelties",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Name = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                IconHref = table.Column<string>(nullable: false),
                Slot = table.Column<string>(nullable: false),
                UnlockItemIds = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Novelties", x => x.Id);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Novelties_Name",
            table: "Novelties",
            column: "Name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "Novelties");
    }
}

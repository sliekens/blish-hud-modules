using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Gliders : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
           name: "Gliders",
           columns: table => new
           {
               Id = table.Column<int>(nullable: false)
                   .Annotation("Sqlite:Autoincrement", true),
               UnlockItemIds = table.Column<string>(nullable: false),
               Order = table.Column<int>(nullable: false),
               IconHref = table.Column<string>(nullable: false),
               Name = table.Column<string>(nullable: false),
               Description = table.Column<string>(nullable: false),
               DefaultDyeColorIds = table.Column<string>(nullable: false)
           },
           constraints: table =>
           {
               table.PrimaryKey("PK_Gliders", x => x.Id);
           });

        migrationBuilder.CreateIndex(
           name: "IX_Gliders_Name",
           table: "Gliders",
           column: "Name");

        migrationBuilder.CreateIndex(
           name: "IX_Gliders_Order",
           table: "Gliders",
           column: "Order");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
           name: "Gliders");
    }
}

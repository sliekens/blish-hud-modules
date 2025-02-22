using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class MistChampions : Migration
{
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
		   name: "MistChampions",
		   columns: table => new
		   {
			   Id = table.Column<int>(nullable: false)
				   .Annotation("Sqlite:Autoincrement", true),
			   Name = table.Column<string>(nullable: false),
			   IconHref = table.Column<string>(nullable: false),
			   Default = table.Column<bool>(nullable: false),
			   UnlockItemIds = table.Column<string>(nullable: false)
		   },
		   constraints: table =>
		   {
			   table.PrimaryKey("PK_MistChampions", x => x.Id);
		   });

		migrationBuilder.CreateIndex(
		   name: "IX_MistChampions_Name",
		   table: "MistChampions",
		   column: "Name");
	}

	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
		   name: "MistChampions");
	}
}

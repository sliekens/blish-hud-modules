using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Recipes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.CreateTable(
            name: "Recipes",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                OutputItemId = table.Column<int>(nullable: false),
                OutputItemCount = table.Column<int>(nullable: false),
                MinRating = table.Column<int>(nullable: false),
                TimeToCraft = table.Column<TimeSpan>(nullable: false),
                Disciplines = table.Column<string>(nullable: false),
                Flags = table.Column<string>(nullable: false),
                Ingredients = table.Column<string>(nullable: false),
                ChatLink = table.Column<string>(nullable: false),
                Type = table.Column<string>(nullable: false),
                OutputUpgradeId = table.Column<int>(nullable: true),
                GuildIngredients = table.Column<string>(nullable: true),
                OutputWvwUpgradeId = table.Column<int>(nullable: true)
            },
            constraints: table =>
            {
                _ = table.PrimaryKey("PK_Recipes", x => x.Id);
            });

        _ = migrationBuilder.CreateIndex(
            name: "IX_Recipes_ChatLink",
            table: "Recipes",
            column: "ChatLink");

        _ = migrationBuilder.CreateIndex(
            name: "IX_Recipes_OutputItemId",
            table: "Recipes",
            column: "OutputItemId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        _ = migrationBuilder.DropTable(
            name: "Recipes");
    }
}

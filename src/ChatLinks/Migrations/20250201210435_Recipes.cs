using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations
{
    public partial class Recipes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
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
                    GuildConsumableRecipe_GuildIngredients = table.Column<string>(nullable: true),
                    OutputUpgradeId = table.Column<int>(nullable: true),
                    GuildIngredients = table.Column<string>(nullable: true),
                    OutputWvwUpgradeId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_ChatLink",
                table: "Recipes",
                column: "ChatLink");

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_OutputItemId",
                table: "Recipes",
                column: "OutputItemId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recipes");
        }
    }
}

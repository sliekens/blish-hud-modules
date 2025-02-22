using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Achievements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AchievementCategories",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                Order = table.Column<int>(nullable: false),
                IconHref = table.Column<string>(nullable: false),
                Achievements = table.Column<string>(nullable: false),
                Tomorrow = table.Column<string>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AchievementCategories", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AchievementGroups",
            columns: table => new
            {
                Id = table.Column<string>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                Order = table.Column<int>(nullable: false),
                Categories = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AchievementGroups", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Achievements",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: false),
                IconHref = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                Requirement = table.Column<string>(nullable: false),
                LockedText = table.Column<string>(nullable: false),
                Flags = table.Column<string>(nullable: false),
                Tiers = table.Column<string>(nullable: false),
                Rewards = table.Column<string>(nullable: true),
                Bits = table.Column<string>(nullable: true),
                Prerequisites = table.Column<string>(nullable: false),
                PointCap = table.Column<int>(nullable: true),
                Type = table.Column<string>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Achievements", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AchievementCategories_Name",
            table: "AchievementCategories",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementCategories_Order",
            table: "AchievementCategories",
            column: "Order");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementGroups_Name",
            table: "AchievementGroups",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_AchievementGroups_Order",
            table: "AchievementGroups",
            column: "Order");

        migrationBuilder.CreateIndex(
            name: "IX_Achievements_Name",
            table: "Achievements",
            column: "Name");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AchievementCategories");

        migrationBuilder.DropTable(
            name: "AchievementGroups");

        migrationBuilder.DropTable(
            name: "Achievements");
    }
}

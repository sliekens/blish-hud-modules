using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class Renames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            update Items
            set Type = 'short_bow'
            where Type = 'shortbow';
            """);

        migrationBuilder.Sql("""
            update Skins
            set Type = 'short_bow'
            where Type = 'shortbow';
            """);

        migrationBuilder.Sql("""
            update Recipes
            set Type = 'short_bow'
            where Type = 'shortbow';
            """);

        migrationBuilder.Sql("""
            update Recipes
            set Type = 'back'
            where Type = 'backpack';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            update Recipes
            set Type = 'backpack'
            where Type = 'back';
            """);

        migrationBuilder.Sql("""
            update Recipes
            set Type = 'shortbow'
            where Type = 'short_bow';
            """);

        migrationBuilder.Sql("""
            update Skins
            set Type = 'shortbow'
            where Type = 'short_bow';
            """);

        migrationBuilder.Sql("""
            update Items
            set Type = 'shortbow'
            where Type = 'short_bow';
            """);
    }
}

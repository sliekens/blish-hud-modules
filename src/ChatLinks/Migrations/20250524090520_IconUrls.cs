using Microsoft.EntityFrameworkCore.Migrations;

namespace SL.ChatLinks.Migrations;

public partial class IconUrls : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // For Achievements: add IconUrl, copy, drop IconHref
        migrationBuilder.AddColumn<string>(
            name: "IconUrl",
            table: "Achievements",
            type: "TEXT",
            nullable: true);
        migrationBuilder.Sql("UPDATE Achievements SET IconUrl = NULLIF(IconHref, '');");
        migrationBuilder.Sql("ALTER TABLE Achievements DROP COLUMN IconHref;");

        // For all other tables: simple rename
        migrationBuilder.RenameColumn("IconHref", "Skins", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Outfits", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Novelties", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "MistChampions", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Miniatures", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "MailCarriers", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Items", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Gliders", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "Finishers", "IconUrl");
        migrationBuilder.RenameColumn("IconHref", "AchievementCategories", "IconUrl");

        // For nullable columns, replace empty strings with NULL
        migrationBuilder.Sql("UPDATE Skins SET IconUrl = NULL WHERE IconUrl = '';");
        migrationBuilder.Sql("UPDATE Items SET IconUrl = NULL WHERE IconUrl = '';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // For Achievements: add IconHref back, copy, drop IconUrl
        migrationBuilder.AddColumn<string>(
            name: "IconHref",
            table: "Achievements",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
        migrationBuilder.Sql("UPDATE Achievements SET IconHref = COALESCE(IconUrl, '');");
        migrationBuilder.Sql("ALTER TABLE Achievements DROP COLUMN IconUrl;");

        // For other tables: simple rename
        migrationBuilder.RenameColumn("IconUrl", "Skins", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Outfits", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Novelties", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "MistChampions", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Miniatures", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "MailCarriers", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Items", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Gliders", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "Finishers", "IconHref");
        migrationBuilder.RenameColumn("IconUrl", "AchievementCategories", "IconHref");

        // For nullable columns, replace NULLs with empty string
        migrationBuilder.Sql("UPDATE Skins SET IconUrl = '' WHERE IconUrl IS NULL;");
        migrationBuilder.Sql("UPDATE Items SET IconUrl = '' WHERE IconUrl IS NULL;");
    }
}

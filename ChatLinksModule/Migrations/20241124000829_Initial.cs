using Microsoft.EntityFrameworkCore.Migrations;

namespace ChatLinksModule.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Level = table.Column<int>(nullable: false),
                    Rarity = table.Column<string>(nullable: false),
                    VendorValue = table.Column<int>(nullable: false),
                    GameTypes = table.Column<string>(nullable: false),
                    Flags = table.Column<string>(nullable: false),
                    Restrictions = table.Column<string>(nullable: false),
                    ChatLink = table.Column<string>(nullable: false),
                    IconHref = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: false),
                    DefaultSkinId = table.Column<int>(nullable: true),
                    WeightClass = table.Column<string>(nullable: true),
                    Defense = table.Column<int>(nullable: true),
                    InfusionSlots = table.Column<string>(nullable: true),
                    AttributeAdjustment = table.Column<double>(nullable: true),
                    AttributeCombinationId = table.Column<int>(nullable: true),
                    Attributes = table.Column<string>(nullable: true),
                    Buff = table.Column<string>(nullable: true),
                    SuffixItemId = table.Column<int>(nullable: true),
                    StatChoices = table.Column<string>(nullable: true),
                    Backpack_DefaultSkinId = table.Column<int>(nullable: true),
                    Backpack_InfusionSlots = table.Column<string>(nullable: true),
                    Backpack_AttributeAdjustment = table.Column<double>(nullable: true),
                    Backpack_AttributeCombinationId = table.Column<int>(nullable: true),
                    Backpack_Attributes = table.Column<string>(nullable: true),
                    Backpack_Buff = table.Column<string>(nullable: true),
                    Backpack_SuffixItemId = table.Column<int>(nullable: true),
                    Backpack_StatChoices = table.Column<string>(nullable: true),
                    UpgradesInto = table.Column<string>(nullable: true),
                    UpgradesFrom = table.Column<string>(nullable: true),
                    NoSellOrSort = table.Column<bool>(nullable: true),
                    Size = table.Column<int>(nullable: true),
                    Effect = table.Column<string>(nullable: true),
                    GenericConsumable_Effect = table.Column<string>(nullable: true),
                    GuildUpgradeId = table.Column<int>(nullable: true),
                    Service_Effect = table.Column<string>(nullable: true),
                    Service_GuildUpgradeId = table.Column<int>(nullable: true),
                    SkinIds = table.Column<string>(nullable: true),
                    ColorId = table.Column<int>(nullable: true),
                    RecipeId = table.Column<int>(nullable: true),
                    ExtraRecipeIds = table.Column<string>(nullable: true),
                    Utility_Effect = table.Column<string>(nullable: true),
                    CraftingMaterial_UpgradesInto = table.Column<string>(nullable: true),
                    Gizmo_GuildUpgradeId = table.Column<int>(nullable: true),
                    MiniatureId = table.Column<int>(nullable: true),
                    Charges = table.Column<int>(nullable: true),
                    Trinket_InfusionSlots = table.Column<string>(nullable: true),
                    Trinket_AttributeAdjustment = table.Column<double>(nullable: true),
                    Trinket_AttributeCombinationId = table.Column<int>(nullable: true),
                    Trinket_Attributes = table.Column<string>(nullable: true),
                    Trinket_Buff = table.Column<string>(nullable: true),
                    Trinket_SuffixItemId = table.Column<int>(nullable: true),
                    Trinket_StatChoices = table.Column<string>(nullable: true),
                    Ring_UpgradesInto = table.Column<string>(nullable: true),
                    Ring_UpgradesFrom = table.Column<string>(nullable: true),
                    UpgradeComponentFlags = table.Column<string>(nullable: true),
                    InfusionUpgradeFlags = table.Column<string>(nullable: true),
                    UpgradeComponent_AttributeAdjustment = table.Column<double>(nullable: true),
                    UpgradeComponent_AttributeCombinationId = table.Column<int>(nullable: true),
                    UpgradeComponent_Attributes = table.Column<string>(nullable: true),
                    UpgradeComponent_Buff = table.Column<string>(nullable: true),
                    SuffixName = table.Column<string>(nullable: true),
                    Bonuses = table.Column<string>(nullable: true),
                    Weapon_DefaultSkinId = table.Column<int>(nullable: true),
                    DamageType = table.Column<string>(nullable: true),
                    MinPower = table.Column<int>(nullable: true),
                    MaxPower = table.Column<int>(nullable: true),
                    Weapon_Defense = table.Column<int>(nullable: true),
                    Weapon_InfusionSlots = table.Column<string>(nullable: true),
                    Weapon_AttributeAdjustment = table.Column<double>(nullable: true),
                    Weapon_AttributeCombinationId = table.Column<int>(nullable: true),
                    Weapon_Attributes = table.Column<string>(nullable: true),
                    Weapon_Buff = table.Column<string>(nullable: true),
                    Weapon_SuffixItemId = table.Column<int>(nullable: true),
                    SecondarySuffixItemId = table.Column<int>(nullable: true),
                    Weapon_StatChoices = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}

using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Graphics.UI;

using GuildWars2.Hero;
using GuildWars2;
using GuildWars2.Items;

using SL.Common.Controls.Items.Services;

using Consumable = GuildWars2.Items.Consumable;
using Container = GuildWars2.Items.Container;

namespace SL.Common.Controls.Items;

public sealed class ItemTooltipPresenter(IItemTooltipView view, ItemIcons icons)
    : Presenter<IItemTooltipView, ItemTooltipModel>(view, Objects.Create<ItemTooltipModel>())
{
    public ItemIcons Icons { get; } = icons;

    public Item? Item { get; set; }

    public IReadOnlyDictionary<int, UpgradeComponent>? Upgrades { get; set; }

    public string AttributeName(Extensible<AttributeName> stat)
    {
        return stat.IsDefined()
            ? stat.ToEnum() switch
            {
                GuildWars2.Hero.AttributeName.Power => "Power",
                GuildWars2.Hero.AttributeName.Precision => "Precision",
                GuildWars2.Hero.AttributeName.Toughness => "Toughness",
                GuildWars2.Hero.AttributeName.Vitality => "Vitality",
                GuildWars2.Hero.AttributeName.Concentration => "Concentration",
                GuildWars2.Hero.AttributeName.ConditionDamage => "Condition Damage",
                GuildWars2.Hero.AttributeName.Expertise => "Expertise",
                GuildWars2.Hero.AttributeName.Ferocity => "Ferocity",
                GuildWars2.Hero.AttributeName.HealingPower => "Healing Power",
                GuildWars2.Hero.AttributeName.AgonyResistance => "Agony Resistance",
                _ => stat.ToString()
            }
            : stat.ToString();
    }

    protected override Task<bool> Load(IProgress<string> progress)
    {
        return base.Load(progress);
    }

    protected override void UpdateView()
    {
        if (Item is null)
        {
            throw new InvalidOperationException();
        }

        switch (Item)
        {
            case Armor armor:
                PrintArmor(armor);
                break;
            case Backpack back:
                PrintBackpack(back);
                break;
            case Bag bag:
                PrintBag(bag);
                break;
            case Consumable consumable:
                PrintConsumable(consumable);
                break;
            case GuildWars2.Items.Container container:
                PrintContainer(container);
                break;
            case CraftingMaterial craftingMaterial:
                PrintCraftingMaterial(craftingMaterial);
                break;
            case GatheringTool gatheringTool:
                PrintGatheringTool(gatheringTool);
                break;
            case Trinket trinket:
                PrintTrinket(trinket);
                break;
            case Gizmo gizmo:
                PrintGizmo(gizmo);
                break;
            case JadeTechModule jadeTechModule:
                PrintJadeTechModule(jadeTechModule);
                break;
            case Miniature miniature:
                PrintMiniature(miniature);
                break;
            case PowerCore powerCore:
                PrintPowerCore(powerCore);
                break;
            case Relic relic:
                PrintRelic(relic);
                break;
            case SalvageTool salvageTool:
                PrintSalvageTool(salvageTool);
                break;
            case Trophy trophy:
                PrintTrophy(trophy);
                break;
            case UpgradeComponent upgradeComponent:
                PrintUpgradeComponent(upgradeComponent);
                break;
            case Weapon weapon:
                PrintWeapon(weapon);
                break;
            default:
                Print(Item);
                break;
        }
    }

    private void PrintArmor(Armor armor)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(armor, Upgrades);
        View.PrintDefense(armor.Defense);
        View.PrintAttributes(armor.Attributes.ToDictionary(
            stat => AttributeName(stat.Key),
            stat => stat.Value
        ));

        View.PrintUpgrades(armor, armor.Flags, Icons, Upgrades);
        View.PrintItemSkin(armor.DefaultSkinId);
        View.PrintItemRarity(armor.Rarity);
        View.PrintWeightClass(armor.WeightClass);
        switch (armor)
        {
            case Boots:
                View.PrintPlainText("Foot Armor");
                break;
            case Coat:
                View.PrintPlainText("Chest Armor");
                break;
            case Gloves:
                View.PrintPlainText("Hand Armor");
                break;
            case Helm:
            case HelmAquatic:
                View.PrintPlainText("Head Armor");
                break;
            case Leggings:
                View.PrintPlainText("Leg Armor");
                break;
            case Shoulders:
                View.PrintPlainText("Shoulder Armor");
                break;
        }

        View.PrintRequiredLevel(armor.Level);
        View.PrintDescription(armor.Description);
        View.PrintInBank();
        View.PrintStatChoices(armor);
        View.PrintUniqueness(armor);
        View.PrintItemBinding(armor);
        View.PrintVendorValue(armor);
    }

    [MemberNotNull(nameof(Upgrades))]
    private void EnsureUpgradesNotNull()
    {
        if (Upgrades is null)
        {
            throw new InvalidOperationException();
        }
    }

    private void PrintBackpack(Backpack back)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(back, Upgrades);
        View.PrintAttributes(back.Attributes.ToDictionary(
            stat => AttributeName(stat.Key),
            stat => stat.Value
        ));

        View.PrintUpgrades(back, back.Flags, Icons, Upgrades);

        View.PrintItemSkin(back.DefaultSkinId);

        View.PrintItemRarity(back.Rarity);
        View.PrintPlainText("Back Item");
        View.PrintRequiredLevel(back.Level);
        View.PrintDescription(back.Description);
        View.PrintInBank();
        View.PrintStatChoices(back);
        View.PrintUniqueness(back);
        View.PrintItemBinding(back);
        View.PrintVendorValue(back);
    }

    private void PrintBag(Bag bag)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(bag, Upgrades);
        View.PrintDescription(bag.Description);
        View.PrintInBank();
        View.PrintUniqueness(bag);
        View.PrintItemBinding(bag);
        View.PrintVendorValue(bag);
    }

    private void PrintConsumable(Consumable consumable)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(consumable, Upgrades);
        switch (consumable)
        {
            case Currency or Service:
                View.PrintPlainText("Takes effect immediately upon receipt.");
                break;
            default:
                View.PrintPlainText("Double-click to consume.");
                break;
        }

        switch (consumable)
        {
            case Food { Effect: not null } food:
                View.PrintEffect(food.Effect);
                break;
            case Utility { Effect: not null } utility:
                View.PrintEffect(utility.Effect);
                break;
            case Service { Effect: not null } service:
                View.PrintEffect(service.Effect);
                break;
            case GenericConsumable { Effect: not null } generic:
                View.PrintEffect(generic.Effect);
                break;
        }

        View.PrintDescription(consumable.Description);

        switch (consumable)
        {
            case Currency or Service:
                View.PrintPlainText(string.IsNullOrEmpty(consumable.Description) ? "Service" : "\r\nService");
                break;
            case Transmutation transmutation:
                View.PrintItemSkin(transmutation.SkinIds.First());
                View.PrintPlainText("\r\nConsumable");
                break;
            case Booze:
                View.PrintPlainText("""

                          Excessive alcohol consumption will result in intoxication.

                          Consumable          
                          """);
                break;
            default:
                View.PrintPlainText(string.IsNullOrEmpty(consumable.Description) ? "Consumable" : "\r\nConsumable");
                break;
        }

        View.PrintRequiredLevel(consumable.Level);
        View.PrintInBank();
        View.PrintUniqueness(consumable);
        View.PrintItemBinding(consumable);
        View.PrintVendorValue(consumable);

    }

    private void PrintContainer(Container container)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(container, Upgrades);
        View.PrintDescription(container.Description);
        View.PrintPlainText(string.IsNullOrEmpty(container.Description) ? "Consumable" : "\r\nConsumable");
        View.PrintInBank();
        View.PrintUniqueness(container);
        View.PrintItemBinding(container);
        View.PrintVendorValue(container);
    }

    private void PrintCraftingMaterial(CraftingMaterial craftingMaterial)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(craftingMaterial, Upgrades);
        View.PrintDescription(craftingMaterial.Description);
        View.PrintInBank();
        View.PrintUniqueness(craftingMaterial);
        View.PrintItemBinding(craftingMaterial);
        View.PrintVendorValue(craftingMaterial);
    }

    private void PrintGatheringTool(GatheringTool gatheringTool)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(gatheringTool, Upgrades);
        View.PrintDescription(gatheringTool.Description);
        View.PrintInBank();
        View.PrintUniqueness(gatheringTool);
        View.PrintItemBinding(gatheringTool);
        View.PrintVendorValue(gatheringTool);
    }

    private void PrintTrinket(Trinket trinket)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(trinket, Upgrades);
        View.PrintAttributes(trinket.Attributes.ToDictionary(
            stat => AttributeName(stat.Key),
            stat => stat.Value
        ));

        View.PrintUpgrades(trinket, trinket.Flags, Icons, Upgrades);
        View.PrintItemRarity(trinket.Rarity);

        switch (trinket)
        {
            case Accessory:
                View.PrintPlainText("Accessory");
                break;
            case Amulet:
                View.PrintPlainText("Amulet");
                break;
            case Ring:
                View.PrintPlainText("Ring");
                break;
        }

        View.PrintRequiredLevel(trinket.Level);
        View.PrintDescription(trinket.Description);
        View.PrintInBank();
        View.PrintStatChoices(trinket);
        View.PrintUniqueness(trinket);
        View.PrintItemBinding(trinket);
        View.PrintVendorValue(trinket);
    }

    private void PrintGizmo(Gizmo gizmo)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(gizmo, Upgrades);
        View.PrintDescription(gizmo.Description, gizmo.Level > 0);
        View.PrintRequiredLevel(gizmo.Level);
        View.PrintInBank();
        View.PrintUniqueness(gizmo);
        View.PrintItemBinding(gizmo);
        View.PrintVendorValue(gizmo);
    }

    private void PrintJadeTechModule(JadeTechModule jadeTechModule)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(jadeTechModule, Upgrades);
        View.PrintDescription(jadeTechModule.Description);
        View.PrintItemRarity(jadeTechModule.Rarity);
        View.PrintPlainText("Module");
        View.PrintRequiredLevel(jadeTechModule.Level);
        View.PrintPlainText("Required Mastery: Jade Bots");
        View.PrintInBank();
        View.PrintUniqueness(jadeTechModule);
        View.PrintItemBinding(jadeTechModule);
        View.PrintVendorValue(jadeTechModule);
    }

    private void PrintMiniature(Miniature miniature)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(miniature, Upgrades);
        View.PrintDescription(miniature.Description);
        View.PrintMini(miniature.MiniatureId);
        View.PrintPlainText("Mini");
        View.PrintInBank();
        View.PrintUniqueness(miniature);
        View.PrintItemBinding(miniature);
        View.PrintVendorValue(miniature);
    }

    private void PrintPowerCore(PowerCore powerCore)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(powerCore, Upgrades);
        View.PrintDescription(powerCore.Description, true);
        View.PrintItemRarity(powerCore.Rarity);
        View.PrintPlainText("Power Core");
        View.PrintRequiredLevel(powerCore.Level);
        View.PrintInBank();
        View.PrintUniqueness(powerCore);
        View.PrintItemBinding(powerCore);
        View.PrintVendorValue(powerCore);
    }

    private void PrintRelic(Relic relic)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(relic, Upgrades);
        View.PrintDescription(relic.Description, true);
        View.PrintItemRarity(relic.Rarity);
        View.PrintPlainText("Relic");
        View.PrintRequiredLevel(relic.Level);
        View.PrintInBank();
        View.PrintUniqueness(relic);
        View.PrintItemBinding(relic);
        View.PrintVendorValue(relic);
    }

    private void PrintSalvageTool(SalvageTool salvageTool)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(salvageTool, Upgrades);
        View.PrintPlainText(" ");
        View.PrintItemRarity(salvageTool.Rarity);
        View.PrintPlainText("Consumable");
        View.PrintDescription(salvageTool.Description);
        View.PrintInBank();
        View.PrintUniqueness(salvageTool);
        View.PrintItemBinding(salvageTool);
        View.PrintVendorValue(salvageTool);
    }

    private void PrintTrophy(Trophy trophy)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(trophy, Upgrades);
        View.PrintDescription(trophy.Description);
        View.PrintPlainText("Trophy");
        View.PrintInBank();
        View.PrintUniqueness(trophy);
        View.PrintItemBinding(trophy);
        View.PrintVendorValue(trophy);
    }

    private void PrintUpgradeComponent(UpgradeComponent upgradeComponent)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(upgradeComponent, Upgrades);
        if (upgradeComponent is Rune rune)
        {
            View.PrintBonuses(rune.Bonuses ?? []);
        }
        else if (upgradeComponent.Buff is { Description.Length: > 0 })
        {
            View.PrintBuff(upgradeComponent.Buff);
        }
        else
        {
            View.PrintAttributes(upgradeComponent.Attributes.ToDictionary(
                stat => AttributeName(stat.Key),
                stat => stat.Value
            ));
        }

        View.PrintDescription(upgradeComponent.Description);
        View.PrintRequiredLevel(upgradeComponent.Level);
        View.PrintInBank();
        View.PrintUniqueness(upgradeComponent);
        View.PrintItemBinding(upgradeComponent);
        View.PrintVendorValue(upgradeComponent);
    }

    private void PrintWeapon(Weapon weapon)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(weapon, Upgrades);
        View.PrintWeaponStrength(weapon);
        View.PrintDefense(weapon.Defense);
        View.PrintAttributes(weapon.Attributes.ToDictionary(
            stat => AttributeName(stat.Key),
            stat => stat.Value
        ));

        View.PrintUpgrades(weapon, weapon.Flags, Icons, Upgrades);
        View.PrintItemSkin(weapon.DefaultSkinId);
        View.PrintItemRarity(weapon.Rarity);
        switch (weapon)
        {
            case Axe:
                View.PrintPlainText("Axe");
                break;
            case Dagger:
                View.PrintPlainText("Dagger");
                break;
            case Focus:
                View.PrintPlainText("Focus");
                break;
            case Greatsword:
                View.PrintPlainText("Greatsword");
                break;
            case Hammer:
                View.PrintPlainText("Hammer");
                break;
            case HarpoonGun:
                View.PrintPlainText("Harpoon Gun");
                break;
            case LargeBundle:
                View.PrintPlainText("Large Bundle");
                break;
            case Longbow:
                View.PrintPlainText("Longbow");
                break;
            case Mace:
                View.PrintPlainText("Mace");
                break;
            case Pistol:
                View.PrintPlainText("Pistol");
                break;
            case Rifle:
                View.PrintPlainText("Rifle");
                break;
            case Scepter:
                View.PrintPlainText("Scepter");
                break;
            case Shield:
                View.PrintPlainText("Shield");
                break;
            case Shortbow:
                View.PrintPlainText("Shortbow");
                break;
            case SmallBundle:
                View.PrintPlainText("Small Bundle");
                break;
            case Spear:
                View.PrintPlainText("Spear");
                break;
            case Staff:
                View.PrintPlainText("Staff");
                break;
            case Sword:
                View.PrintPlainText("Sword");
                break;
            case Torch:
                View.PrintPlainText("Torch");
                break;
            case Toy:
            case ToyTwoHanded:
                View.PrintPlainText("Toy");
                break;
            case Trident:
                View.PrintPlainText("Trident");
                break;
            case Warhorn:
                View.PrintPlainText("Warhorn");
                break;
        }

        View.PrintRequiredLevel(weapon.Level);
        View.PrintDescription(weapon.Description);
        View.PrintInBank();
        View.PrintStatChoices(weapon);
        View.PrintUniqueness(weapon);
        View.PrintItemBinding(weapon);
        View.PrintVendorValue(weapon);
    }

    private void Print(Item item)
    {
        EnsureUpgradesNotNull();
        View.PrintHeader(item, Upgrades);
        View.PrintDescription(item.Description);
        View.PrintInBank();
        View.PrintUniqueness(item);
        View.PrintItemBinding(item);
        View.PrintVendorValue(item);
    }
}
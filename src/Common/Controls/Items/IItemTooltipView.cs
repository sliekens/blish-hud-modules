using Blish_HUD.Common.UI.Views;

using GuildWars2;
using GuildWars2.Hero;
using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.Common.Controls.Items.Services;

namespace SL.Common.Controls.Items;

public interface IItemTooltipView : ITooltipView
{
    void PrintPlainText(string text, Color? textColor = null);

    void PrintHeader(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades);

    void PrintDefense(int defense);

    void PrintAttributes(IReadOnlyDictionary<string, int> attributes);

    void PrintUpgrades(IUpgradable item, ItemFlags flags, ItemIcons icons, IReadOnlyDictionary<int, UpgradeComponent> upgrades);

    void PrintItemSkin(int skinId);

    void PrintItemRarity(Extensible<Rarity> rarity);

    void PrintWeightClass(Extensible<WeightClass> weightClass);

    void PrintRequiredLevel(int level);

    void PrintDescription(string description, bool finalNewLine = false);

    void PrintInBank();

    void PrintItemBinding(Item item);

    void PrintVendorValue(Item item);

    void PrintEffect(Effect effect);

    void PrintMini(int miniatureId);

    void PrintBuff(Buff buff);

    void PrintBonuses(IReadOnlyList<string> bonuses);

    void PrintWeaponStrength(Weapon weapon);

    void PrintStatChoices(ICombatEquipment equipment);

    void PrintUniqueness(Item item);
}
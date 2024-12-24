using GuildWars2.Items;

namespace SL.Common;

public static class ItemExtensions
{
    public static int? SuffixItemId(this Item item)
    {
        return item switch
        {
            Armor armor => armor.SuffixItemId,
            Weapon weapon => weapon.SuffixItemId,
            Backpack back => back.SuffixItemId,
            Trinket trinket => trinket.SuffixItemId,
            _ => null
        };
    }
    public static int? SecondarySuffixItemId(this Item item)
    {
        return item switch
        {
            Weapon weapon => weapon.SecondarySuffixItemId,
            _ => null
        };
    }

    public static UpgradeComponent? SuffixItem(this Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        var suffixItemId = item.SuffixItemId();
        if (suffixItemId.HasValue && upgrades.TryGetValue(suffixItemId.Value, out UpgradeComponent suffixItem))
        {
            return suffixItem;
        }

        return null;
    }

    public static UpgradeComponent? SecondarySuffixItem(this Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        var secondarySuffixItemId = item.SecondarySuffixItemId();
        if (secondarySuffixItemId.HasValue && upgrades.TryGetValue(secondarySuffixItemId.Value, out UpgradeComponent secondarySuffixItem))
        {
            return secondarySuffixItem;
        }

        return null;
    }

    public static string NameWithoutSuffix(this Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        var name = item.Name;
        if (!item.Flags.HideSuffix)
        {
            var suffixItem = item.SuffixItem(upgrades);
            if (suffixItem is not null)
            {
                name = name.Replace($" {suffixItem.SuffixName}", "");
            }
        }

        return name;
    }
}
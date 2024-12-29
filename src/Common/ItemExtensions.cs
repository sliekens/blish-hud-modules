using GuildWars2.Items;

namespace SL.Common;

public static class ItemExtensions
{
    public static int? SuffixItemId(this Item item)
    {
        if (item is IUpgradable upgradable)
        {
            return upgradable.SuffixItemId;
        }

        return null;
    }

    public static int? SecondarySuffixItemId(this Item item)
    {
        if (item is IUpgradable upgradable)
        {
            return upgradable.SecondarySuffixItemId;
        }

        return null;
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
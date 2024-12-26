using System.Collections.ObjectModel;

using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items;

public sealed class ItemsTabModel
{
    private readonly List<Item> _default = [];

    private readonly Dictionary<int, UpgradeComponent> _upgrades = [];

    public IEnumerable<Item> DefaultOptions => _default.AsReadOnly();

    public IReadOnlyDictionary<int, UpgradeComponent> Upgrades => new ReadOnlyDictionary<int, UpgradeComponent>(_upgrades);

    public void AddUpgrade(UpgradeComponent upgrade)
    {
        _upgrades.Add(upgrade.Id, upgrade);
    }

    public void AddDefaultOption(Item item)
    {
        _default.Add(item);
    }

    public void ClearUpgrades()
    {
        _upgrades.Clear();
    }

    public void ClearDefaultOptions()
    {
        _default.Clear();
    }
}
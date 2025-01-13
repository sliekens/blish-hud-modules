using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;

using UpgradeSlot = SL.ChatLinks.UI.Tabs.Items2.Tooltips.UpgradeSlot;

namespace SL.ChatLinks.UI.Tabs.Items2.Collections;

public sealed class ItemsListViewModel(
    ItemIcons icons,
    Customizer customizer,
    Item item,
    ItemTooltipViewModelFactory tooltipViewModelFactory,
    bool isSelected
) : ViewModel
{
    private bool _isSelected = isSelected;

    public Item Item { get; } = item ?? throw new ArgumentNullException(nameof(item));

    public Color Color { get; } = ItemColors.Rarity(item.Rarity);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetField(ref _isSelected, value);
    }

    public AsyncTexture2D? GetIcon()
    {
        return icons.GetIcon(item);
    }

    public ItemTooltipViewModel CreateTooltipViewModel()
    {
        List<UpgradeSlot> upgrades = [];
        if (Item is IUpgradable upgradable)
        {
            upgrades.AddRange(UpgradeSlots(upgradable));
            upgrades.AddRange(InfusionSlots(upgradable));
        }

        return tooltipViewModelFactory.Create(item, upgrades);
    }

    private IEnumerable<UpgradeSlot> UpgradeSlots(IUpgradable upgradable)
    {
        return upgradable.UpgradeSlots
            .Select(upgradeComponentId => new UpgradeSlot
            {
                Type = UpgradeSlotType.Default,
                UpgradeComponent = customizer.GetUpgradeComponent(upgradeComponentId)
            });
    }
    private IEnumerable<UpgradeSlot> InfusionSlots(IUpgradable upgradable)
    {
        return upgradable.InfusionSlots
            .Select(infusionSlot => new UpgradeSlot
            {
                Type = infusionSlot.Flags switch
                {
                    { Infusion: true } => UpgradeSlotType.Infusion,
                    { Enrichment: true } => UpgradeSlotType.Enrichment,
                    _ => UpgradeSlotType.Default
                },
                UpgradeComponent = customizer.GetUpgradeComponent(infusionSlot.ItemId)
            });
    }
}
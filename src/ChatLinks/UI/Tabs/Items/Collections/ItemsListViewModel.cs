using System.Diagnostics;
using System.Net;

using Blish_HUD.Content;

using GuildWars2.Items;

using Microsoft.Xna.Framework;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;
using SL.Common.ModelBinding;

using UpgradeSlot = SL.ChatLinks.UI.Tabs.Items.Tooltips.UpgradeSlot;

namespace SL.ChatLinks.UI.Tabs.Items.Collections;

public sealed class ItemsListViewModel(
    IClipBoard clipboard,
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

    public RelayCommand ToggleCommand => new(() => IsSelected = !IsSelected);

    public RelayCommand CopyNameCommand => new(() => clipboard.SetText(Item.Name));

    public RelayCommand CopyChatLinkCommand => new(() => clipboard.SetText(Item.ChatLink));

    public RelayCommand OpenWikiCommand => new(() => Process.Start($"https://wiki.guildwars2.com/wiki/?search={WebUtility.UrlEncode(item.ChatLink)}"));

    public RelayCommand OpenApiCommand =>
        new(() => Process.Start($"https://api.guildwars2.com/v2/items/{item.Id}?v=latest"));

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

        return tooltipViewModelFactory.Create(item, 1, upgrades);
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
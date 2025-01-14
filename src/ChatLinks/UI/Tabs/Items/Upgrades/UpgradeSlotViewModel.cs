using Blish_HUD.Content;

using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items.Tooltips;
using SL.Common;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSlotViewModel(
    UpgradeSlotType type,
    ItemIcons icons,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory
) : ViewModel
{
    private UpgradeSlotType _type = type;

    private UpgradeComponent? _selectedUpgradeComponent;

    private UpgradeComponent? _defaultUpgradeComponent;

    public UpgradeSlotType Type
    {
        get => _type;
        set => SetField(ref _type, value);
    }

    public UpgradeComponent? DefaultUpgradeComponent
    {
        get => _defaultUpgradeComponent;
        set => SetField(ref _defaultUpgradeComponent, value);
    }

    public UpgradeComponent? SelectedUpgradeComponent
    {
        get => _selectedUpgradeComponent;
        set => SetField(ref _selectedUpgradeComponent, value);
    }

    public AsyncTexture2D? GetIcon(UpgradeComponent item) => icons.GetIcon(item);

    public ItemTooltipViewModel CreateTooltipViewModel(UpgradeComponent item) => itemTooltipViewModelFactory.Create(item, 1, []);
}
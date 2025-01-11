using Blish_HUD.Content;

using GuildWars2.Items;

using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Content.Upgrades;

public sealed class UpgradeSlotViewModel(
    UpgradeSlotType type,
    ItemIcons icons
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

    public AsyncTexture2D? Icon(UpgradeComponent item) => icons.GetIcon(item);
}
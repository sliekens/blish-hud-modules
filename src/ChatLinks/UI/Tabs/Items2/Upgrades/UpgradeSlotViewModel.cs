using System.Windows.Input;

using Blish_HUD.Content;

using GuildWars2.Items;

using SL.ChatLinks.UI.Tabs.Items2.Tooltips;
using SL.Common;
using SL.Common.Controls.Items.Services;
using SL.Common.Controls.Items.Upgrades;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Items2.Upgrades;

public sealed class UpgradeSlotViewModel(
    UpgradeSlotType type,
    ItemIcons icons,
    ItemTooltipViewModelFactory itemTooltipViewModelFactory
) : ViewModel
{
    public event EventHandler? Customizing;

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

    public ICommand CustomizeCommand => new RelayCommand(OnCustomize);

    private void OnCustomize()
    {
        Customizing?.Invoke(this, EventArgs.Empty);
    }

    public AsyncTexture2D? GetIcon(UpgradeComponent item) => icons.GetIcon(item);

    public ItemTooltipViewModel CreateTooltipViewModel(UpgradeComponent item) => itemTooltipViewModelFactory.Create(item);
}
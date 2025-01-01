using GuildWars2.Items;

namespace SL.Common.Controls.Items.Upgrades;

public class UpgradeSlotViewModel : ViewModel
{
    private UpgradeComponent? _selectedUpgradeComponent;

    private UpgradeComponent? _defaultUpgradeComponent;

    public event Action? Customized;

    public event Action? Cleared;

    public UpgradeSlotType? Type { get; set; }

    public ItemIcons Icons { get; set; } = ServiceLocator.Resolve<ItemIcons>();

    public UpgradeComponent? DefaultUpgradeComponent
    {
        get => _defaultUpgradeComponent;
        set
        {
            SetField(ref _defaultUpgradeComponent, value);
        }
    }

    public UpgradeComponent? SelectedUpgradeComponent
    {
        get => _selectedUpgradeComponent;
        set
        {
            SetField(ref _selectedUpgradeComponent, value);
        }
    }

    public void OnCustomize()
    {
        Customized?.Invoke();
    }

    public void OnClear()
    {
        SelectedUpgradeComponent = null;
        Cleared?.Invoke();
    }
}
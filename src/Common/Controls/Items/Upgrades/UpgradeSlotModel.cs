using GuildWars2.Items;

namespace SL.Common.Controls.Items.Upgrades;

public class UpgradeSlotModel
{
    public UpgradeSlotType Type { get; set; }

    public UpgradeComponent? DefaultUpgradeComponent { get; set; }

    public UpgradeComponent? SelectedUpgradeComponent { get; set; }

    public UpgradeComponent? EffectiveUpgrade => SelectedUpgradeComponent ?? DefaultUpgradeComponent;
}
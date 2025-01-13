using GuildWars2.Items;

using SL.Common.Controls.Items.Upgrades;

namespace SL.ChatLinks.UI.Tabs.Items2.Tooltips;

public sealed class UpgradeSlot
{
    public UpgradeSlotType Type { get; set; }

    public UpgradeComponent? UpgradeComponent { get; set; }
}
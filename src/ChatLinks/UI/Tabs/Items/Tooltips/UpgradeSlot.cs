using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Tooltips;

public sealed class UpgradeSlot
{
    public UpgradeSlotType Type { get; set; }

    public UpgradeComponent? UpgradeComponent { get; set; }
}
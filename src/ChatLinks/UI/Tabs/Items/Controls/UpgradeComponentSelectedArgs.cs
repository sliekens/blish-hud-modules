using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Controls;

public class UpgradeComponentSelectedArgs(UpgradeComponent? selected) : EventArgs
{
    public UpgradeComponent? Selected { get; } = selected;
}
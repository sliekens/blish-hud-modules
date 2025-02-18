using GuildWars2.Items;

namespace SL.ChatLinks.UI.Tabs.Items.Upgrades;

public sealed class UpgradeSelectedEventArgs : EventArgs
{
    public required UpgradeComponent Selected { get; init; }
}

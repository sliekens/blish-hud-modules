using Blish_HUD.Content;

using GuildWars2.Items;

namespace SL.Common.Controls.Items.Upgrades;

public class UsedUpgradeSlot
{
    public required UpgradeComponent UpgradeComponent { get; set; }

    public required AsyncTexture2D? Icon { get; set; }

    public required bool IsDefault { get; set; }
}
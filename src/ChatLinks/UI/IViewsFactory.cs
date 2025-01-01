using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

namespace SL.ChatLinks.UI;

public interface IViewsFactory
{
    IView CreateItemsTabView();

    IView CreateItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades);
}
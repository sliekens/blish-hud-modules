using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

namespace SL.ChatLinks.UI;

public interface IViewsFactory
{
    IView CreateItemsTabView();

    ITooltipView CreateItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades);
}
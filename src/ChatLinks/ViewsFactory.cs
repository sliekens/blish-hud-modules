using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Extensions.DependencyInjection;

using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items;
using SL.Common.Controls.Items;

namespace SL.ChatLinks;

public sealed class ViewsFactory(IServiceProvider serviceProvider) : IViewsFactory
{
    public IView CreateItemsTabView()
    {
        return new AsyncView(() => ActivatorUtilities.CreateInstance<ItemsTabView>(serviceProvider));
    }

    public ITooltipView CreateItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        return ActivatorUtilities.CreateInstance<ItemTooltipView>(serviceProvider, item, upgrades);
    }
}
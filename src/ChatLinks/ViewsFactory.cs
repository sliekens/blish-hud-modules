using Blish_HUD.Common.UI.Views;
using Blish_HUD.Graphics.UI;

using GuildWars2.Items;

using Microsoft.Extensions.DependencyInjection;

using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items2;

using ItemTooltipView = SL.Common.Controls.Items.ItemTooltipView;

namespace SL.ChatLinks;

public sealed class ViewsFactory(IServiceProvider serviceProvider) : IViewsFactory
{
    public IView CreateItemsTabView()
    {
        return new AsyncView(() => ActivatorUtilities.CreateInstance<ItemsTabView>(serviceProvider));
    }

    public IView CreateItemsTabView2()
    {
        return new AsyncView(serviceProvider.GetRequiredService<ItemsTabView2>);
    }

    public ITooltipView CreateItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        return ActivatorUtilities.CreateInstance<ItemTooltipView>(serviceProvider, item, upgrades);
    }
}
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
        return new AsyncView(() =>
        {
            ItemsTabView view = ActivatorUtilities.CreateInstance<ItemsTabView>(serviceProvider);
            ItemsTabModel model = ActivatorUtilities.CreateInstance<ItemsTabModel>(serviceProvider);
            view.WithPresenter(ActivatorUtilities.CreateInstance<ItemsTabPresenter>(serviceProvider, view, model));
            return view;
        });
    }

    public IView CreateItemTooltipView(Item item, IReadOnlyDictionary<int, UpgradeComponent> upgrades)
    {
        var icons = serviceProvider.GetRequiredService<ItemIcons>();
        return new AsyncView(() => new ItemTooltipView(item, icons, upgrades));
    }
}
using Blish_HUD.Graphics.UI;

using Microsoft.Extensions.DependencyInjection;

using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items;
using SL.ChatLinks.UI.Tabs.Items.Services;

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
}
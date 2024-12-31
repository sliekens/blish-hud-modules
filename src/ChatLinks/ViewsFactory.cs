using Blish_HUD.Graphics.UI;

using Microsoft.Extensions.DependencyInjection;

using SL.ChatLinks.UI;
using SL.ChatLinks.UI.Tabs.Items.Services;

namespace SL.ChatLinks;

public sealed class ViewsFactory(IServiceProvider serviceProvider) : IViewsFactory
{
    public IView CreateItemsTabView()
    {
        return new AsyncView(serviceProvider.GetRequiredService<IItemsTabView>());
    }
}
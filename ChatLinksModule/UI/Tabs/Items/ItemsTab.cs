using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Items;

public sealed class ItemsTab : Tab
{
	private ItemsTab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null)
		: base(icon, view, name, priority)
	{
	}

	public static ItemsTab Create(ModuleParameters parameters)
	{
		var icon = AsyncTexture2D.FromAssetId(156699);
		var view = () => ItemsView.Create(parameters);
		var name = "Items";
		var priority = TabPriority.ItemsTab;
		return new ItemsTab(icon, view, name, priority);
	}
}
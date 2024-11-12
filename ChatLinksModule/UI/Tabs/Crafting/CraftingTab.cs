using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Crafting;

public sealed class CraftingTab : Tab
{
	private CraftingTab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) : base(icon,
		view, name, priority)
	{
	}

	public static CraftingTab Create(ModuleParameters parameters)
	{
		var icon = AsyncTexture2D.FromAssetId(156711);
		var view = () => CraftingView.Create(parameters);
		var name = "Crafting";
		var priority = TabPriority.CraftingTab;
		return new CraftingTab(icon, view, name, priority);
	}
}
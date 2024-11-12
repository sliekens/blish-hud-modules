using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Crafting;

public class CraftingView : View
{
	private CraftingView()
	{
	}

	public static CraftingView Create(ModuleParameters parameters)
	{
		return new CraftingView();
	}

	protected override void Build(Container buildPanel)
	{
	}
}
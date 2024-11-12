using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsView : View
{
	private ItemsView()
	{
	}

	public static ItemsView Create(ModuleParameters parameters)
	{
		return new ItemsView();
	}

	protected override void Build(Container buildPanel)
	{
	}
}
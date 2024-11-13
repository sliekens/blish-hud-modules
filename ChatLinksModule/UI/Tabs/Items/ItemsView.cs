using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Items;

public class ItemsView : View
{
	private TextBox _searchBox;

	private ItemsView()
	{
	}

	public static ItemsView Create(ModuleParameters parameters)
	{
		return new ItemsView();
	}

	protected override void Build(Container buildPanel)
	{
		_searchBox = new TextBox
		{
			Parent = buildPanel
		};

		_searchBox.TextChanged += SearchInput;
	}

	private async void SearchInput(object sender, System.EventArgs e)
	{
		ScreenNotification.ShowNotification(_searchBox.Text);
	}
}
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using ChatLinksModule.UI.Tabs.Achievements;
using ChatLinksModule.UI.Tabs.Crafting;
using ChatLinksModule.UI.Tabs.Items;
using Microsoft.Xna.Framework;

namespace ChatLinksModule.UI;

public sealed class MainWindow : TabbedWindow2
{
	private readonly AsyncEmblem _emblem;

	private MainWindow(AsyncTexture2D background, Rectangle windowRegion,
		Rectangle contentRegion, AsyncTexture2D emblem)
		: base(background, windowRegion, contentRegion)
	{
		_emblem = AsyncEmblem.Attach(this, emblem);
		TabChanged += MainWindow_TabChanged;
	}

	private void MainWindow_TabChanged(object sender, ValueChangedEventArgs<Tab> e)
	{
		Subtitle = e.NewValue switch
		{
			ItemsTab => "Items",
			AchievementsTab => "Achievements",
			CraftingTab => "Crafting",
			_ => ""
		};
	}

	public static MainWindow Create(ModuleParameters parameters)
	{
		var emblem = AsyncTexture2D.FromAssetId(2237584);
		var background = AsyncTexture2D.FromAssetId(155985);
		var windowRegion = new Rectangle(0, 26, 953, 691);
		var contentRegion = new Rectangle(70, 71, 839, 605);
		var window = new MainWindow(background, windowRegion, contentRegion, emblem)
		{
			Parent = Graphics.SpriteScreen,
			Title = "Chat links",
			Id = "sliekens.chat-links.main-window",
			Location = new Point(300, 300),
			CanResize = true
		};

		window.Tabs.Add(ItemsTab.Create(parameters));
		window.Tabs.Add(AchievementsTab.Create(parameters));
		window.Tabs.Add(CraftingTab.Create(parameters));

		return window;
	}


	protected override void DisposeControl()
	{
		TabChanged -= MainWindow_TabChanged;
		_emblem.Dispose();
		base.DisposeControl();
	}
}
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;
using Microsoft.Xna.Framework;

namespace ChatLinksModule;

public sealed class MyMainWindow : TabbedWindow2
{
	private readonly AsyncEmblem _emblem;

	private MyMainWindow(AsyncTexture2D background, Rectangle windowRegion,
		Rectangle contentRegion, AsyncTexture2D emblem)
		: base(background, windowRegion, contentRegion)
	{
		_emblem = AsyncEmblem.Attach(this, emblem);
	}

	public static MyMainWindow Create(ModuleParameters parameters)
	{
		var emblem = AsyncTexture2D.FromAssetId(2237584);
		var background = AsyncTexture2D.FromAssetId(155985);
		var windowRegion = new Rectangle(0, 26, 953, 691);
		var contentRegion = new Rectangle(70, 71, 839, 605);
		return new MyMainWindow(background, windowRegion, contentRegion, emblem)
		{
			Parent = Graphics.SpriteScreen,
			Title = "My module",
			Subtitle = "My module subtitle",
			Id = "bhm.my-module.my-main-window",
			Location = new Point(300, 300),
			CanResize = true
		};
	}

	protected override void DisposeControl()
	{
		_emblem.Dispose();
		base.DisposeControl();
	}
}
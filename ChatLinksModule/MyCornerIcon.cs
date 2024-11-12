using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Modules;

namespace ChatLinksModule;

public sealed class MyCornerIcon : CornerIcon
{
	private MyCornerIcon(
		AsyncTexture2D icon,
		AsyncTexture2D hoverIcon,
		string iconName)
		: base(icon, hoverIcon, iconName)
	{
	}

	public static MyCornerIcon Create(ModuleParameters parameters)
	{
		var icon = AsyncTexture2D.FromAssetId(155156);
		var hoverIcon = AsyncTexture2D.FromAssetId(155157);
		var iconName = "My module";
		return new MyCornerIcon(icon, hoverIcon, iconName)
		{
			Parent = Graphics.SpriteScreen,
			Priority = 745727698
		};
	}
}
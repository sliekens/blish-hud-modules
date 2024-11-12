using System;
using Blish_HUD.Content;
using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Achievements;

public class AchievementsTab : Tab
{
	private AchievementsTab(AsyncTexture2D icon, Func<IView> view, string name = null, int? priority = null) : base(
		icon, view, name, priority)
	{
	}

	public static AchievementsTab Create(ModuleParameters parameters)
	{
		var icon = AsyncTexture2D.FromAssetId(156710);
		var view = () => AchievementsView.Create(parameters);
		var name = "Achievements";
		var priority = TabPriority.AchievementsTab;
		return new AchievementsTab(icon, view, name, priority)
		{
			Enabled = false
		};
	}
}
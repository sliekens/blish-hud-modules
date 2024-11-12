using Blish_HUD.Controls;
using Blish_HUD.Graphics.UI;
using Blish_HUD.Modules;

namespace ChatLinksModule.UI.Tabs.Achievements;

public class AchievementsView : View
{
	private AchievementsView()
	{
	}

	public static AchievementsView Create(ModuleParameters parameters)
	{
		return new AchievementsView();
	}

	protected override void Build(Container buildPanel)
	{
	}
}
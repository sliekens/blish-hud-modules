using Blish_HUD.Content;
using Blish_HUD.Controls;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public class AchievementsTab : Tab
{
    public AchievementsTab(Func<AchievementsView> view)
        : base(AsyncTexture2D.FromAssetId(156710), view, "Achievements", TabPriority.AchievementsTab)
    {
    }
}

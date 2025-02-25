using Microsoft.Extensions.DependencyInjection;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementsTabViewModelFactory(IServiceProvider sp)
{
    public AchievementsTabViewModel Create()
    {
        return ActivatorUtilities.CreateInstance<AchievementsTabViewModel>(sp);
    }
}

using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.DependencyInjection;

namespace SL.ChatLinks.UI.Tabs.Achievements.Tooltips;

public sealed class AchievementTooltipViewModelFactory(IServiceProvider sp)
{
    public AchievementTooltipViewModel Create(Achievement achievement)
    {
        return ActivatorUtilities.CreateInstance<AchievementTooltipViewModel>(sp, achievement);
    }
}

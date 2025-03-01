using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;

using Microsoft.Extensions.DependencyInjection;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementTileViewModelFactory(IServiceProvider sp)
{
    public AchievementTileViewModel Create(Achievement achievement, AchievementCategory? category, AccountAchievement? accountAchievement)
    {
        AchievementTileViewModel vm = ActivatorUtilities.CreateInstance<AchievementTileViewModel>(sp, achievement);
        vm.Category = category;
        vm.AccountAchievement = accountAchievement;
        return vm;
    }
}

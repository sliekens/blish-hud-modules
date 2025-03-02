using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

using Microsoft.Extensions.DependencyInjection;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementTileViewModelFactory(IServiceProvider sp)
{
    public AchievementTileViewModel Create(
        Achievement achievement,
        AchievementCategory? category,
        AchievementGroup? group,
        IReadOnlyList<AccountAchievement>? progression)
    {
        AchievementTileViewModel vm = ActivatorUtilities.CreateInstance<AchievementTileViewModel>(sp, achievement);
        vm.Category = category;
        vm.Group = group;
        vm.Progression = progression;
        return vm;
    }
}

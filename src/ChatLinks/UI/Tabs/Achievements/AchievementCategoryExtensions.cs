using GuildWars2.Hero.Achievements.Categories;

namespace SL.ChatLinks.UI.Tabs.Achievements;
internal static class AchievementCategoryExtensions
{
    public static bool? IsParentOf(this AchievementCategory category, int achievementId)
    {
        ThrowHelper.ThrowIfNull(category);

        if (category.Achievements.Any(achievement => achievement.Id == achievementId))
        {
            return true;
        }

        if (category.Tomorrow is null)
        {
            return false;
        }

        if (category.Tomorrow.Any(achievement => achievement.Id == achievementId))
        {
            return true;
        }

        // Can't say for sure
        // the achievement might be in a daily category but not currently in the rotation
        return null;
    }
}

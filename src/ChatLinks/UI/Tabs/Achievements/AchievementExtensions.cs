using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal static class AchievementExtensions
{
    public static Uri? IconUrl(this Achievement achievement)
    {
        ThrowHelper.ThrowIfNull(achievement);
        return !string.IsNullOrEmpty(achievement.IconHref)
            ? new Uri(achievement.IconHref!)
            : null;
    }

    public static Uri? IconUrl(this AchievementCategory category)
    {
        ThrowHelper.ThrowIfNull(category);
        return !string.IsNullOrEmpty(category.IconHref)
            ? new Uri(category.IconHref!)
            : null;
    }

    public static bool IsLocked(
    this Achievement achievement,
    AchievementGroup? group,
    IReadOnlyList<AccountAchievement>? progression)
    {
        // Per-character achievement progress is always unavailable
        // so assume is is unlocked
        if (progression is not null && group?.IsPerCharacter() == true)
        {
            return false;
        }

        if (achievement.Flags.RequiresUnlock)
        {
            AccountAchievement? progress = progression?
                .SingleOrDefault(progress => progress.Id == achievement.Id);
            return progress?.Unlocked != true;
        }

        if (achievement.Prerequisites.Count > 0)
        {
            List<AccountAchievement?> prerequisites = [
                .. achievement.Prerequisites
                    .Select(pre => progression?.SingleOrDefault(progress => progress.Id == pre))
            ];

            // Not all progress is in the API...
            return !prerequisites.All(progress => progress?.Done ?? false);
        }

        return false;
    }

    public static bool IsHidden(
        this Achievement achievement,
        IReadOnlyList<AccountAchievement>? progression)
    {

        if (achievement.Flags.Hidden)
        {
            bool? hasProgress = progression?.Any(progress => progress.Id == achievement.Id);
            return !(hasProgress ?? false);
        }

        return false;
    }
}

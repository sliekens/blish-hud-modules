using GuildWars2.Hero.Achievements;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal static class AchievementExtensions
{
    public static bool IsLocked(this Achievement achievement, IReadOnlyList<AccountAchievement>? progression)
    {
        if (achievement.Flags.RequiresUnlock)
        {
            AccountAchievement? progress = progression?
                .SingleOrDefault(progress => progress.Id == achievement.Id);
            return progress is null || !progress.Unlocked;
        }

        if (achievement.Prerequisites.Count > 0)
        {
            List<AccountAchievement>? prerequisites = progression?
                .Where(progress => achievement.Prerequisites.Contains(progress.Id))
                .ToList();
            return prerequisites is null || prerequisites.Any(pre => !pre.Done);
        }

        return false;
    }
}

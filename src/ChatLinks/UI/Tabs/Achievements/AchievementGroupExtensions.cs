using GuildWars2.Hero.Achievements.Groups;

namespace SL.ChatLinks.UI.Tabs.Achievements;

internal static class AchievementGroupExtensions
{
    public static bool IsPerCharacter(this AchievementGroup group)
    {
        return group.Id == "EFADEE67-588F-412F-A1BD-6C9AFF782988";
    }
}

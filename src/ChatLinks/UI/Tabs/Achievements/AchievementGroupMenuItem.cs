using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public class AchievementGroupMenuItem
{
    public required AchievementGroup Group { get; set; }

    public required IEnumerable<AchievementCategory> Categories { get; set; }
}

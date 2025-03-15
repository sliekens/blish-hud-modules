using GuildWars2.Hero.Achievements;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Achievements.Tooltips;

public sealed class AchievementTooltipViewModel(
    ILogger<AchievementTooltipViewModel> logger,
    IStringLocalizer<AchievementTooltipView> localizer,
    IDbContextFactory contextFactory,
    ILocale locale,
    AccountUnlocks hero,
    Achievement achievement
) : ViewModel
{
    public delegate AchievementTooltipViewModel Factory(Achievement achievement);

    public IStringLocalizer<AchievementTooltipView> Localizer { get; } = localizer;

    public string Name => achievement.Name;

    public string Requirement
    {
        get
        {
            string requirement = achievement.Requirement;
            if (achievement.Tiers.Count > 0)
            {
                AchievementTier tier = achievement.Tiers[^1];
                if (tier is not null)
                {
                    string count = $" {tier.Count:N0} ";
                    requirement = requirement.Replace("  ", count);
                }

            }

            return requirement;
        }
    }

    public string Description => achievement.Description;
}

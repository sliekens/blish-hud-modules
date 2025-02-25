
using System.Collections.ObjectModel;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

using Microsoft.EntityFrameworkCore;

using SL.ChatLinks.Storage;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public class AchievementGroupMenuItem
{
    public required AchievementGroup Group { get; set; }

    public required IEnumerable<AchievementCategory> Categories { get; set; }
}

public sealed class AchievementsTabViewModel(IDbContextFactory contextFactory, ILocale locale) : ViewModel, IDisposable
{
    private ObservableCollection<AchievementGroupMenuItem> _groups = [];

    private ObservableCollection<Achievement> _achievements = [];

    public ObservableCollection<AchievementGroupMenuItem> MenuItems
    {
        get => _groups;
        private set => SetField(ref _groups, value);
    }

    public ObservableCollection<Achievement> Achievements
    {
        get => _achievements;
        private set => SetField(ref _achievements, value);
    }


    public async Task<bool> Load()
    {
        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            List<AchievementGroup> groups = await context.AchievementGroups
                .OrderBy(groups => groups.Order)
                .ToListAsync()
                .ConfigureAwait(false);

            List<AchievementCategory> categories = await context.AchievementCategories
                .OrderBy(category => category.Order)
                .ToListAsync()
                .ConfigureAwait(false);

            MenuItems = [.. groups.Select(group => new AchievementGroupMenuItem
                {
                    Group = group,
                    Categories = categories.Where(category => group.Categories.Contains(category.Id))
                })];
        }

        return true;
    }

    public void Dispose()
    {
    }

    public async Task SelectCategory(AchievementCategory category)
    {
        ThrowHelper.ThrowIfNull(category);

        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            IEnumerable<int> ids = category.Achievements.Select(r => r.Id);
            List<Achievement> achievements = await context.Achievements
                .Where(achievement => ids.Contains(achievement.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            Achievements = [.. achievements];
        }
    }
}

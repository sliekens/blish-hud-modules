
using System.Collections.ObjectModel;
using System.Windows.Input;

using Blish_HUD;
using Blish_HUD.Content;

using GuildWars2.Authorization;
using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using SL.ChatLinks.Storage;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementsTabViewModel(
    IEventAggregator eventAggregator,
    IDbContextFactory contextFactory,
    IStringLocalizer<AchievementsTabView> localizer,
    ILocale locale,
    AchievementTileViewModelFactory achievementTileViewModelFactory,
    AccountUnlocks unlocks
) : ViewModel, IDisposable
{
    private ObservableCollection<AchievementGroupMenuItem> _groups = [];

    private ObservableCollection<AchievementTileViewModel> _achievements = [];

    private string? _headerText;

    private AsyncTexture2D? _headerIcon;

    private string _searchText = "";

    private AchievementCategory? _selectedCategory;

    public ObservableCollection<AchievementGroupMenuItem> MenuItems
    {
        get => _groups;
        private set => SetField(ref _groups, value);
    }

    public ObservableCollection<AchievementTileViewModel> Achievements
    {
        get => _achievements;
        private set => SetField(ref _achievements, value);
    }

    public string? HeaderText
    {
        get => _headerText;
        set => SetField(ref _headerText, value);
    }

    public AsyncTexture2D? HeaderIcon
    {
        get => _headerIcon;
        set => SetField(ref _headerIcon, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetField(ref _searchText, value);
    }

    public ICommand SearchCommand => new AsyncRelayCommand(async () =>
    {
        await Task.Run(OnSearch).ConfigureAwait(false);
    });

    public string CategoriesTitle => localizer["Categories"];

    public string SearchPlaceholder => localizer["Search"];

    public AchievementCategory? SelectedCategory
    {
        get => _selectedCategory;
        set => SetField(ref _selectedCategory, value);
    }

    public async Task<bool> Load()
    {
        await LoadAchievementCategories().ConfigureAwait(false);

        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
        eventAggregator.Subscribe<DatabaseSeeded>(OnDatabaseSeeded);

        return true;
    }

    private async Task LoadAchievementCategories()
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

            List<int> categorizedIds = [
                .. categories.SelectMany(c => (List<int>)[.. c.Achievements.Select(a => a.Id), .. c.Tomorrow?.Select(a => a.Id) ?? []])
            ];

            var achievements = await context.Achievements
                .Select(a => new { a.Id, a.Flags })
                .ToListAsync()
                .ConfigureAwait(false);

            categories.Add(new()
            {
                Id = -1,
                Name = localizer["Uncategorized"],
                Description = "",
                IconHref = "",
                Order = int.MaxValue,
                Achievements = [.. achievements
                    .Where(a => !a.Flags.Daily)
                    .Where(a => !a.Flags.Weekly)
                    .Where(a => !categorizedIds.Contains(a.Id))
                    .Select(a => new AchievementRef()
                    {
                        Id = a.Id,
                        Level = new()
                        {
                            Min = 0,
                            Max = 0
                        },
                        Flags = new()
                        {
                            PvE = !a.Flags.Pvp,
                            SpecialEvent = false,
                            Other = []
                        }
                    })],
                Tomorrow = []
            });

            groups.Add(new AchievementGroup
            {
                Id = "",
                Name = localizer["Uncategorized"],
                Description = "",
                Order = int.MaxValue,
                Categories = [-1]
            });

            MenuItems = [.. groups.Select(group => new AchievementGroupMenuItem
                {
                    Group = group,
                    Categories = categories.Where(category => group.Categories.Contains(category.Id))
                })];
        }
    }

    private async Task OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(SearchPlaceholder));
        OnPropertyChanged(nameof(CategoriesTitle));
        await LoadAchievementCategories().ConfigureAwait(false);
        if (SelectedCategory is null)
        {
            await OnSearch().ConfigureAwait(false);
        }
    }

    private async Task OnDatabaseSeeded(DatabaseSeeded seeded)
    {
        if (seeded.Updated["achievement_categories"] > 0)
        {
            await LoadAchievementCategories().ConfigureAwait(false);
        }

        if (SelectedCategory is null && seeded.Updated["achievements"] > 0)
        {
            await OnSearch().ConfigureAwait(false);
        }
    }

    public async Task OnSearch()
    {
        SelectedCategory = null;
        ObservableCollection<AchievementTileViewModel> results = [];

        string query = SearchText.Trim();
        if (query.Length >= 3)
        {
            ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
            await using (context.ConfigureAwait(false))
            {
                List<Achievement> achievements = await context.Achievements
                    .Where(achievement => EF.Functions.Like(achievement.Name, $"%{query}%"))
                    .OrderBy(achievement => achievement.Name)
                    .ToListAsync()
                    .ConfigureAwait(false);


                if (achievements.Count > 0)
                {
                    List<AchievementCategory> categories = await context.AchievementCategories
                        .ToListAsync()
                        .ConfigureAwait(false);

                    List<AchievementGroup> groups = await context.AchievementGroups
                        .ToListAsync()
                        .ConfigureAwait(false);

                    List<AccountAchievement>? progression = null;
                    if (unlocks.HasPermission(Permission.Progression))
                    {
                        progression = [
                            .. await unlocks.GetAccountAchievements(CancellationToken.None)
                                .ConfigureAwait(false)
                        ];
                    }

                    achievements = SortAchievements(achievements, categories, groups, progression);
                    foreach (Achievement achievement in achievements)
                    {
                        AchievementCategory? category = categories
                            .FirstOrDefault(category => category.IsParentOf(achievement.Id) == true);

                        AchievementGroup? group = null;
                        if (category is not null)
                        {
                            group = groups.FirstOrDefault(group => group.Categories.Contains(category.Id));
                        }

                        AchievementTileViewModel achievementTileViewModel = achievementTileViewModelFactory
                            .Create(achievement, category, group, progression);

                        results.Add(achievementTileViewModel);
                    }
                }
            }
        }

        HeaderIcon = AsyncTexture2D.FromAssetId(155061);
        HeaderText = SearchText;
        Achievements = results;
    }

    public async Task SelectCategory(AchievementCategory category)
    {
        ThrowHelper.ThrowIfNull(category);
        SelectedCategory = category;

        ChatLinksContext context = contextFactory.CreateDbContext(locale.Current);
        await using (context.ConfigureAwait(false))
        {
            List<AchievementGroup> groups = await context.AchievementGroups
                .ToListAsync()
                .ConfigureAwait(false);

            // Switch to client-side evaluation for json array filtering
            groups = [.. groups.Where(group => group.Categories.Contains(category.Id))];

            IEnumerable<int> ids = category.Achievements.Select(achievement => achievement.Id);
            List<Achievement> achievements = await context.Achievements
                .Where(achievement => ids.Contains(achievement.Id))
                .OrderBy(achievement => achievement.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            List<AccountAchievement>? progression = null;
            if (unlocks.HasPermission(Permission.Progression))
            {
                progression = [..
                    await unlocks.GetAccountAchievements(CancellationToken.None)
                        .ConfigureAwait(false)
                ];
            }

            achievements = SortAchievements(achievements, [category], groups, progression);
            HeaderText = !string.IsNullOrEmpty(category.Name) ? category.Name : null;
            HeaderIcon = !string.IsNullOrEmpty(category.IconHref) ? GameService.Content.GetRenderServiceTexture(category.IconHref) : null;
            Achievements = [
                .. achievements.Select(achievement => achievementTileViewModelFactory
                    .Create(
                        achievement,
                        category,
                        groups.FirstOrDefault(),
                        progression
                    )
                )
            ];
        }
    }

    private static List<Achievement> SortAchievements(
        List<Achievement> achievements,
        List<AchievementCategory> categories,
        List<AchievementGroup> groups,
        List<AccountAchievement>? progression)
    {
        return [.. from achievement in achievements
            let category = categories.FirstOrDefault(category => category.IsParentOf(achievement.Id) == true)
            let @group = groups.FirstOrDefault(x => category is not null && x.Categories.Contains(category.Id))
            let locked = achievement.IsLocked(@group, progression)
            let hidden = achievement.IsHidden(progression)
            orderby hidden,
                locked,
                @group?.Order ?? int.MaxValue,
                category?.Order ?? int.MaxValue,
                achievement.Flags.CategoryDisplay descending,
                achievement.Flags.MoveToTop descending
            select achievement
        ];
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}

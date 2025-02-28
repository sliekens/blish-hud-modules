
using System.Collections.ObjectModel;
using System.Windows.Input;

using Blish_HUD;
using Blish_HUD.Content;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

using SL.ChatLinks.Storage;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public class AchievementGroupMenuItem
{
    public required AchievementGroup Group { get; set; }

    public required IEnumerable<AchievementCategory> Categories { get; set; }
}

public sealed class AchievementsTabViewModel(
    IEventAggregator eventAggregator,
    IDbContextFactory contextFactory,
    IStringLocalizer<AchievementsTabView> localizer,
    ILocale locale,
    AchievementTileViewModelFactory achievementTileViewModelFactory
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
                IQueryable<Achievement> search = context.Achievements
                    .Where(achievement => EF.Functions.Like(achievement.Name, $"%{query}%"));

                if (await search.AnyAsync().ConfigureAwait(false))
                {
                    List<AchievementCategory> categories = await context.AchievementCategories
                        .OrderBy(category => category.Order)
                        .ToListAsync()
                        .ConfigureAwait(false);

                    await foreach (Achievement achievement in search.AsAsyncEnumerable().ConfigureAwait(false))
                    {
                        AchievementCategory? category = categories
                            .FirstOrDefault(category => category.Achievements.Concat(category.Tomorrow ?? []).Any(reference => reference.Id == achievement.Id));

                        results.Add(achievementTileViewModelFactory.Create(achievement, category));
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
            IEnumerable<int> ids = category.Achievements.Select(r => r.Id);
            List<Achievement> achievements = await context.Achievements
                .Where(achievement => ids.Contains(achievement.Id))
                .OrderBy(achievement => achievement.Name)
                .ToListAsync()
                .ConfigureAwait(false);

            HeaderText = !string.IsNullOrEmpty(category.Name) ? category.Name : null;
            HeaderIcon = !string.IsNullOrEmpty(category.IconHref) ? GameService.Content.GetRenderServiceTexture(category.IconHref) : null;
            Achievements = [.. achievements.Select(achievement => achievementTileViewModelFactory.Create(achievement, category))];
        }
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}

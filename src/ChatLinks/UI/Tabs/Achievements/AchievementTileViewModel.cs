using System.Diagnostics;
using System.Net;

using Blish_HUD.Content;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;
using GuildWars2.Hero.Achievements.Groups;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Achievements.Tooltips;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementTileViewModel : ViewModel, IDisposable
{
    public delegate AchievementTileViewModel Factory(
        Achievement achievement,
        AchievementCategory? category,
        AchievementGroup? group,
        IReadOnlyList<AccountAchievement>? progression
    );

    private readonly IEventAggregator _eventAggregator;

    private readonly IStringLocalizer<AchievementTile> _localizer;

    private readonly IClipBoard _clipboard;

    private readonly IDbContextFactory _contextFactory;

    private readonly IconsService _icons;

    private readonly AchievementTooltipViewModel.Factory _achievementTooltipViewModelFactory;

    private Achievement _achievement;

    private AchievementCategory? _category;

    private IReadOnlyList<AccountAchievement>? _progression;

    private AchievementGroup? _group;

    public AchievementTileViewModel(
        IEventAggregator eventAggregator,
        IStringLocalizer<AchievementTile> localizer,
        IClipBoard clipboard,
        IDbContextFactory contextFactory,
        IconsService icons,
        AchievementTooltipViewModel.Factory achievementTooltipViewModelFactory,
        Achievement achievement,
        AchievementCategory? category,
        AchievementGroup? group,
        IReadOnlyList<AccountAchievement>? progression
)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _eventAggregator = eventAggregator;
        _localizer = localizer;
        _clipboard = clipboard;
        _contextFactory = contextFactory;
        _icons = icons;
        _achievementTooltipViewModelFactory = achievementTooltipViewModelFactory;
        _achievement = achievement;
        _category = category;
        _group = group;
        _progression = progression;
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);
    }

    private async Task OnLocaleChanged(LocaleChanged args)
    {
        OnPropertyChanged(nameof(CopyNameLabel));
        OnPropertyChanged(nameof(CopyChatLinkLabel));
        OnPropertyChanged(nameof(OpenWikiLabel));
        OnPropertyChanged(nameof(OpenApiLabel));

        ChatLinksContext context = _contextFactory.CreateDbContext(args.Language);
        await using (context.ConfigureAwait(false))
        {
            Achievement = context.Achievements.SingleOrDefault(achievement => achievement.Id == Achievement.Id);
        }
    }

    public string Name => Achievement.Name;

    public AsyncTexture2D? GetIcon()
    {
        return _icons.GetIcon(Achievement.IconUrl)
            ?? _icons.GetIcon(_category?.IconUrl);
    }

    public string CompletedLabel => _localizer["Completed"];

    public string ChatLink => Achievement.GetChatLink().ToString();

    public string CopyNameLabel => _localizer["Copy Name"];

    public RelayCommand CopyNameCommand => new(() => _clipboard.SetText(Achievement.Name));

    public string CopyChatLinkLabel => _localizer["Copy Chat Link"];

    public RelayCommand CopyChatLinkCommand => new(() => _clipboard.SetText(ChatLink));

    public string OpenWikiLabel => _localizer["Open Wiki"];

    public RelayCommand OpenWikiCommand => new(() => Process.Start(_localizer["Wiki search", WebUtility.UrlEncode(Name)]));

    public string OpenApiLabel => _localizer["Open API"];

    public RelayCommand OpenApiCommand =>
        new(() => Process.Start(_localizer["Achievement API", Achievement.Id]));

    public Achievement Achievement
    {
        get => _achievement;
        set => SetField(ref _achievement, value);
    }

    public AchievementCategory? Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public AchievementGroup? Group
    {
        get => _group;
        set => SetField(ref _group, value);
    }

    public IReadOnlyList<AccountAchievement>? Progression
    {
        get => _progression;
        set
        {
            if (SetField(ref _progression, value))
            {
                OnPropertyChanged(nameof(Progress));
            }
        }
    }

    public AccountAchievement? Progress => Progression
        .SingleOrDefault(accountAchievement => accountAchievement.Id == Achievement.Id);

    public bool Locked => Achievement.IsLocked(Group, Progression);

    public string AchievementProgressUnavailable => _localizer["Achievement progress unavailable"];

    public string PerCharacterAchievementProgressUnavailable => _localizer["Per-character achievement progress unavailable"];

    public bool IsPerCharacter => Group?.IsPerCharacter() ?? false;

    public bool IsDaily => Achievement.Flags.Daily;

    public bool IsWeekly => Achievement.Flags.Weekly;

    public bool IsMonthly => Achievement.Flags.Monthly;

    public string HiddenLabel => _localizer["Hidden achievement"];

    public AchievementTooltipViewModel CreateAchievementTooltipViewModel()
    {
        return _achievementTooltipViewModelFactory(Achievement);
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}

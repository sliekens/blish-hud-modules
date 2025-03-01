using System.Diagnostics;
using System.Net;

using GuildWars2.Hero.Achievements;
using GuildWars2.Hero.Achievements.Categories;

using Microsoft.Extensions.Localization;

using SL.ChatLinks.Storage;
using SL.ChatLinks.UI.Tabs.Achievements.Tooltips;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI.Tabs.Achievements;

public sealed class AchievementTileViewModel : ViewModel, IDisposable
{
    private readonly IEventAggregator _eventAggregator;

    private readonly IStringLocalizer<AchievementTile> _localizer;

    private readonly IClipBoard _clipboard;

    private readonly IDbContextFactory _contextFactory;

    private readonly AchievementTooltipViewModelFactory _achievementTooltipViewModelFactory;

    private Achievement _achievement;

    private AchievementCategory? _category;

    private AccountAchievement? _accountAchievement;

    public AchievementTileViewModel(
        IEventAggregator eventAggregator,
        IStringLocalizer<AchievementTile> localizer,
        IClipBoard clipboard,
        IDbContextFactory contextFactory,
        AchievementTooltipViewModelFactory achievementTooltipViewModelFactory,
        Achievement achievement
)
    {
        ThrowHelper.ThrowIfNull(eventAggregator);
        _eventAggregator = eventAggregator;
        _localizer = localizer;
        _clipboard = clipboard;
        _contextFactory = contextFactory;
        _achievementTooltipViewModelFactory = achievementTooltipViewModelFactory;
        _achievement = achievement;
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

    public string Description => Achievement.Description;

    public string IconHref
    {
        get
        {
            if (!string.IsNullOrEmpty(Achievement.IconHref))
            {
                return Achievement.IconHref;
            }

            if (!string.IsNullOrEmpty(_category?.IconHref))
            {
                return _category!.IconHref;
            }

            return string.Empty;
        }
    }

    public string CompletedLabel => _localizer["Completed"];

    public bool? Completed => _accountAchievement?.Done;

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

    public AccountAchievement? AccountAchievement
    {
        get => _accountAchievement;
        set
        {
            if (SetField(ref _accountAchievement, value))
            {
                OnPropertyChanged(nameof(Completed));
            }
        }
    }

    public AchievementTooltipViewModel CreateAchievementTooltipViewModel()
    {
        return _achievementTooltipViewModelFactory.Create(Achievement);
    }

    public void Dispose()
    {
        _eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using SL.ChatLinks.Storage;
using SL.Common.Controls;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI;

public sealed class MainIconViewModel(
    ILogger<MainIconViewModel> logger,
    IStringLocalizer<MainIcon> localizer,
    IEventAggregator eventAggregator,
    ILocale locale,
    DatabaseSeeder seeder,
    ModuleSettings settings
) : ViewModel, IDisposable
{
    private string? _loadingMessage;

    private string? _tooltipText;

    private bool _raiseStackSize = settings.RaiseStackSize;

    private bool _bananaMode = settings.BananaMode;

    private event EventHandler? DatabaseUpdated;

    public void Initialize()
    {
        eventAggregator.Subscribe<DatabaseSyncProgress>(OnDatabaseSyncProgress);
        eventAggregator.Subscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        eventAggregator.Subscribe<LocaleChanged>(OnLocaleChanged);

        _ = ChangeToken.OnChange(settings.GetChangeToken, moduleSettings =>
        {
            BananaMode = moduleSettings.BananaMode;
            RaiseStackSize = moduleSettings.RaiseStackSize;
        }, settings);
    }

    public string Name => localizer["Name"];

    public string KoFiLabel => localizer["Buy me a coffee"];

    public string SyncLabel => localizer["Sync database"];

    public string BananaModeLabel => localizer["Banana of Imagination-mode"];

    public string RaiseStackSizeLabel => localizer["Raise stack size limit"];

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public AsyncTexture2D Texture => AsyncTexture2D.FromAssetId(155156);

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public AsyncTexture2D HoverTexture => AsyncTexture2D.FromAssetId(155157);

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public int Priority => 745727698;

    public string? LoadingMessage
    {
        get => _loadingMessage;
        set => SetField(ref _loadingMessage, value);
    }

    public string? TooltipText
    {
        get => _tooltipText;
        set => SetField(ref _tooltipText, value);
    }

    public bool RaiseStackSize
    {
        get => _raiseStackSize;
        set
        {
            if (SetField(ref _raiseStackSize, value))
            {
                settings.RaiseStackSize = value;
            }
        }
    }

    public bool BananaMode
    {
        get => _bananaMode;
        set
        {
            if (SetField(ref _bananaMode, value))
            {
                settings.BananaMode = value;
            }
        }
    }

    public AsyncRelayCommand ClickCommand => new(async () =>
    {
        await eventAggregator.PublishAsync(new MainIconClicked(), CancellationToken.None).ConfigureAwait(false);
    });

    public AsyncRelayCommand SyncCommand => new(
        async () =>
        {
            try
            {
                await Task.Run(async () =>
                {
                    await seeder.Sync(locale.Current, CancellationToken.None).ConfigureAwait(false);
                    await seeder.Vacuum(locale.Current, CancellationToken.None).ConfigureAwait(false);
                    await seeder.Optimize(locale.Current, CancellationToken.None).ConfigureAwait(false);
                }).ConfigureAwait(false);

                ScreenNotification.ShowNotification(localizer["Chat Links database is up-to-date"], ScreenNotification.NotificationType.Green);
            }
            catch (Exception reason)
            {
                logger.LogError(reason, "Sync failed");
                ScreenNotification.ShowNotification(localizer["Sync failed"],
                    ScreenNotification.NotificationType.Warning);
            }
        },
        () => !seeder.IsSynchronizing,
        handler =>
        {
            DatabaseUpdated += handler;
        },
        handler =>
        {
            DatabaseUpdated -= handler;
        });

    private void OnDatabaseSyncProgress(DatabaseSyncProgress args)
    {
        LocalizedString step = localizer[args.Step];

        LoadingMessage = localizer["Downloading", step, args.Report.ResultCount, args.Report.ResultTotal];
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnDatabaseSyncCompleted(DatabaseSyncCompleted args)
    {
        LoadingMessage = null;
        TooltipText = null;
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void OnLocaleChanged(LocaleChanged changed)
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(KoFiLabel));
        OnPropertyChanged(nameof(SyncLabel));
        OnPropertyChanged(nameof(BananaModeLabel));
        OnPropertyChanged(nameof(RaiseStackSizeLabel));
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public RelayCommand KoFiCommand => new(() =>
    {
        _ = Process.Start("https://ko-fi.com/sliekens");
    });

    public IEnumerable<ContextMenuStripItem> ContextMenuItems()
    {
        ContextMenuStripItem bananaModeItem = new(BananaModeLabel)
        {
            CanCheck = true,
            Checked = BananaMode
        };
        bananaModeItem.CheckedChanged += (sender, args) =>
        {
            BananaMode = args.Checked;
        };

        ContextMenuStripItem raiseStackSizeItem = new(RaiseStackSizeLabel)
        {
            CanCheck = true,
            Checked = RaiseStackSize
        };
        raiseStackSizeItem.CheckedChanged += (sender, args) =>
        {
            RaiseStackSize = args.Checked;
        };

        ContextMenuStripItem syncItem = SyncCommand.ToMenuItem(() => SyncLabel);

        ContextMenuStripItem koFiItem = KoFiCommand.ToMenuItem(() => KoFiLabel);

        yield return bananaModeItem;
        yield return raiseStackSizeItem;
        yield return syncItem;
        yield return koFiItem;
    }

    public void Dispose()
    {
        eventAggregator.Unsubscribe<DatabaseSyncProgress>(OnDatabaseSyncProgress);
        eventAggregator.Unsubscribe<DatabaseSyncCompleted>(OnDatabaseSyncCompleted);
        eventAggregator.Unsubscribe<LocaleChanged>(OnLocaleChanged);
    }
}

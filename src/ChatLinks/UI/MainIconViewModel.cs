using System.Diagnostics;
using System.Globalization;

using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using SL.ChatLinks.Storage;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI;

public class MainIconViewModel(
    ILogger<MainIconViewModel> logger,
    IEventAggregator eventAggregator,
    ILocale locale,
    DatabaseSeeder seeder,
    ModuleSettings settings
) : ViewModel
{
    private string? _loadingMessage;

    private string? _tooltipText;

    private bool _raiseStackSize = settings.RaiseStackSize;

    private bool _bananaMode = settings.BananaMode;

    private event EventHandler? DatabaseUpdated;

    public void Initialize()
    {
        eventAggregator.Subscribe<DatabaseSyncProgress>(DatabaseSyncProgress);
        eventAggregator.Subscribe<DatabaseSyncCompleted>(DatabaseSyncCompleted);

        ChangeToken.OnChange(settings.GetChangeToken, moduleSettings =>
        {
            BananaMode = moduleSettings.BananaMode;
            RaiseStackSize = moduleSettings.RaiseStackSize;
        }, settings);
    }


    public string Name => """
                          Chat links
                          Right-click for options
                          """;

    public AsyncTexture2D Texture => AsyncTexture2D.FromAssetId(155156);

    public AsyncTexture2D HoverTexture => AsyncTexture2D.FromAssetId(155157);

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
        await eventAggregator.PublishAsync(new MainIconClicked(), CancellationToken.None);
    });

    public AsyncRelayCommand SyncCommand => new(
        async () =>
        {
            try
            {
                await await Task.Factory.StartNew(async () =>
                {
                    await seeder.Sync(locale.Current, CancellationToken.None);
                }, TaskCreationOptions.LongRunning);

                ScreenNotification.ShowNotification("Everything is up-to-date.", ScreenNotification.NotificationType.Green);
            }
            catch (Exception reason)
            {
                logger.LogError(reason, "Sync failed");
                ScreenNotification.ShowNotification("Sync failed, try again later.",
                    ScreenNotification.NotificationType.Warning);
            }
        },
        () => string.IsNullOrEmpty(LoadingMessage),
        handler =>
        {
            DatabaseUpdated += handler;
        },
        handler =>
        {
            DatabaseUpdated -= handler;
        });

    private void DatabaseSyncProgress(DatabaseSyncProgress args)
    {
        var step = args.Step switch
        {
            "items" => "items",
            "skins" => "skins",
            _ => args.Step
        };

        LoadingMessage = $"Synchronizing {step}... ({args.Report.ResultCount} of {args.Report.ResultTotal})";
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void DatabaseSyncCompleted(DatabaseSyncCompleted args)
    {
        LoadingMessage = null;
        TooltipText = null;
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }

    public RelayCommand KoFiCommand => new(() =>
    {
        Process.Start("https://ko-fi.com/sliekens");
    });
}
using Blish_HUD;
using Blish_HUD.Content;
using Blish_HUD.Controls;

using Microsoft.Extensions.Logging;

using SL.ChatLinks.Storage;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI;

public class MainIconViewModel(
    ILogger<MainIconViewModel> logger,
    IEventAggregator eventAggregator,
    ItemSeeder seeder
) : ViewModel
{
    private string? _loadingMessage;
    private string? _tooltipText;

    private event EventHandler? DatabaseUpdated;

    public void Initialize()
    {
        eventAggregator.Subscribe<DatabaseSyncProgress>(DatabaseSyncProgress);
        eventAggregator.Subscribe<DatabaseSyncCompleted>(DatabaseSyncCompleted);
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
                    await seeder.Seed(CancellationToken.None);
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
        LoadingMessage = $"Fetching items... ({args.Report.ResultCount} of {args.Report.ResultTotal})";
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void DatabaseSyncCompleted(DatabaseSyncCompleted args)
    {
        LoadingMessage = null;
        TooltipText = null;
        DatabaseUpdated?.Invoke(this, EventArgs.Empty);
    }
}
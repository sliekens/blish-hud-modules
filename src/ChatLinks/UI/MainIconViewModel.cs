using Blish_HUD.Content;

using SL.ChatLinks.Storage;
using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI;

public class MainIconViewModel(IEventAggregator eventAggregator) : ViewModel
{
    private string? _loadingMessage;

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

    public AsyncRelayCommand ClickCommand => new(async () =>
    {
        await eventAggregator.PublishAsync(new MainIconClicked(), CancellationToken.None);
    });

    private void DatabaseSyncProgress(DatabaseSyncProgress args)
    {
        LoadingMessage = $"Fetching items... ({args.Report.ResultCount} of {args.Report.ResultTotal})";
    }

    private void DatabaseSyncCompleted(DatabaseSyncCompleted args)
    {
        LoadingMessage = null;
    }
}
using Blish_HUD.Content;

using SL.Common;
using SL.Common.ModelBinding;

namespace SL.ChatLinks.UI;

public class MainIconViewModel(IEventAggregator eventAggregator) : ViewModel
{
    public string Name => """
        Chat links
        Right-click for options
        """;

    public AsyncTexture2D Texture => AsyncTexture2D.FromAssetId(155156);

    public AsyncTexture2D HoverTexture => AsyncTexture2D.FromAssetId(155157);

    public int Priority => 745727698;

    public AsyncRelayCommand ClickCommand => new(async () =>
    {
        await eventAggregator.PublishAsync(new MainIconClicked(), CancellationToken.None);
    });
}
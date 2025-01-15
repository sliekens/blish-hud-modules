using Blish_HUD.Settings;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using SL.ChatLinks.UI.Tabs.Items;

namespace SL.ChatLinks;

internal sealed class ChatLinkOptionsAdapter : IOptionsChangeTokenSource<ChatLinkOptions>
{
    private CancellationTokenSource _cts = new();

    public ChatLinkOptionsAdapter(SettingCollection settings)
    {
        if (settings.TryGetSetting("RaiseStackSize", out SettingEntry<bool> raiseStackSize))
        {
            raiseStackSize.SettingChanged += (sender, args) =>
            {
                var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                previousCts.Cancel();
                previousCts.Dispose();
            };
        }

        if (settings.TryGetSetting("BananaMode", out SettingEntry<bool> bananaMode))
        {
            bananaMode.SettingChanged += (sender, args) =>
            {
                var previousCts = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
                previousCts.Cancel();
                previousCts.Dispose();
            };
        }
    }

    public IChangeToken GetChangeToken()
    {
        return new CancellationChangeToken(_cts.Token);
    }

    public string Name => Options.DefaultName;
}